using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace klib
{
    public static class Shell
    {
        public static void WriteLine(string message)
        {
            #region DEBUG
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " - " + message);
            #endregion
        }
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
            catch (IOException)
            {
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
        /// <param name="fileName">Full file name</param>
        /// <param name="info">Information to be save/param>
        /// <param name="wait">Wait is the file unlocked?</param>
        public static void SaveInFile(string fileName, Stream info, bool wait = false)
        {
 
            try
            {

                while (wait == true && Shell.IsFileLocked(fileName))
                    System.Threading.Thread.Sleep(1500);

                using (var file = new System.IO.StreamWriter(fileName, true))
                {
                    file.WriteLine(info);
                    file.Close();
                }

            }
            finally
            {
        
            }
        }

        
        public static void SaveInFile(string fileName, string info, bool ovride = false, bool wait = false)
        {
            //byte[] byteArray = Encoding.UTF8.GetBytes(info);
            //SaveInFile(fileName, new MemoryStream(byteArray), wait);

            try
            {

                while (wait == true && Shell.IsFileLocked(fileName))
                    System.Threading.Thread.Sleep(1500);

                if (ovride && System.IO.File.Exists(fileName))
                    System.IO.File.Delete(fileName);

                using (var file = new System.IO.StreamWriter(fileName, true))
                {
                    file.WriteLine(info);
                    file.Close();
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

           for(int i = 0; i < biggerString; i++)
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
    }
}
