using Microsoft.VisualBasic.FileIO;
using RS.SetupApp.Models;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RS.SetupApp.Views
{
    public class HomeViewModel : NotifyBase
    {

        private double installProgressValue;
        /// <summary>
        /// 安装进度条的值
        /// </summary>

        public double InstallProgressValue
        {
            get { return installProgressValue; }
            set
            {
                installProgressValue = value;
                this.OnPropertyChanged();
            }
        }


        private int accumulatedSteps;
        /// <summary>
        /// 累计步数
        /// </summary>
        public int AccumulatedSteps
        {
            get { return accumulatedSteps; }
            set
            {
                accumulatedSteps = value;
                this.OnPropertyChanged();
                UpateInstallProgressValue();
            }
        }

        /// <summary>
        /// 重置进度条
        /// </summary>
        public void ResetProgress()
        {
            //任务执行之前清空进度
            this.TotalProgressStep = 0;
            this.AccumulatedSteps = 0;
        }

        /// <summary>
        /// 更新进度条
        /// </summary>
        public void UpateInstallProgressValue()
        {
            if (this.TotalProgressStep == 0)
            {
                this.InstallProgressValue = 0;
                return;
            }
            this.InstallProgressValue = Math.Ceiling((double)AccumulatedSteps / TotalProgressStep * 100);
        }


        private int totalProgressStep;
        /// <summary>
        /// 总进度步数
        /// </summary>
        public int TotalProgressStep
        {
            get { return totalProgressStep; }
            set
            {
                totalProgressStep = value;
                this.OnPropertyChanged();
            }
        }



        private TaskStatus installTaskStatus = TaskStatus.Created;
        /// <summary>
        /// 安装任务状态
        /// </summary>

        public TaskStatus InstallTaskStatus
        {
            get { return installTaskStatus; }
            set
            {
                installTaskStatus = value;
                this.OnPropertyChanged();
            }
        }


        private bool isReadAndAgree;
        /// <summary>
        /// 是否阅读并同意
        /// </summary>
        public bool IsReadAndAgree
        {
            get { return isReadAndAgree; }
            set
            {
                isReadAndAgree = value;
                this.OnPropertyChanged();
            }
        }

        private bool isAllowAutoRun;
        /// <summary>
        /// 是否
        /// </summary>
        public bool IsAllowAutoRun
        {
            get { return isAllowAutoRun; }
            set
            {
                isAllowAutoRun = value;
                this.OnPropertyChanged();
            }
        }


        private long diskSizeNeed;
        /// <summary>
        /// 所需空间
        /// </summary>
        public long DiskSizeNeed
        {
            get { return diskSizeNeed; }
            set
            {
                diskSizeNeed = value;
                this.OnPropertyChanged();
            }
        }



        private long diskSizeFree;
        /// <summary>
        /// 可用空间
        /// </summary>
        public long DiskSizeFree
        {
            get { return diskSizeFree; }
            set
            {
                diskSizeFree = value;
                this.OnPropertyChanged();
            }
        }


        private string installDir;
        /// <summary>
        /// 安装目录
        /// </summary>
        public string InstallDir
        {
            get
            {
                if (installDir == null)
                {
                    //var defautlDir = SpecialDirectories.ProgramFiles;
                    //installDir = Path.Join(defautlDir, "RSAnnotation");
                    installDir = $@"D:\RSAnnotation";
                }
                return installDir;
            }
            set
            {
                installDir = value;
                this.OnPropertyChanged();
            }
        }

        private bool isCanInstall;
        /// <summary>
        /// 是否可以安装
        /// </summary>
        public bool IsCanInstall
        {
            get { return isCanInstall; }
            set
            {
                isCanInstall = value;
                this.OnPropertyChanged();
            }
        }


        private string errorTooltip;
        /// <summary>
        /// 错误提示
        /// </summary>
        public string ErrorTooltip
        {
            get { return errorTooltip; }
            set
            {
                errorTooltip = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 清空错误提示
        /// </summary>
        public void ClearErrorTooltip()
        {
            this.ErrorTooltip = null;
        }


        private string runningTooltip = "安装进行中...";
        /// <summary>
        /// 安装过程中的提示词
        /// </summary>
        public string RunningTooltip
        {
            get { return runningTooltip; }
            set
            {
                runningTooltip = value;
                this.OnPropertyChanged();
            }
        }


        private Visibility isCloseBtnVisibility ;
        /// <summary>
        /// 安装过程中的提示词
        /// </summary>
        public Visibility IsCloseBtnVisibility
        {
            get { return isCloseBtnVisibility; }
            set
            {
                isCloseBtnVisibility = value;
                this.OnPropertyChanged();
            }
        }
    }
}
