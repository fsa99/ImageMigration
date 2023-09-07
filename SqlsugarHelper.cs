using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMigration
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static async Task Main(string[] args)
        {
            Console.WriteLine("开始上传！！！");
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Info($"{DateTime.Now}:开始上传！");
            ImagePathBLL imags = new ImagePathBLL(logger);
            try
            {
                await imags.LocalImageCut();
            }
            catch (Exception ex)
            {
                logger.Error($"时间:{DateTime.Now}\r\n {ex.Message} ");
            }

            try
            {
                await imags.BatchUploadPictures();
            }
            catch (Exception ex)
            {
                logger.Error($"时间:{DateTime.Now}\r\n {ex.Message} ");
            }
            finally
            { 
                logger.Info($"{DateTime.Now}:本次结束！");
            }
        }
    }
}
