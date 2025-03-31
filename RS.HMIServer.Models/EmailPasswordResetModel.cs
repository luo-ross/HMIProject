using System.ComponentModel.DataAnnotations;

namespace RS.HMIServer.Models
{
    public class EmailPasswordResetModel
    {
        /// <summary>
        /// 邮箱地址
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 重置链接
        /// </summary>
        public string ResetLink { get; set; }

    }
}
