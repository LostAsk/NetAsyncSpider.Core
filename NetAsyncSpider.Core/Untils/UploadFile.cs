using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core.Untils
{
    #region MsMultiPartFormData类
    /// <summary>
    /// MsMultiPartFormData Post上传格式
    /// </summary>
    internal class MsMultiPartFormData
    {   /// <summary>
        /// 私有表单字节列表
        /// </summary>
        private List<byte> formData;
        /// <summary>
        /// 头开始格式
        /// </summary>
        public String Boundary = "---------------" + DateTime.Now.Ticks.ToString("x");
        /// <summary>
        /// 文件名描述格式
        /// </summary>
        private String fieldName = "Content-Disposition: form-data; name=\"{0}\"";
        /// <summary>
        /// 类型描述格式
        /// </summary>
        private String fileContentType = "Content-Type: {0}";
        /// <summary>
        /// 字段描述格式
        /// </summary>
        private String fileField = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"";
        /// <summary>
        /// 编码
        /// </summary>
        private Encoding encode = Encoding.UTF8;
        /// <summary>
        /// MsMultiPartFormData初始化
        /// </summary>
        /// <param name="en">编码</param>
        public MsMultiPartFormData()
        {
            formData = new List<byte>();
        }

        public MsMultiPartFormData(Encoding en = null)
        {
            formData = new List<byte>();
            if (en != null) { encode = en; }
        }
        /// <summary>
        /// 添加字段
        /// </summary>
        /// <param name="FieldName">字段名</param>
        /// <param name="FieldValue">字段值</param>
        public void AddFormField(String FieldName, String FieldValue)
        {
            String newFieldName = fieldName;
            newFieldName = string.Format(newFieldName, FieldName);
            formData.AddRange(encode.GetBytes("--" + Boundary + "\r\n"));
            formData.AddRange(encode.GetBytes(newFieldName + "\r\n\r\n"));
            formData.AddRange(encode.GetBytes(FieldValue + "\r\n"));
        }

        /// <summary>
        /// 添加文件
        /// </summary>
        /// <param name="FieldName">字段名</param>
        /// <param name="FileName">文件名</param>
        /// <param name="FileContent">字节流</param>
        /// <param name="ContentType">类型</param>
        public void AddFile(String FieldName, String FileName, byte[] FileContent, String ContentType = "")
        {
            String newFileField = fileField;
            String newFileContentType = fileContentType;
            newFileField = string.Format(newFileField, FieldName, FileName);
            newFileContentType = string.Format(newFileContentType, ContentType);
            formData.AddRange(encode.GetBytes("--" + Boundary + "\r\n"));
            formData.AddRange(encode.GetBytes(newFileField + "\r\n"));
            formData.AddRange(encode.GetBytes(newFileContentType + "\r\n\r\n"));
            formData.AddRange(FileContent);
            formData.AddRange(encode.GetBytes("\r\n"));
        }
        /// <summary>
        /// 添加实体流文件
        /// </summary>
        /// <param name="FieldName">字段名</param>
        /// <param name="FileName">文件名</param>
        /// <param name="FileContent">字节流</param>
        public void AddStreamFile(String FieldName, String FileName, byte[] FileContent)
        {
            AddFile(FieldName, FileName, FileContent, "application/octet-stream");
        }
        /// <summary>
        /// 添加新表预处理
        /// </summary>
        public void PrepareFormData()
        {
            formData.AddRange(encode.GetBytes("--" + Boundary + "--"));
        }
        /// <summary>
        /// 获取表单
        /// </summary>
        public List<byte> GetFormData()
        {
            return formData;
        }
    }
    #endregion


    #region 上传文件格式
    /// <summary>
    /// 上传文件格式
    /// </summary>
    public class UploadFile
    {
        /// <summary>
        /// 字段名
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// 上传字节流
        /// </summary>
        public byte[] Stream { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件类型 默认"application/octet-stream"
        /// </summary>
        public string ContentType { get; set; } = "application/octet-stream";

    }
    #endregion
}
