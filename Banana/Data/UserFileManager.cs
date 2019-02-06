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
    }
}
