using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.SetupApp.Models
{
    public class InstallTask
    {
        public InstallTask()
        {
            TaskStatus= TaskStatus.Created;
        }
        /// <summary>
        /// 安装任务状态
        /// </summary>
        public TaskStatus TaskStatus { get; set; }

        /// <summary>
        /// 具体的安装任务
        /// </summary>
        public Func<CancellationToken, Task<TaskStatus>> InstallAction { get; set; }

        /// <summary>
        /// 安装提示词
        /// </summary>
        public string InstallTooltip { get; set; }

        /// <summary>
        /// 获取任务步数
        /// </summary>
        public Func<CancellationToken, int> GetTaskStepAsync { get; set; }
   
    }
}
