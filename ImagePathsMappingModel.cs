using SqlSugar;
using System;

namespace ImageMigration
{
    /// <summary>
    /// 图片路径映射 以其上传记录
    /// </summary>
    [SugarTable("SF$$ImagePathsMapping")]
    public class ImagePathsMappingModel
    {
        /// <summary>
        /// 图片原路径
        /// </summary>
        public string OldPicturePath { get; set; }
        /// <summary>
        /// TP图片服务器路径
        /// </summary>
        public string TPServerPath { get; set; }
        /// <summary>
        /// 本地删除标志 0|本地已删除  1|本地存在
        /// </summary>
        public int LocalDeleteSign { get; set; }
        /// <summary>
        /// 上传标志 0|未上传  1|已上传TP
        /// </summary>
        public int UploadSign { get; set; }
        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime? UploadTime { get; set; }
        /// <summary>
        /// 数据库主键
        /// 自增列
        /// </summary>
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int uid { get; set; }
    }

    public class PageInfo
    {
        /// <summary>
        /// 每页行数
        /// </summary>
        public int limit { get; set; }
        /// <summary>
        /// 当前页
        /// </summary>
        public int page { get; set; }
        /// <summary>
        /// 总记录数
        /// </summary>
        public int recordCount { get; set; }
    }
}
