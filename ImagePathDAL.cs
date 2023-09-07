using System.Collections.Generic;
using System.Configuration;
using TZWMS.DataAccess;

namespace ImageMigration
{
    public class ImagePathDAL
    {
        static string connectionString = ConfigurationManager.AppSettings["connectionString"];
        public static List<ImagePathsMappingModel> GetImagePathsMappings()
        {
            List<ImagePathsMappingModel> lines = new List<ImagePathsMappingModel>();
            using (SqlsugarHelper db = new SqlsugarHelper(connectionString))
            {
                lines = db.SqlQuery<ImagePathsMappingModel>("SELECT OldPicturePath,TPServerPath,LocalDeleteSign,UploadSign,UploadTime,uid FROM [dbo].[SF$$ImagePathsMapping] ");
            }
            return lines;
        }

        public static bool Save(string uid, ImagePathsMappingModel model)
        {
            using (SqlsugarHelper db = new SqlsugarHelper(connectionString))
            {
                if (string.IsNullOrEmpty(uid))
                {
                    return db.ExecuteReturnId(model) > 0;
                }
                else
                {
                    model.uid = int.Parse(uid);
                    return db.Updateable(model) > 0;
                }
            }
        }

        public static bool Delete(List<ImagePathsMappingModel> models)
        {
            using (SqlsugarHelper db = new SqlsugarHelper(connectionString))
            {
                return db.Deleteable(models) > 0;
            }
        }
    }
}
