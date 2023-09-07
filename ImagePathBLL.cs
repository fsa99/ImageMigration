using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMigration
{
    public class ImagePathBLL
    {
        private Logger logger;

        List<ImagePathsMappingModel> imagePaths;

        public ImagePathBLL(Logger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// 批量上传图片
        /// </summary>
        /// <returns></returns>
        public async Task BatchUploadPictures()
        {
            this.imagePaths = GetImagePathsMappings();

            List<Task<string>> uploadTasks = new List<Task<string>>();

            foreach (var img in imagePaths)
            {
                Task<string> uploadTask = UploadImagesToTPAsync(img);
                uploadTasks.Add(uploadTask);
                // Start processing the task result as soon as it's completed
                _ = ProcessUploadResult(uploadTask);
            }

            // Wait for all tasks to complete
            await Task.WhenAll(uploadTasks);

            // Batch processing completed
            Console.WriteLine("Batch processing completed.");
        }

        private async Task ProcessUploadResult(Task<string> task)
        {
            string result = await task;
            Console.WriteLine(result);
        }


        public List<ImagePathsMappingModel> GetImagePathsMappings()
        {
            List<ImagePathsMappingModel> imagePaths = ImagePathDAL.GetImagePathsMappings();
            imagePaths = imagePaths.FindAll(t => t.UploadSign == 0 && t.LocalDeleteSign == 1);
            return imagePaths;
        }

        public async Task LocalImageCut()
        {
            string oldPath1 = ConfigurationManager.AppSettings["OldPath1"];
            string newPath1 = ConfigurationManager.AppSettings["NewPath1"];
            string oldPath2 = ConfigurationManager.AppSettings["OldPath2"];
            string newPath2 = ConfigurationManager.AppSettings["NewPath2"];
            string oldPath3 = ConfigurationManager.AppSettings["OldPath3"];
            string newPath3 = ConfigurationManager.AppSettings["NewPath3"];
            string oldPath4 = ConfigurationManager.AppSettings["OldPath4"];
            string newPath4 = ConfigurationManager.AppSettings["NewPath4"];
            var completionSource = new TaskCompletionSource<object>();
            Task task1 = LocalImageCut(oldPath3, newPath3, completionSource);
            Task task2 = LocalImageCut(oldPath1, newPath1, completionSource);
            Task task3 = LocalImageCut(oldPath2, newPath2, completionSource);
            Task task4 = LocalImageCut(oldPath4, newPath4, completionSource);
            await Task.WhenAll(task1, task2, task3, task4);
            Console.WriteLine("所有剪切均已完成！");
        }

        public async Task LocalImageCut(string oldPath, string newPath, TaskCompletionSource<object> completionSource)
        {
            if (!Directory.Exists(oldPath))
            {
                logger.Error(oldPath + "不存在！");
                return;
            }
            string[] filePaths = Directory.GetFiles(oldPath);

            List<FileInfo> files = new List<FileInfo>();
            List<Task<string>> CutTasks = new List<Task<string>>();
            foreach (string filePath in filePaths)
            {
                if (filePath.Contains("Thumbs.db"))
                {
                    continue;
                }
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                DateTime date = new DateTime();
                try
                {
                    date = DateTime.ParseExact(fileName.Substring(0, 14), "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    logger.Error(filePath + ex.Message);
                    continue;
                }
                string formattedDate = date.ToString("yyyy-MM-dd");
                string newFilePath = newPath + "\\" + formattedDate + "\\" + Path.GetFileName(filePath);
                Task<string> cutmsg = FileHelper.MoveFileAsync(filePath, newFilePath);
                CutTasks.Add(cutmsg);
                _ = ProcessUploadResult(cutmsg);
            }

            await Task.WhenAll(CutTasks);
            Console.WriteLine("Batch processing completed.");
            completionSource.SetResult(null);
        }

        public async Task<string> UploadImagesToTPAsync(ImagePathsMappingModel img)
        {
            int sum = 0;
            string msg = string.Empty;
            if (img is null)
            {
                msg = "执行时间：" + DateTime.Now + "\r\n" + $"没有需要上传的图片";
                logger.Info(msg);
                return msg;
            }
            string result = await FileHelper.CopyFileAsync(img.OldPicturePath, img.TPServerPath);
            if (string.IsNullOrEmpty(result))
            {
                img.UploadSign = 1;
                img.UploadTime = DateTime.Now;
                ImagePathDAL.Save(img.uid.ToString(), img);
                msg = $"时间：{DateTime.Now} 源文件：{img.OldPicturePath}\t已成功上传至：{img.TPServerPath}";
                logger.Info(msg);
                return msg;
            }
            msg += "执行时间：" + DateTime.Now + result + "\r\n";
            logger.Error(msg);
            return msg;
        }
    }
}
