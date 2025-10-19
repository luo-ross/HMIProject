using System;
using System.Collections.Generic;
using RS.Commons.Extend;

namespace RS.Commons.Usually
{
    /// <summary>
    /// 常用公共类 
    /// </summary>
    public static class Commons
    {
        #region 自动生成编号
        /// <summary>
        /// 表示全局唯一标识符 (GUID)。  
        /// </summary>
        /// <returns></returns>
        public static string GuId()
        {
            return Guid.NewGuid().ToString();
        }
        /// <summary>
        /// 自动生成编号  201008251145409865  
        /// </summary>
        /// <returns></returns>
        public static string CreateNo()
        {
            Random random = new Random();
            string strRandom = random.Next(1000, 10000).ToString(); //生成编号 
            string code = DateTime.Now.ToString("yyyyMMddHHmmssffff") + strRandom;//形如
            return code;
        }

      
        #endregion


        /// <summary>
        /// 隐藏字符
        /// </summary>
        /// <param name="inputStr"></param>
        /// <param name="startDisplayLength"></param>
        /// <param name="endDiplayLength"></param>
        /// <returns></returns>
        public static string GetxxxString(string inputStr, int startDisplayLength, int endDiplayLength)
        {
            if (string.IsNullOrEmpty(inputStr))
            {
                return string.Empty;
            }
            int strMinLength = startDisplayLength + endDiplayLength;
            var strLen = inputStr.Length;
            if (strLen <= strMinLength)
            {
                if (strLen > 1)
                {
                    return "*".PadLeft(strLen, '*');
                }
                else
                {
                    return "*";
                }
            }

            var subStr = inputStr.Substring(startDisplayLength, strLen - strMinLength);
            return inputStr.Replace(subStr, "*".PadLeft(subStr.Length, '*'));
        }

       
        public static string GetPreviewImgSrc(string fileaddress)
        {

            var index = fileaddress.LastIndexOf(".");
            var suffix = fileaddress.Substring(index);
            switch (suffix)
            {
                case ".png":
                case ".jpg":
                case ".gif":
                case ".bmp":
                case ".jpeg":
                    return fileaddress;
                case ".zip":
                case ".rar":
                    return "/Content/img/rar.jpg";
                case ".doc":
                case ".docx":
                    return "/Content/img/word.jpg";
                case ".xls":
                case ".xlsx":
                    return "/Content/img/excel.jpg";
                case ".ppt":
                case ".pptx":
                    return "/Content/img/ppt.jpg";
                case ".pdf":
                    return "/Content/img/pdf.jpg";
                case ".txt":
                    return "/Content/img/txt.jpg";
                default:
                    return "";
            }
        }

    }
}
