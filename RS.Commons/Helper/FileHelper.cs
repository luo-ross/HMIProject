using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;

namespace RS.Commons.Helper
{
    public class FileHelper
    {
        #region 检测指定目录是否存在
        /// <summary>
        /// 检测指定目录是否存在  
        /// </summary>
        /// <param name="directoryPath">目录的绝对路径</param>
        /// <returns></returns>
        public static bool IsExistDirectory(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }
        #endregion

        #region 检测指定文件是否存在,如果存在返回true
        /// <summary>
        /// 检测指定文件是否存在,如果存在则返回true。  
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>        
        public static bool IsExistFile(string filePath)
        {
            return File.Exists(filePath);
        }
        #endregion

        #region 获取指定目录中的文件列表
        /// <summary>
        /// 获取指定目录中所有文件列表  
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>        
        public static string[] GetFileNames(string directoryPath)
        {
            //如果目录不存在，则抛出异常
            if (!IsExistDirectory(directoryPath))
            {
                throw new FileNotFoundException();
            }

            //获取文件列表
            return Directory.GetFiles(directoryPath);
        }



        #endregion

        #region 获取指定目录中所有子目录列表,若要搜索嵌套的子目录列表,请使用重载方法.
        /// <summary>
        /// 获取指定目录中所有子目录列表,若要搜索嵌套的子目录列表,请使用重载方法.  
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>        
        public static string[] GetDirectories(string directoryPath)
        {
            try
            {
                return Directory.GetDirectories(directoryPath);
            }
            catch (IOException ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 获取指定目录及子目录中所有文件列表
        /// <summary>
        /// 获取指定目录及子目录中所有文件列表  
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>
        /// <param name="searchPattern">模式字符串，"*"代表0或N个字符，"?"代表1个字符。
        /// 范例："Log*.xml"表示搜索所有以Log开头的Xml文件。</param>
        /// <param name="isSearchChild">是否搜索子目录</param>
        public static string[] GetFileNames(string directoryPath, string searchPattern, bool isSearchChild)
        {
            //如果目录不存在，则抛出异常
            if (!IsExistDirectory(directoryPath))
            {
                throw new FileNotFoundException();
            }

            try
            {
                if (isSearchChild)
                {
                    return Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);
                }
                else
                {
                    return Directory.GetFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly);
                }
            }
            catch (IOException ex)
            {
                throw ex;
            }
        }
        #endregion
       
        public static void DeleteFileWithAbsolutPath(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        #region 复制文件夹
        /// <summary>
        /// 复制文件夹(递归) 
        /// </summary>
        /// <param name="varFromDirectory">源文件夹路径</param>
        /// <param name="varToDirectory">目标文件夹路径</param>
        public static void CopyFolder(string varFromDirectory, string varToDirectory)
        {
            Directory.CreateDirectory(varToDirectory);

            if (!Directory.Exists(varFromDirectory)) return;

            string[] directories = Directory.GetDirectories(varFromDirectory);

            if (directories.Length > 0)
            {
                foreach (string d in directories)
                {
                    CopyFolder(d, varToDirectory + d.Substring(d.LastIndexOf("\\")));
                }
            }
            string[] files = Directory.GetFiles(varFromDirectory);
            if (files.Length > 0)
            {
                foreach (string s in files)
                {
                    File.Copy(s, varToDirectory + s.Substring(s.LastIndexOf("\\")), true);
                }
            }
        }
        #endregion

        #region 删除指定文件夹对应其他文件夹里的文件
        /// <summary>
        /// 删除指定文件夹对应其他文件夹里的文件 
        /// </summary>
        /// <param name="varFromDirectory">指定文件夹路径</param>
        /// <param name="varToDirectory">对应其他文件夹路径</param>
        public static void DeleteFolderFiles(string varFromDirectory, string varToDirectory)
        {
            Directory.CreateDirectory(varToDirectory);

            if (!Directory.Exists(varFromDirectory)) return;

            string[] directories = Directory.GetDirectories(varFromDirectory);

            if (directories.Length > 0)
            {
                foreach (string d in directories)
                {
                    DeleteFolderFiles(d, varToDirectory + d.Substring(d.LastIndexOf("\\")));
                }
            }


            string[] files = Directory.GetFiles(varFromDirectory);

            if (files.Length > 0)
            {
                foreach (string s in files)
                {
                    File.Delete(varToDirectory + s.Substring(s.LastIndexOf("\\")));
                }
            }
        }
        #endregion

        #region 从文件的绝对路径中获取文件名( 包含扩展名 )
        /// <summary>
        /// 从文件的绝对路径中获取文件名( 包含扩展名 ) 
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>        
        public static string GetFileName(string filePath)
        {
            //获取文件的名称
            FileInfo fi = new FileInfo(filePath);
            return fi.Name;
        }
        #endregion

        #region 创建一个目录
        /// <summary>
        /// 创建一个目录 
        /// </summary>
        /// <param name="directoryPath">目录的绝对路径</param>
        public static void CreateDirectory(string directoryPath)
        {
            //如果目录不存在则创建该目录
            if (!IsExistDirectory(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
        #endregion

        #region 创建一个文件
        /// <summary>
        /// 创建一个文件。 
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        public static void CreateFile(string filePath)
        {
            try
            {
                //如果文件不存在则创建该文件
                if (!IsExistFile(filePath))
                {
                    //创建一个FileInfo对象
                    FileInfo file = new FileInfo(filePath);

                    //创建文件
                    FileStream fs = file.Create();

                    //关闭文件流
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                //LogHelper.WriteTraceLog(TraceLogLevel.Error, ex.Message);
                throw ex;
            }
        }

        #endregion

        #region 获取一个文件的长度
        /// <summary>
        /// 获取一个文件的长度,单位为Byte 
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>        
        public static long GetFileSize(string filePath)
        {
            //创建一个文件对象
            FileInfo fi = new FileInfo(filePath);

            //获取文件的大小
            return fi.Length;
        }
        #endregion

        #region 创建文件
        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="content">内容</param>
        public static void CreateFileContent(string path, string content)
        {
            FileInfo fi = new FileInfo(path);
            var di = fi.Directory;
            if (!di.Exists)
            {
                di.Create();
            }
            StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.GetEncoding("GB2312"));
            sw.Write(content);
            sw.Close();
        }
        #endregion

        #region 获取文件大小并以B，KB，GB，TB
        /// <summary>
        /// 计算文件大小函数(保留两位小数),Size为字节大小 
        /// </summary>
        /// <param name="size">初始文件大小</param>
        /// <returns></returns>
        public static string ToFileSize(long size)
        {
            string m_strSize = "";
            long FactSize = 0;
            FactSize = size;
            if (FactSize < 1024.00)
                m_strSize = FactSize.ToString("F2") + " 字节";
            else if (FactSize >= 1024.00 && FactSize < 1048576)
                m_strSize = (FactSize / 1024.00).ToString("F2") + " KB";
            else if (FactSize >= 1048576 && FactSize < 1073741824)
                m_strSize = (FactSize / 1024.00 / 1024.00).ToString("F2") + " MB";
            else if (FactSize >= 1073741824)
                m_strSize = (FactSize / 1024.00 / 1024.00 / 1024.00).ToString("F2") + " GB";
            return m_strSize;
        }
        #endregion

        #region 本地路径
     
        public static string WinMapPath(string path)
        {
            path = path.Replace("/", "\\");
            if (path.StartsWith("\\"))
            {
                path = path.TrimStart('\\');
            }
            return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }
        #endregion

        /// <summary>
        /// 获取目录下指定扩展名的图像文件
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        /// <param name="imageExtensions">需要匹配的图像扩展名（如 ["*.jpg", "*.png"]）</param>
        /// <param name="searchOption">搜索选项（是否包含子目录）</param>
        /// <returns>图像文件路径列表</returns>
        /// <exception cref="DirectoryNotFoundException">目录不存在时抛出</exception>
        /// <exception cref="ArgumentNullException">扩展名参数为空时抛出</exception>
        public static List<string> GetImageFiles(
           string directoryPath,
            IEnumerable<string> imageExtensions,
            SearchOption searchOption = SearchOption.TopDirectoryOnly
        )
        {
            // 验证目录是否存在
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Directory not exist：{directoryPath}");
            }

            // 验证扩展名参数不为空
            if (imageExtensions == null || !imageExtensions.Any())
            {
                throw new ArgumentNullException(nameof(imageExtensions), "Please provide at least one image file extension.");
            }

            var imageFiles = new List<string>();

            // 遍历传入的扩展名，获取匹配的文件
            foreach (var ext in imageExtensions)
            {
                // 过滤无效的扩展名（如空字符串或不含通配符的格式）
                if (!string.IsNullOrWhiteSpace(ext) && ext.Contains("*"))
                {
                    imageFiles.AddRange(Directory.GetFiles(directoryPath, ext, searchOption));
                }
            }

            // 去重并返回
            return imageFiles.Distinct().ToList();
        }
    
     

        public static byte[] GetBytesByBase64String(string base64Str)
        {
            return Convert.FromBase64String(base64Str);
        }

        public static void Base64StringToFile(string content, string file, FileMode fileMode)
        {
            FileStream fs = new FileStream(file, fileMode, FileAccess.ReadWrite, FileShare.ReadWrite);
            try
            {
                if (fs.CanWrite)
                {
                    byte[] buffer = Convert.FromBase64String(content);
                    if (buffer.Length > 0)
                    {
                        fs.Write(buffer, 0, buffer.Length);
                        fs.Flush();
                    }
                }
            }
            finally
            {
                fs.Close();
                fs.Dispose();
            }
        }

        public static void BackUpFile(string sourcefile, string targetfile, string targetPath)
        {
            if (!IsExistFile(sourcefile))
            {
                return;
            }
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            File.Copy(sourcefile, targetfile, true);
        }

        public static void SaveStringToFile(string content, string file, FileMode fileMode)
        {
            FileStream fs = new FileStream(file, fileMode, FileAccess.ReadWrite, FileShare.ReadWrite);
            try
            {
                if (fs.CanWrite)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(content); ;
                    if (buffer.Length > 0)
                    {
                        fs.Write(buffer, 0, buffer.Length);
                        fs.Flush();
                    }
                }
            }
            finally
            {
                fs.Close();
                fs.Dispose();
            }
        }
        public static string StreamToBase64String(Stream fileStrem)
        {
            try
            {
                string content = string.Empty;
                try
                {
                    if (fileStrem.CanRead)
                    {
                        byte[] buffer = new byte[fileStrem.Length];
                        fileStrem.Read(buffer, 0, buffer.Length);
                        content = Convert.ToBase64String(buffer);
                    }
                }
                finally
                {
                    fileStrem.Close();
                    fileStrem.Dispose();
                }
                return content;
            }
            catch (Exception)
            {
                return "";
            }
        }



        public static string ImgToBase64String(string file)
        {
            try
            {
                string content = string.Empty;
                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                try
                {
                    if (fs.CanRead)
                    {
                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        content = Convert.ToBase64String(buffer);
                    }
                }
                finally
                {
                    fs.Close();
                    fs.Dispose();
                }
                return content;
            }
            catch (Exception)
            {
                return "";
            }
        }
        public static string ShareRead(string file, Encoding encoding)
        {
            string content = string.Empty;
            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            try
            {
                if (fs.CanRead)
                {
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    content = encoding.GetString(buffer);
                }
            }
            finally
            {
                fs.Close();
                fs.Dispose();
            }
            return content;
        }

        public static void ShareAppend(string content, string file, Encoding encoding)
        {
            ShareWrite(content, file, encoding, FileMode.Append);
        }

        public static string GetFileNameWithoutExtension(string filePath)
        {
            //获取文件的名称
            return Path.GetFileNameWithoutExtension(filePath);
        }
        public static void ShareWrite(string content, string file, Encoding encoding, FileMode fileMode)
        {
            FileStream fs = new FileStream(file, fileMode, FileAccess.Write, FileShare.Read);
            try
            {
                if (fs.CanWrite)
                {
                    byte[] buffer = encoding.GetBytes(content);
                    if (buffer.Length > 0)
                    {
                        fs.Write(buffer, 0, buffer.Length);
                        fs.Flush();
                    }
                }
            }
            finally
            {
                fs.Close();
                fs.Dispose();
            }
        }


        public static byte[] FileContent(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    byte[] buffur = new byte[fs.Length];
                    fs.Read(buffur, 0, (int)fs.Length);
                    return buffur;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static FileInfo[] GetFiles(string directoryPath, string[] searchPatterns)
        {
            // 确保传入的目录路径是有效的  
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"The directory {directoryPath} was not found.");
            }

            // 对每个搜索模式调用Directory.GetFiles，并将结果转换为FileInfo数组  
            var filePaths = searchPatterns
                .SelectMany(pattern => Directory.GetFiles(directoryPath, pattern))
                .Select(path => new FileInfo(path))
                .ToArray();

            return filePaths;
        }


     


      


    
    }
}
