using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Banana.Data
{
    public class UserFileManager
    {
        public string UploadsPath { get; }

        public UserFileManager(IHostingEnvironment env)
        {
            UploadsPath = Path.Combine(env.WebRootPath, "uploads");
        }

        // returns the hashed name of the file, or null if failed
        public string AddFile(int pageId, IFormFile file)
        {
            try
            {
                var path = Path.Combine(UploadsPath, pageId.ToString());
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var ext = Path.GetExtension(file.FileName);
                var hashedName = Guid.NewGuid().ToString("N") + ext;
                using (var stream = File.Open(Path.Combine(path, hashedName), FileMode.Create))
                    file.CopyTo(stream);
                return hashedName;
            }
            catch { return null; }
        }

        public bool DeleteFile(int pageId, string hashedName)
        {
            try
            {
                var path = Path.Combine(UploadsPath, pageId.ToString());
                var fullName = Path.Combine(path, hashedName);
                if (!File.Exists(fullName))
                    return true;
                File.Delete(fullName);

                if (!Directory.EnumerateFiles(path).Any())
                    Directory.Delete(path);
                return true;
            }
            catch { return false; }
        }

        public bool CopyFiles(int from, int to, bool move)
        {
            try
            {
                string fromPath = Path.Combine(UploadsPath, from.ToString()),
                    toPath = Path.Combine(UploadsPath, to.ToString());
                if (Directory.Exists(toPath))
                    Directory.Delete(toPath, true);
                if (!Directory.Exists(fromPath))
                    return true;
                if (move)
                    Directory.Move(fromPath, toPath);
                else
                    CopyDirectoryFiles(fromPath, toPath);
                return true;
            }
            catch { return false; }
        }

        public bool DeleteFiles(int pageId)
        {
            try
            {
                string path = Path.Combine(UploadsPath, pageId.ToString());
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                return true;
            }
            catch { return false; }
        }

        private void CopyDirectoryFiles(string from, string to)
        {
            if (!Directory.Exists(to))
                Directory.CreateDirectory(to);
            var files = Directory.GetFiles(from);
            foreach (string file in files)
                File.Copy(file, file.Replace(from, to), true);
        }
    }
}
