using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Helpers
{
    public class PathHelper
    {
        public static string MapPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string MapPath(string path)
        {
            try
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentException("传入的路径不能为空。", nameof(path));
                }

                string directoryPath;
                if (Path.HasExtension(path))
                {
                    // 如果是文件路径，提取其所在的文件夹路径
                    directoryPath = Path.GetDirectoryName(path);
                }
                else
                {
                    // 如果是文件夹路径，直接使用该路径
                    directoryPath = path;
                }

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                return path;
            }
            catch (Exception ex)
            {
                throw new Exception($"创建文件夹时发生错误: {ex.Message}");
            }
        }
    }
}
