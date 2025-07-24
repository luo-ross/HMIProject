using RS.Win32API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Commons
{
    public class FileHelper
    {
        /// <summary>
        /// 打开文件所在位置
        /// </summary>
        public static void ExplorerFile(string filePath)
        {

            if (!File.Exists(filePath) && !Directory.Exists(filePath))
            {
                return;
            }

            if (Directory.Exists(filePath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    //Arguments = $"/select,\"{filePath}\"",
                    Arguments = $"\"{filePath}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };
                Process.Start(startInfo);
            }
            else
            {
                nint pidlList = NativeMethods.ILCreateFromPathW(filePath);
                if (pidlList != nint.Zero)
                {
                    try
                    {
                        Marshal.ThrowExceptionForHR(NativeMethods.SHOpenFolderAndSelectItems(pidlList, 0, nint.Zero, 0));
                    }
                    finally
                    {
                        NativeMethods.ILFree(pidlList);
                    }
                }
            }
        }

        /// <summary>
        /// 创建一个临时文件并返回打开的可写文件流
        /// </summary>
        /// <param name="filePath">输出参数，返回创建的临时文件的完整路径</param>
        /// <returns>可写入的文件流，使用完毕后需释放</returns>
        public static FileStream CreateAndOpenTemporaryFile(out string filePath)
        {
            filePath = Path.GetTempFileName();
            try
            {
                return new FileStream(
                    filePath,
                    FileMode.OpenOrCreate,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 4096,
                    FileOptions.DeleteOnClose
                );
            }
            catch
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {

                }
                throw;
            }
        }


        /// <summary>
        /// 安全删除临时文件，忽略常见异常
        /// </summary>
        /// <param name="filePath">要删除的文件路径</param>
        /// <returns>如果文件成功删除或不存在，返回 true；否则返回 false</returns>
        public static bool DeleteTemporaryFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) { 
                return true; // 路径为空视为已删除
            }

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return true; 
            }
            catch (DirectoryNotFoundException)
            {
                return true; 
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }

    }
}
