using System.ComponentModel.DataAnnotations;

namespace RS.HMIServer.Models
{
    public class EmailPasswordConfirmModel
    {
        /// <summary>
        /// 邮箱地址
        /// </summary>
        public string Email { get; set; }
       
        /// <summary>
        /// 会话主键
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// MD5密码
        /// </summary>
        public string Password { get; set; }
    }
}
