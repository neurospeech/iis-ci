using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISCI
{
    public class HashService
    {

        public static HashService Instance = new HashService();


        public string ComputeHash(string filePath) {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var fs = File.OpenRead(filePath))
                {
                    return Convert.ToBase64String(md5.ComputeHash(fs));
                }
            }

        }

    }
}
