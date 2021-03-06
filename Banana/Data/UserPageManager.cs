﻿using Banana.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Banana.Data
{
    public class UserPageManager : DbContext
    {
        private UserFileManager _fileManager;

        public UserPageManager(DbContextOptions<UserPageManager> options, UserFileManager fileManager) : base(options)
        {
            _fileManager = fileManager;
        }

        private DbSet<UserPage> Pages { get; set; }
        private DbSet<UserCourse> Courses { get; set; }
        private DbSet<UserCoursePagesItem> CoursePages { get; set; }
        private DbSet<UserLabel> Labels { get; set; }
        private DbSet<UserRole> UserRole { get; set; }
        private DbSet<UserAccessItem> UserAccess { get; set; }
        private DbSet<UserFavoritesItem> UserFavorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserCoursePagesItem>().HasKey("CourseId", "PageId");
            modelBuilder.Entity<UserAccessItem>().HasKey("UserName", "CourseId");
            modelBuilder.Entity<UserFavoritesItem>().HasKey("UserName", "CourseId");
            modelBuilder.Entity<UserRole>().HasKey("UserName");
        }

        public UserCourse GetCourse(int id)
        {
            return (from course in Courses
                    where course.Id == id
                    select course).FirstOrDefault();
        }

        public IEnumerable<UserCourse> GetAllCourses(Func<UserCourse, bool> selector = null)
        {
            return from course in Courses
                   where selector == null || selector(course)
                   orderby course.LastUpdatedDate descending
                   select course;
        }

        public UserPage GetPage(int id, bool allowDraft)
        {
            var p = (from page in Pages
                     where page.Id == id
                     select page).FirstOrDefault();
            if (!allowDraft && p?.IsDraft == true)
                return null;
            return p;
        }

        public IEnumerable<UserPage> GetAllPages(int courseId, Func<UserPage, bool> selector = null)
        {
            return from item in CoursePages
                   where item.CourseId == courseId
                   orderby item.PageIndex
                   join page in Pages on item.PageId equals page.Id
                   where selector == null || selector(page)
                   select page;
        }

        public IEnumerable<UserCourse> GetCoursesByCreator(string user)
        {
            return from course in Courses
                   where course.Creator == user
                   select course;
        }

        public IEnumerable<UserCourse> GetCoursesByFollower(string user)
        {
            return (from item in UserFavorites
                    where item.UserName == user
                    join course in Courses on item.CourseId equals course.Id
                    select course)
                   .Union(GetCoursesByCreator(user))
                   .OrderByDescending(course => course.LastUpdatedDate);
        }

        public bool UserIsFollower(string userName, int courseId)
        {
            return (from item in UserFavorites
                    where item.UserName == userName && item.CourseId == courseId
                    select item).Any();
        }

        public int NewId()
        {
            Random r = new Random();
            while (true)
            {
                int id = r.Next(10000000, 99999999);
                if (!Pages.Any(page => page.Id == id) && !Courses.Any(course => course.Id == id))
                    return id;
            }
        }

        public bool UserIsAdmin(string userName)
        {
            return (from role in UserRole
                    where role.UserName == userName && role.RoleId == "Administrator"
                    select role).Any();
        }

        public bool UserCanEdit(string userName, int courseId)
        {
            return GetCourse(courseId).Creator == userName || UserIsAdmin(userName);
        }

        public UserCourse NewCourse(string userName, string userEmail, string htmlTitle)
        {
            int id = NewId();
            var now = DateTime.Now;
            var course = new UserCourse
            {
                Id = id,
                Title = htmlTitle,
                Creator = userName,
                CreatorEmail = userEmail,
                MainPageId = id,
                CreationDate = now,
                LastUpdatedDate = now,
                LastUpdatedPageId = id
            };

            Courses.Add(course);
            return course;
        }

        public void AddPage(UserPage page, int index)
        {
            Add(page);

            Add(new UserCoursePagesItem
            {
                CourseId = page.CourseId,
                PageId = page.Id,
                PageIndex = index
            });

            var items = from item in CoursePages
                        where item.CourseId == page.CourseId && item.PageIndex >= index
                        select item;
            foreach (var item in items)
            {
                Update(item);
                item.PageIndex++;
            }
        }

        public void PublishPage(UserPage page, ExpansionData data)
        {
            var now = DateTime.Now;
            var course = GetCourse(page.CourseId);
            Update(page);
            page.LastModifiedDate = now;

            var draftPage = GetPage(page.DraftId ?? 0, true);
            if (draftPage != null)
                Remove(draftPage);
            page.DraftId = null;

            if (!page.IsPublic) // first publishing
            {
                page.IsPublic = true;
                page.CreationDate = page.LastModifiedDate;
                Update(course);
                course.LastUpdatedDate = now;
                course.LastUpdatedPageId = page.Id;
            }

            if (course.MainPageId == page.Id)
                course.Title = page.HtmlTitle;

            // update labels
            ClearLabels(page);
            foreach (var bookmark in data.Bookmarks)
            {
                string content = Parser.ToHtmlFinal(bookmark.Content, ParserOptions.SingleLine);
                AddLabel(page, bookmark.Label, content);
            }
        }

        public void DeletePage(UserPage page)
        {
            var course = GetCourse(page.CourseId);
            var pages = GetAllPages(course.Id).ToList();
            var items = (from item in CoursePages
                         where item.CourseId == course.Id
                         select item).ToList();
            var itemDict = new Dictionary<int, UserCoursePagesItem>();
            foreach (var item in items)
                itemDict.Add(item.PageId, item);

            int i = 0, // 0: before deleting; 1: deleting; 2: after deleting
                removedCount = 1;

            Remove(page);
            if (page.DraftId != null)
                Remove(GetPage(page.DraftId ?? -1, true));
            Remove(itemDict[page.Id]);
            _fileManager.DeleteFiles(page.Id);
            foreach (var p in pages)
            {
                if (i == 0)
                {
                    if (p.Id == page.Id) i = 1;
                }
                else if (i == 1)
                {
                    if (p.PageLevel <= page.PageLevel)
                        i = 2;
                    else
                    {
                        Remove(p);
                        Remove(itemDict[p.Id]);
                        _fileManager.DeleteFiles(p.Id);
                        removedCount++;
                    }
                }
                
                if (i == 2)
                {
                    var item = itemDict[p.Id];
                    Update(item);
                    item.PageIndex -= removedCount;
                }
            }
        }

        public void DeleteDraftPage(int draftId)
        {
            Remove(GetPage(draftId, true));
        }

        public void DeleteCourse(int courseId)
        {
            var pages = GetAllPages(courseId).ToList();
            var course = GetCourse(courseId);

            foreach (var page in pages)
            {
                Remove(page);
                if (page.DraftId != null)
                    Remove(GetPage(page.DraftId ?? -1, true));
                _fileManager.DeleteFiles(page.Id);
            }

            Remove(course);

            RemoveRange(from item in CoursePages
                        where item.CourseId == courseId
                        select item);

            RemoveRange(from item in UserFavorites
                        where item.CourseId == courseId
                        select item);

            RemoveRange(from item in UserAccess
                        where item.CourseId == courseId
                        select item);

            RemoveRange(from label in Labels
                        where label.CourseId == courseId
                        select label);
        }

        public void ClearLabels(UserPage page)
        {
            var result = from label in Labels
                         where label.PageId == page.Id
                         select label;
            Labels.RemoveRange(result);
        }

        public void AddLabel(UserPage page, string key, string content)
        {
            Labels.Add(new UserLabel
            {
                Id = Guid.NewGuid().ToString(),
                PageId = page.Id,
                CourseId = page.CourseId,
                Key = key,
                Content = content
            });
        }

        public IQueryable<UserLabel> GetAllLabels(int courseId)
        {
            return from label in Labels
                   where label.CourseId == courseId
                   select label;
        }

        public void AddToFavorites(string userName, int courseId)
        {
            if ((from item in UserFavorites
                 where item.UserName == userName && item.CourseId == courseId
                 select item).Any())
                return;
            UserFavorites.Add(new UserFavoritesItem
            {
                UserName = userName,
                CourseId = courseId
            });
        }

        public void CancelFavorites(string userName, int courseId)
        {
            var toRemove = from item in UserFavorites
                           where item.UserName == userName && item.CourseId == courseId
                           select item;
            foreach (var item in toRemove)
                Remove(item);
        }

        public void AddAccess(string userName, int courseId)
        {
            var result = from item in UserAccess
                         where item.UserName == userName && item.CourseId == courseId
                         select item;
            foreach (var item in result)
                if (item.HasAccess)
                    return;
            if (result.Any())
            {
                var item = result.Single();
                Update(item);
                item.HasAccess = true;
                item.AttemptCount = 0;
            }
            else
            {
                UserAccess.Add(new UserAccessItem
                {
                    UserName = userName,
                    CourseId = courseId,
                    HasAccess = true,
                    AttemptCount = 0
                });
            }
        }

        public void WrongPasswordAtAccess(string userName, int courseId)
        {
            var result = from item in UserAccess
                         where item.UserName == userName && item.CourseId == courseId
                         select item;
            if (result.Any())
            {
                var item = result.Single();
                Update(item);
                if (item.AttemptCount < 200)
                    item.AttemptCount++;
            }
            else
            {
                UserAccess.Add(new UserAccessItem
                {
                    UserName = userName,
                    CourseId = courseId,
                    HasAccess = false,
                    AttemptCount = 1
                });
            }
        }

        public bool IsBlocked(string userName, int courseId)
        {
            return (from item in UserAccess
                    where item.UserName == userName && item.CourseId == courseId && !item.HasAccess && item.AttemptCount >= 200
                    select item).Any();
        }

        public bool HasAccess(string userName, UserCourse course)
        {
            if (course.Creator == userName || UserIsAdmin(userName))
                return true;
            return (from item in UserAccess
                    where item.UserName == userName && item.CourseId == course.Id && item.HasAccess
                    select item).Any();
        }

        public void ResetAccess(int courseId)
        {
            UserAccess.RemoveRange(from item in UserAccess
                                   where item.CourseId == courseId
                                   select item);
        }

        public bool AddFile(UserPage page, IFormFile file)
        {
            string name = _fileManager.AddFile(page.Id, file);

            if (name == null)
                return false;

            Update(page);
            var files = page.GetFileList();
            if (files.ContainsKey(file.FileName))
            {
                if (!_fileManager.DeleteFile(page.Id, files[file.FileName]))
                    return false;
                files.Remove(file.FileName);
            }
            files.Add(file.FileName, name);
            page.SaveFilesString(files);
            SaveChanges();
            return true;
        }

        // returns false if file is not found
        // returns null if file operation failed
        public bool? DeleteFile(UserPage page, string fileName)
        {
            if (fileName == null)
                return false;

            var files = page.GetFileList();
            if (!files.TryGetValue(fileName, out string name))
                return false;

            if (!_fileManager.DeleteFile(page.Id, name))
                return null;

            Update(page);
            files.Remove(fileName);
            page.SaveFilesString(files);
            SaveChanges();
            return true;
        }

        public string GetFileUrl(UserPage page, string fileName)
        {
            var dict = page.GetFileList();
            if (dict.TryGetValue(fileName, out string result))
                return $"~/uploads/{page.Id}/{result}";
            else
                return null;
        }

        public bool CopyFiles(int from, int to)
        {
            return _fileManager.CopyFiles(from, to, false);
        }

        public bool MoveFiles(int from, int to)
        {
            return _fileManager.CopyFiles(from, to, true);
        }
    }
}
