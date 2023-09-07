using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ImageMigration
{
    public class FileHelper : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool LogonUser(string pszUsername, string pszDomain, string pszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        extern static bool CloseHandle(IntPtr handle);

        [DllImport("Advapi32.DLL")]
        static extern bool ImpersonateLoggedOnUser(IntPtr hToken);
        [DllImport("Advapi32.DLL")]
        static extern bool RevertToSelf();
        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_LOGON_NEWCREDENTIALS = 9;//域控中的需要用:Interactive = 2
        const string userName = "Udpuser";
        const string passWord = "Udplogin@123";
        const string ip = "10.3.14.16";
        private bool disposed;

        public FileHelper(string username, string password, string ip)
        {
            IntPtr pExistingTokenHandle = new IntPtr(0);
            IntPtr pDuplicateTokenHandle = new IntPtr(0);

            try
            {
                bool bImpersonated = LogonUser(username, ip, password,
                    LOGON32_LOGON_NEWCREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref pExistingTokenHandle);

                if (bImpersonated)
                {
                    if (!ImpersonateLoggedOnUser(pExistingTokenHandle))
                    {
                        int nErrorCode = Marshal.GetLastWin32Error();
                        throw new Exception("ImpersonateLoggedOnUser error;Code=" + nErrorCode);
                    }
                }
                else
                {
                    int nErrorCode = Marshal.GetLastWin32Error();
                    throw new Exception("LogonUser error;Code=" + nErrorCode);
                }
            }
            finally
            {
                if (pExistingTokenHandle != IntPtr.Zero)
                    CloseHandle(pExistingTokenHandle);
                if (pDuplicateTokenHandle != IntPtr.Zero)
                    CloseHandle(pDuplicateTokenHandle);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                RevertToSelf();
                disposed = true;
            }
        }
        /// <summary>
        /// 获取指定目录下最新图片名称
        /// </summary>
        /// <param name="selectPath"></param>
        /// <returns></returns>
        public static string GetCSVFilePath(string selectPath)
        {
            try
            {
                using (FileHelper tool = new FileHelper(userName, passWord, ip))
                {
                    var dicInfo = new DirectoryInfo(selectPath);//选择的目录信息
                    List<FileInfo> files = dicInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly).OrderByDescending(t => t.LastWriteTime).ToList();
                    string file = files.First()?.FullName;
                    return file.Substring(13).Replace(@"\", @"/");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        /// <summary>
        /// 获取指定目录取最新图片并移走
        /// </summary>
        /// <param name="selectPath"></param>
        /// <returns></returns>
        public static string GetCSVFilePathMove(string selectPath)
        {
            try
            {
                using (FileHelper tool = new FileHelper(userName, passWord, ip))
                {
                    string newPath = selectPath + "\\" + DateTime.Now.ToString("yyyyMMdd");
                    var dicInfo = new DirectoryInfo(selectPath);//选择的目录信息
                    List<FileInfo> files = dicInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly).OrderByDescending(t => t.LastWriteTime).ToList();
                    if (files.Count == 0)
                    {
                        return "";
                    }
                    string file = files.First()?.FullName;
                    string name = file.Substring(file.LastIndexOf("\\") + 1);
                    string newFile = newPath + "\\" + name;
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }
                    if (!File.Exists(newFile))
                    {
                        File.Move(file, newFile);
                    }
                    string returnPath = newFile.Substring(13).Replace(@"\", @"/");
                    return returnPath;
                }
            }
            catch
            {
                return "";
            }
        }
        /// <summary>
        /// 异步获取图片
        /// </summary>
        /// <param name="ip">目录</param>
        /// <param name="fileNames">图片名称</param>
        /// <param name="millisecondsDelay">延时时长</param>
        /// <returns></returns>
        public static async Task<string> GetFilePathMove(string ip, List<string> fileNames, int millisecondsDelay = 1000)
        {
            await Task.Run(() =>
            {
                Thread.Sleep(millisecondsDelay);
                try
                {
                    using (FileHelper tool = new FileHelper(userName, passWord, ip))
                    {
                        string filePath = Path.Combine("\\\\" + ip, fileNames[0]);
                        string newPath = filePath.Substring(0, filePath.LastIndexOf("\\"));
                        var dicInfo = new DirectoryInfo(newPath.Substring(0, newPath.LastIndexOf("\\")));//选择的目录信息
                        List<FileInfo> files = dicInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly).OrderByDescending(t => t.LastWriteTime).ToList();
                        if (files.Count == 0)
                        {
                            return;
                        }
                        if (!Directory.Exists(newPath))
                        {
                            Directory.CreateDirectory(newPath);
                        }
                        for (int i = 0; i < fileNames.Count; i++)
                        {
                            if (i < files.Count)
                            {
                                string newFile = Path.Combine("\\\\" + ip, fileNames[i]);
                                if (!File.Exists(newFile))
                                {
                                    File.Move(files[i].FullName, newFile);
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
            });
            return "";
        }
        /// <summary>
        /// 根据文件名获取图片
        /// </summary>
        /// <param name="Addr"></param>
        /// <param name="PicName"></param>
        /// <returns></returns>
        public static string GetPicPathMove(string Addr, string PicName)
        {
            var ftpAddr = "//70.18.40.2/";
            string dayDir = DateTime.Now.ToString("yyyyMMdd/");
            string yesterdayDir = DateTime.Now.AddDays(-1).ToString("yyyyMMdd/");
            string oldFile = ftpAddr + Addr + PicName;
            string newFile = Addr + dayDir + PicName;
            string newDir = ftpAddr + Addr + dayDir;
            try
            {
                using (FileHelper tool = new FileHelper("user", "tztek", "70.18.40.2"))
                {
                    if (!Directory.Exists(newDir))
                    {
                        Directory.CreateDirectory(newDir);
                    }
                    if (!File.Exists(oldFile))
                    {
                        //当天yyyyMMdd文件夹
                        if (File.Exists(ftpAddr + newFile))
                        {
                            return newFile;
                        }
                        //昨天yyyyMMdd文件夹
                        if (File.Exists(ftpAddr + Addr + yesterdayDir + PicName))
                        {
                            return Addr + yesterdayDir + PicName;
                        }
                        return "";
                    }
                    if (!File.Exists(ftpAddr + newFile))
                    {
                        File.Move(oldFile, ftpAddr + newFile);
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
            return newFile;
        }
        /// <summary>
        /// 删除指定目录下图片
        /// </summary>
        /// <param name="selectPath"></param>
        /// <returns></returns>
        public static void CleanPics(string selectPath)
        {
            using (FileHelper tool = new FileHelper("user", "tztek", "192.168.51.6"))
            {
                var dicInfo = new DirectoryInfo(selectPath);//选择的目录信息
                List<FileInfo> files = dicInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly).ToList();
                foreach (var f in files)
                {
                    File.Delete(f.FullName);
                }
            }
        }
        /// <summary>
        /// 清除指定目录下文件
        /// </summary>
        /// <param name="selectPaths"></param>
        public static void CleanPics(List<string> selectPaths)
        {
            using (FileHelper tool = new FileHelper("user", "tztek", "192.168.51.6"))
            {
                foreach (string selectPath in selectPaths)
                {
                    var dicInfo = new DirectoryInfo(selectPath);//选择的目录信息
                    List<FileInfo> files = dicInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly).ToList();
                    string newPath = Path.Combine(selectPath, DateTime.Now.ToString("yyyyMMdd"), "Delete");
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }
                    foreach (var f in files)
                    {
                        if (File.Exists(Path.Combine(newPath, f.Name)))
                        {
                            File.Delete(Path.Combine(newPath, f.Name));
                        }
                        f.MoveTo(Path.Combine(newPath, f.Name));
                    }
                }
            }
        }

        /// <summary>
        /// 异步方法，将指定共享文件夹中的文件复制到另一个共享文件夹中
        /// </summary>
        /// <param name="sourceFolderPath">源文件夹路径</param>
        /// <param name="targetFolderPath">目标文件夹路径</param>
        public static async Task<string> CopyFileAsync(string sourceFilePath, string targetFilePath)
        {
            try
            {
                using (FileHelper tool = new FileHelper("Udpuser", "Udplogin@123", "10.3.14.16"))
                {
                    try
                    {
                        if (!File.Exists(sourceFilePath))
                        {
                            return $"Source file not found. {sourceFilePath}";
                        }
                        string targetDirectory = Path.GetDirectoryName(targetFilePath);
                        if (!Directory.Exists(targetDirectory))
                        {
                            Directory.CreateDirectory(targetDirectory);
                        }

                        using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                        using (FileStream targetStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                        {
                            await sourceStream.CopyToAsync(targetStream);
                        }
                        return string.Empty;
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"时间:{DateTime.Now}\r\n {ex.Message} ");
                        return ex.Message;
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error($"时间:{DateTime.Now}\r\n {ex.Message} ");
                return ex.Message;
            }
        }

        /// <summary>
        /// 异步方法，复制文件到目标文件夹
        /// </summary>
        /// <param name="sourceFilePath">源文件路径</param>
        /// <param name="targetFilePath">目标文件路径</param>
        //public static async Task CopyFileAsync(string sourceFilePath, string targetFilePath)
        //{
        //    // 使用异步方式复制文件
        //    using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
        //    using (FileStream targetStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
        //    {
        //        await sourceStream.CopyToAsync(targetStream);
        //    }
        //}

        /// <summary>
        /// 异步方法，剪切文件到目标文件夹
        /// </summary>
        /// <param name="sourceFilePath">源文件路径</param>
        /// <param name="targetFilePath">目标文件路径</param>

        public static async Task<string> MoveFileAsync(string sourceFilePath, string targetFilePath)
        {
            try
            {
                if (!File.Exists(sourceFilePath))
                {
                    throw new FileNotFoundException("Source file not found.", sourceFilePath);
                }

                string targetDirectory = Path.GetDirectoryName(targetFilePath)?.Trim();
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }
                // 使用异步方式剪切文件
                using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                using (FileStream targetStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    await sourceStream.CopyToAsync(targetStream);
                }

                // 删除原始文件
                File.Delete(sourceFilePath);
                string resultRes = $"原路径:{sourceFilePath} 成功剪切到：{targetFilePath}";
                logger.Info(resultRes);
                return resultRes;
            }
            catch (FileNotFoundException ex)
            {
                logger.Error($"This is an error message:{DateTime.Now} \r\n{ex.Message}");
                return ex.Message;
            }
            catch (Exception ex)
            {
                logger.Error($"This is an error message:{DateTime.Now} \r\n{ex.Message}");
                return ex.Message;
            }
        }

        /// <summary>
        /// 异步方法，剪切文件到目标文件夹
        /// </summary>
        /// <param name="sourceFilePath">源文件路径</param>
        /// <param name="targetFilePath">目标文件路径</param>

        public static async Task<string> MoveFileAsync(string sourceFilePath, string targetFilePath1, string targetFilePath2)
        {
            try
            {
                if (!File.Exists(sourceFilePath))
                {
                    throw new FileNotFoundException("Source file not found.", sourceFilePath);
                }

                string targetDirectory1 = Path.GetDirectoryName(targetFilePath1);
                if (!Directory.Exists(targetDirectory1))
                {
                    Directory.CreateDirectory(targetDirectory1);
                }
                string targetDirectory2 = Path.GetDirectoryName(targetFilePath2);
                if (!Directory.Exists(targetDirectory2))
                {
                    Directory.CreateDirectory(targetDirectory2);
                }
                // 使用异步方式剪切文件
                using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                using (FileStream targetStream1 = new FileStream(targetFilePath1, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                using (FileStream targetStream2 = new FileStream(targetFilePath2, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    await sourceStream.CopyToAsync(targetStream1);
                    await sourceStream.CopyToAsync(targetStream2);
                }

                // 删除原始文件
                File.Delete(sourceFilePath);
                return string.Empty;
            }
            catch (FileNotFoundException ex)
            {
                logger.Error($"This is an error message:{DateTime.Now} \r\n{ex.Message}");
                return ex.Message;
            }
            catch (Exception ex)
            {
                logger.Error($"This is an error message:{DateTime.Now} \r\n{ex.Message}");
                return ex.Message;
            }
        }

        public async static Task CleanFullNames(List<string> fullName)
        {
            //using (FileHelper tool = new FileHelper("user", "tztek", "."))
            //{
            bool a = File.Exists(fullName[0]);
            var filesToDelete = fullName.AsParallel().Where(file => File.Exists(file)).ToList();

            Parallel.ForEach(filesToDelete, file =>
            {
                File.Delete(file);
            });
            //}
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
