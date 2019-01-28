using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace klib
{
    public static class Shell
    {
        public const string LOG = "SHELL";
#region Report
        public static void Report(implement.LException lex)
        {
            var rep = new model.Report();
            rep.CID = "Undefined";
            rep.Id = lex.Code;
            rep.Message = lex.Message;
            rep.Details = ValuesEx.To(lex).ToJson();
            core.ReportSystem.Save(rep);
            core.ReportSystem.Send(rep);
        }

        public static void Report(string message)
        {
            var rep = new model.Report();
            rep.CID = "Undefined";
            rep.Id = 1;
            rep.Message = message;
            rep.Details = null;
            core.ReportSystem.Save(rep);
            core.ReportSystem.Send(rep);
        }
        #endregion

        #region Debug
        public static void WriteLine(int id, string log, Exception ex)
        {
            #region DEBUG
            if (log.Length > 6)
                log = log.Substring(0, 6);
            else if (log.Length < 6)
                log += new string(' ', 6 - log.Length);

            if (System.Environment.UserInteractive)

                Console.WriteLine($"ID{id.ToString("0000")} {DateTime.Now.ToString("HH:mm:ss")} ! {log.ToUpper()}: {ex.Message}");
            else
                Shell.Report(ex.Message);
            #endregion
        }

        public static void WriteLine(int id, string log, string message, bool clear = false)
        {
            if (log.Length > 6)
                log = log.Substring(0, 6);
            else if (log.Length < 6)
                log += new string(' ', 6 - log.Length);
            WriteLine(id, $"{log.ToUpper()}: {message}", clear);
        }

        [Obsolete]
        public static void WriteLine(int id, string message, bool clear = false)
        {
            #region DEBUG
            if (System.Environment.UserInteractive)
            {
                if (clear)
                    Console.Clear();

                Console.WriteLine($"ID{id.ToString("0000")} {DateTime.Now.ToString("HH:mm:ss")} . {message}");
            } else
            {
                Shell.Report(message);
            }
            #endregion
        }
        #endregion

        #region CronTab
        public static bool CronNow(string mask)
        {
            var cron = new core.Cron(mask);
            return cron.IsNow();
        }

        public static int CronWait(string mask)
        {
            var cron = new core.Cron(mask);
            return cron.Wait();
        }
        #endregion

        #region Encrypton
        /// <summary>
        /// Encrypto the string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Encrypt1(string input, string key)  
        {
            var byte24 = Md5Hash(key).Substring(0, 24);
            byte[] inputArray = UTF8Encoding.UTF8.GetBytes(input);  
            var tripleDES = new TripleDESCryptoServiceProvider();
            tripleDES.Key = UTF8Encoding.UTF8.GetBytes(byte24);  
            tripleDES.Mode = CipherMode.ECB;  
            tripleDES.Padding = PaddingMode.PKCS7;  
            var cTransform = tripleDES.CreateEncryptor();  
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);  
            tripleDES.Clear();  
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);  
        }
        
        /// <summary>
        /// Decrypto the string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Decrypt1(string input, string key)
        {
            var byte24 = Md5Hash(key).Substring(0, 24);
            byte[] inputArray = Convert.FromBase64String(input);
            var tripleDES = new TripleDESCryptoServiceProvider();
            tripleDES.Key = UTF8Encoding.UTF8.GetBytes(byte24);
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            var cTransform = tripleDES.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        public static string Md5Hash(string value)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(value));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

        /// <summary>
        /// Mixer the values
        /// </summary>
        /// <param name="def">Mixer default or random</param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string StringMixer(bool def, params object[] values)
        {
            var totalSize = 0;
            var biggerString = 0;
            var works = values.Length;
            var result = String.Empty;

            foreach (string value in values)
            {
                totalSize += value.Length;
                biggerString = biggerString < value.Length ? value.Length : biggerString;
            }

            for (int i = 0; i < biggerString; i++)
            {
                foreach (string value in values)
                {
                    if (value.Length >= (i + 1))
                        result = value[i] + result;
                }
            }

            if (result.Length == totalSize)
                return result;
            else
                throw new LException(1, "Error to mixer the strings");

        }
        #endregion

        #region IO
        /// <summary>
        /// Verify if the file is locked
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsFileLocked(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            return IsFileLocked(fileInfo);
        }
        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException ex)
            {
                klib.Shell.WriteLine(R.Project.ID, $"SHELL : File is lock. {ex.Message}");
                //o arquivo está indiposnível pelas seguintes causas:
                //está sendo escrito
                //utilizado por uma outra thread
                //não existe ou sendo criado
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //arquivo está disponível
            return false;
        }

        [Obsolete("Bug - Stream")]
        /// <summary>
        /// Save the stream in file
        /// </summary>
        /// <param name="file">Full file name</param>
        /// <param name="info">Information to be save/param>
        /// <param name="wait">Wait is the file unlocked?</param>
        public static void SaveInFile(FileInfo file, Stream info, bool wait = false)
        {
 
            try
            {

                while (wait == true && Shell.IsFileLocked(file))
                    System.Threading.Thread.Sleep(1500);

                using (var fileW = new System.IO.StreamWriter(file.FullName, true))
                {
                    fileW.WriteLine(info);
                    fileW.Close();
                }

            }
            finally
            {
        
            }
        }
        
        public static void SaveInFile(FileInfo file, string info, bool ovride = false, bool wait = false)
        {
            //byte[] byteArray = Encoding.UTF8.GetBytes(info);
            //SaveInFile(fileName, new MemoryStream(byteArray), wait);

            try
            {
                var exists = System.IO.File.Exists(file.FullName);
                while (wait == true && Shell.IsFileLocked(file) && exists)
                {
                    klib.Shell.WriteLine(R.Project.ID, $"SHELL : waiting the file is locked. {file.FullName}");
                    System.Threading.Thread.Sleep(1500);
                }

                if (ovride && exists)
                    System.IO.File.Delete(file.FullName);

                using (var fileW = new System.IO.StreamWriter(file.FullName, true))
                {
                    fileW.WriteLine(info);
                    fileW.Close();
                }

            }
            finally
            {

            }
        }

        public static System.IO.FileInfo CreateFile(byte[] bytes, string filename, bool replace = false)
        {
            if (bytes == null || bytes.Length < 1)
                throw new Exception("The bytes cannot be empty");

            return CreateFile(new MemoryStream(bytes), filename, replace);
        }

        public static FileInfo CreateTmpFile(string extension)
        {
            var name = DateTime.Now.ToString("yyyMMddhhmmssffff");

            return new FileInfo($"{TempDir()}/{name}.{extension}");
        }

        /// <summary>
        /// Create file in Temp directory application.
        /// </summary>
        /// <param name="stream">Data</param>
        /// <param name="filename">File name</param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public static System.IO.FileInfo CreateFile(Stream stream, string filename, bool replace = false)
        {
            if (stream == null)
                throw new Exception("The stream is not to be empty");


            if (System.IO.File.Exists(filename) && !replace)
                return new System.IO.FileInfo(filename);

            using (var output = new FileStream(filename, FileMode.OpenOrCreate))
            {
                stream.CopyTo(output);
                stream.Close();
            }

            return new System.IO.FileInfo(filename);
        }

        public static System.IO.FileInfo CreateFile(String val, string filename, bool replace = false)
        {
            if (System.IO.File.Exists(filename) && !replace)
                return new System.IO.FileInfo(filename);

            System.IO.File.WriteAllText(filename, val);

            return new System.IO.FileInfo(filename);
        }

        /// <summary>
        /// Get the user temp directory.
        /// </summary>
        /// <param name="createFolder"></param>
        /// <returns></returns>
        public static System.IO.DirectoryInfo TempDir(string createFolder = null)
        {
            var tempDir = System.IO.Path.GetTempPath();
            if (!String.IsNullOrEmpty(createFolder))
                System.IO.Directory.CreateDirectory($"{tempDir}/{createFolder}");

            return new System.IO.DirectoryInfo($"{tempDir}/{createFolder}");
        }

        public static FileInfo Mktemp(string ext = null)
        {
            String fileName;
            var random = System.IO.Path.GetRandomFileName();
            if (String.IsNullOrEmpty(ext))
                fileName = random;
            else
                fileName = $"{random.Split('.')[0]}.{ext}";

            var fileFull = System.IO.Path.Combine(TempDir("tmp").FullName, fileName);

            using (System.IO.File.Create(fileFull))
                return new FileInfo(fileFull);

        }
        
       public static void SendToFTP(Uri url, FileInfo file, klib.model.Credentials1 cred)
       {
            try
            {
                url = new Uri($"{url.AbsoluteUri}{file.Name}");
                var request = WebRequest.Create(url) as FtpWebRequest;
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(cred.User, cred.Passwd);
                request.UseBinary = true;
                request.ContentLength = file.Length;

                using (var resp = (FtpWebResponse)request.GetResponse())
                     klib.Shell.WriteLine(R.Project.ID, LOG, $"FTP response {resp.StatusCode}");
                

                using (FileStream fs = file.OpenRead())
                {
                    byte[] buffer = new byte[2048];
                    int bytesSent = 0;
                    int bytes = 0;

                    using (var stream = request.GetRequestStream())
                    {
                        while (bytesSent < file.Length)
                        {
                            bytes = fs.Read(buffer, 0, buffer.Length);
                            stream.Write(buffer, 0, bytes);
                            bytesSent += bytes;
                        }
                    }
                }
            }catch(Exception ex)
            {
                WriteLine(R.Project.ID, LOG, $"Error trying to send the {file.FullName} in {url.AbsoluteUri}. {ex.Message}");
                throw ex;
            }
        }
        #endregion

        #region Printer
        public static bool Printer(model.Printer printer, FileInfo file)
        {
            // Open the file.
            var fs = new FileStream(file.FullName, FileMode.Open);
            var br = new BinaryReader(fs);
            // Dim an array of bytes big enough to hold the file's contents.
            var bytes = new Byte[fs.Length];
            var bSuccess = false;
            // Your unmanaged pointer.
            var pUnmanagedBytes = new IntPtr(0);
            int nLength;

            nLength = Convert.ToInt32(fs.Length);
            // Read the contents of the file into the array.
            bytes = br.ReadBytes(nLength);
            // Allocate some unmanaged memory for those bytes.
            pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
            // Copy the managed byte array into the unmanaged array.
            Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
            // Send the unmanaged bytes to the printer.
            bSuccess = core.PrinterHelper.SendBytesToPrinter(printer.Name, pUnmanagedBytes, nLength);
            // Free the unmanaged memory that you allocated earlier.
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            return bSuccess;
        }
        public static bool Printer(model.Printer printer, string value)
        {
            IntPtr pBytes;
            Int32 dwCount;
            var bSucess = false;
            // How many characters are in the string?
            dwCount = value.Length;
            // Assume that the printer is expecting ANSI text, and then convert
            // the string to ANSI text.
            pBytes = Marshal.StringToCoTaskMemAnsi(value);
            // Send the converted ANSI string to the printer.
            bSucess = core.PrinterHelper.SendBytesToPrinter(printer.Name, pBytes, dwCount);
            Marshal.FreeCoTaskMem(pBytes);
            return bSucess;
        }
        #endregion
    }
}
