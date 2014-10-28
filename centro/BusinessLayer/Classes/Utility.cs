using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Configuration;
using System.Net;
using System.IO;
using System.Drawing;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using BusinessLayer.Models.DataModel;

namespace BusinessLayer.Classes
{
    public class GroupByDate
    {
        public DateTime CreatedOn { get; set; }        
    }

    public static class Utility
    {
        // This constant string is used as a "salt" value for the PasswordDeriveBytes function calls.
        // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
        // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
        private const string initVector = "tu89geji340t89u2";

        // This constant is used to determine the keysize of the encryption algorithm.
        private const int keysize = 256;

        /// <summary>
        /// Create the Bitmap by from stream
        /// </summary>
        /// <param name="original_image"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static Bitmap GetThumbnail(Stream original_image, int height, int width)
        {
            return GetThumbnail(new Bitmap(original_image), height, width);
        }

        /// <summary>
        /// Create the Bitmap by from Bitmap
        /// </summary>
        /// <param name="original_image"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static Bitmap GetThumbnail(Bitmap original_image, int height, int width)
        {
            Bitmap new_thumb = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format64bppPArgb);

            Graphics new_graphic_thumb = Graphics.FromImage(new_thumb);
            new_graphic_thumb.Clear(Color.White);

            float start_x = 0;
            float start_y = 0;
            float process_width = 0;
            float process_height = 0;

            if (original_image.Width > original_image.Height)
            {
                process_height = (float)original_image.Height;
                process_width = ((float)width / (float)height) * process_height;
                start_x = (original_image.Width / 2) - (process_width / 2);
            }
            else
            {
                process_width = (float)original_image.Width;
                process_height = process_width / ((float)width / (float)height);
                start_y = (original_image.Height / 2) - (process_height / 2);
            }

            new_graphic_thumb.DrawImage(original_image, new Rectangle(0, 0, width, height), (int)start_x, (int)start_y, process_width, process_height, GraphicsUnit.Pixel);
            return new_thumb;
        }

        /// <summary>
        /// Trim white spaces from object values
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T TrimSpaces<T>(this T obj)
        {
            var str_properties = obj.GetType().GetProperties().Where(x => x.PropertyType == typeof(string));
            foreach (var item in str_properties)
            {
                var value = item.GetValue(obj, null);
                if (value != null) item.SetValue(obj, value.ToString().Trim(), null);
            }

            return obj;
        }

        /// <summary>
        /// get configuration value by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ReadConfig(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// Get btyes of a file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static byte[] ReadFileBytes(string file)
        {
            return System.IO.File.ReadAllBytes(HttpContext.Current.Server.MapPath(file));
        }

        /// <summary>
        /// Get context of a file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string ReadFileText(string file)
        {
            return File.ReadAllText(HttpContext.Current.Server.MapPath(file));
        }

        public static void WriteFile(string file, string content)
        {
            System.IO.StreamWriter sw = null;
            sw = new StreamWriter(HttpContext.Current.Server.MapPath(file), true);
            sw.WriteLine(content);
            sw.Close();
        }

        /// <summary>
        /// Write the string into file after specified index
        /// </summary>
        /// <param name="file"></param>
        /// <param name="index"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static void WriteFile(string file, int index, string content)
        {
            string data = ReadFileText(file);
            data = data.Substring(0, index) + content;
            File.WriteAllText(HttpContext.Current.Server.MapPath(file), data, Encoding.UTF8);
        }

        /// <summary>
        /// Write the byte array into file after specified index
        /// </summary>
        /// <param name="file"></param>
        /// <param name="index"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static void WriteFile(string file, int index, byte[] bytes)
        {
            using (Stream stream = new FileStream(file, FileMode.OpenOrCreate))
            {
                stream.Seek(1000, SeekOrigin.Begin);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        /// <summary>
        /// Move file from source to destination
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static void MoveFile(string source, string destinationFolder, string filename)
        {
            if (!System.IO.Directory.Exists(HttpContext.Current.Server.MapPath(destinationFolder)))
                System.IO.Directory.CreateDirectory(HttpContext.Current.Server.MapPath(destinationFolder));
            File.Move(HttpContext.Current.Server.MapPath(source), HttpContext.Current.Server.MapPath(destinationFolder + filename));
        }

        /// <summary>
        /// Convert the string into byte array
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// Convert the byte array into string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        /// <summary>
        /// Delete a file from HDD
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static void DeleteFile(string path)
        {
            if (File.Exists(HttpContext.Current.Server.MapPath(path)))
            {
                System.IO.File.Delete(HttpContext.Current.Server.MapPath(path));
            }
        }

        /// <summary>
        /// Get sub domain from a URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetSubDomain(string url)
        {
            var index = url.IndexOf(".");

            if (index < 0) return "";
            else return url.Split('.')[0];
        }

        /// <summary>
        /// Generate a random string of specifies length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetRandomString(int length)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < length; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Replace null string with emtpy string
        /// </summary>
        /// <param name="str"></param>        
        /// <returns></returns>
        public static string ToValidString(this string str)
        {
            return string.IsNullOrEmpty(str) ? "" : str;
        }

        /// <summary>
        /// Create MD5 hash of a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CreateMD5Hash(string input)
        {
            // Use input string to calculate MD5 hash
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
                // To force the hex string to lower-case letters instead of
                // upper-case, use he following line instead:
                // sb.Append(hashBytes[i].ToString("x2")); 
            }
            return sb.ToString();
        }

        /// <summary>
        /// Encrypt a string
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        public static string EncryptString(string Message)
        {
            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Config.EncryptionPassword));
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;
            byte[] DataToEncrypt = UTF8.GetBytes(Message);
            try
            {
                ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
                Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
            }
            finally
            {
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }
            return Convert.ToBase64String(Results);
        }

        /// <summary>
        /// Decrypt a string
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        public static string DecryptString(string Message)
        {
            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Config.EncryptionPassword));
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;
            byte[] DataToDecrypt = Convert.FromBase64String(Message);
            try
            {
                ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
                Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
            }
            finally
            {
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }
            return UTF8.GetString(Results);
        }

        /// <summary>
        /// Get the readable value of a string
        /// </summary>
        /// <param name="pascalCaseString"></param>
        /// <returns></returns>
        public static string Wordify(string pascalCaseString)
        {
            Regex r = new Regex("(?<=[a-z])(?<x>[A-Z])|(?<=.)(?<x>[A-Z])(?=[a-z])");
            return r.Replace(pascalCaseString, " ${x}");
        }

        /// <summary>
        /// Decodes the string from base 64 to actual string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Decode(string str)
        {
            //TODO: Change the name of this function to some good one. This is not making sense.
            byte[] encodedDataAsBytes
          = System.Convert.FromBase64String(str);
            string returnValue =
               System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }

        /// <summary>
        /// Get the path
        /// </summary>
        /// <param name="member_id"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string GetPath(long member_id, string saperator = "\\")
        {
            var s = Convert.ToString(member_id, 16);
            s = "00000000".Substring(s.Length) + s;

            StringBuilder sb = new StringBuilder(s);
            sb.Insert(6, saperator);
            sb.Insert(4, saperator);
            sb.Insert(2, saperator);
            sb.Insert(0, saperator);

            return sb.ToString();
        }

        public static bool UniqueEmail(string email, int? currentUserid)
        {
            using (var context = new CentroEntities())
            {
                User user;
                if (currentUserid == null)
                    user = context.Users.Where(m => m.EmailId.Equals(email, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                else
                    user = context.Users.Where(m => m.EmailId.Equals(email, StringComparison.InvariantCultureIgnoreCase) && m.UserID != currentUserid.Value).FirstOrDefault();
                if (user != null)
                    return false;
                else
                    return true;
            }
        }

        public static bool UniqueUsername(string Username)
        {
            using (var context = new CentroEntities())
            {
                User user;
                user = context.Users.Where(m => m.UserName.Equals(Username, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (user != null)
                    return false;
                else
                    return true;
            }
        }

        public static bool UniqueShopname(string shopname, int user_id)
        {
            using (var context = new CentroEntities())
            {
                var query = context.Shops.Where(m => m.ShopName.Equals(shopname, StringComparison.InvariantCultureIgnoreCase) && m.UserId != user_id);
                if (query.Any())
                    return false;
                else
                    return true;
            }
        }

        public static bool UniqueCategoryname(string category_name)
        {
            using (var context = new CentroEntities())
            {
                var query = context.Categories.Where(m => m.Name.Equals(category_name, StringComparison.InvariantCultureIgnoreCase));
                if (query.Any())
                    return false;
                else
                    return true;
            }
        }

        //public decimal RoundDown(decimal i, double decimalPlaces)
        //{
        //    var power = Math.Pow(10, decimalPlaces);
        //    return Math.Floor(i * power) / power;
        //}

        public static decimal CustomRound(decimal x)
        {
            return decimal.Round(x - 0.001m, 3, MidpointRounding.AwayFromZero);
        }

        public static string SpacesToHifen(string url)
        {
            return url.Replace(" ", "-").Replace("/", "@").Replace("&", "__");
        }

        public static string HifenToSpace(string title)
        {
            return title.Replace("-", " ").Replace("@", "/").Replace("__", "&");
        }

        public static bool UniqueHubName(string hubname, int user_id,int? hub_id)
        {
            using (var context = new CentroEntities())
            {
                Hub obj;
                if (hub_id == null)
                {
                    obj = context.Hubs.Where(m => m.Title.Equals(hubname, StringComparison.InvariantCultureIgnoreCase) && m.UserID == user_id).FirstOrDefault();
                }
                else
                {
                    obj = context.Hubs.Where(m => m.Title.Equals(hubname, StringComparison.InvariantCultureIgnoreCase) && m.UserID == user_id && m.HubID!=hub_id).FirstOrDefault();
                }
                if (obj!=null)
                    return false;
                else
                    return true;
            }
        }

        public static IEnumerable<DirectoryInfo> EnumerateDirectories(DirectoryInfo dir, string target)
        {
            foreach (var di in dir.EnumerateDirectories(target, SearchOption.AllDirectories))
            {
                if ((di.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden && (di.Attributes & FileAttributes.System) != FileAttributes.System)
                {
                    if (di.Name.EndsWith(target, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return di;
                        continue;
                    }
                    foreach (var subDir in EnumerateDirectories(di, target))
                    {
                        yield return subDir;
                    }
                }
            }
        }
    }
}
