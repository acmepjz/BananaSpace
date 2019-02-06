﻿using Microsoft.AspNetCore.Http;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserCoursePagesItem>().HasKey("CourseId", "PageId");
            modelBuilder.Entity<UserRole>().HasKey("UserName");
        }

        public UserCourse GetCourse(int id)
        {
            return (from course in Courses
                    where course.Id == id
                    select course).FirstOrDefault();
        }

        public IEnumerable<UserCourse> GetAllCourses()
        {
            return from course in Courses
                   where course.Status == CourseStatus.Normal
                   orderby course.LastUpdatedDate descending
                   select course;
        }

        public IEnumerable<UserCourse> GetAllCourseRequests()
        {
            return from course in Courses
                   where course.Status == CourseStatus.Request
                   orderby course.CreationDate descending
                   select course;
        }

        public UserPage GetPage(int id)
        {
            return (from page in Pages
                    where page.Id == id
                    select page).FirstOrDefault();
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
                   orderby course.CreationDate descending
                   select course;
        }

        public IEnumerable<UserCourse> GetCoursesByFollower(string user)
        {
            return null;
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

        public bool UserCanEdit(string userName, UserPage page)
        {
            return GetCourse(page.CourseId).Creator == userName || UserIsAdmin(userName);
        }

        public void NewCourseRequest(string title, string type, string userName)
        {
            int id = NewId();
            var course = new UserCourse
            {
                Id = id,
                Title = title,
                Type = type,
                Creator = userName,
                MainPageId = id,
                Status = CourseStatus.Request,
                CreationDate = DateTime.Now,
                LastUpdatedDate = DateTime.Now,
                LastUpdatedPageId = id
            };

            Courses.Add(course);
            SaveChanges();
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
            Remove(itemDict[page.Id]);
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
    }
}
