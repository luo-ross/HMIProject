using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{
    /// <summary>
    /// 角色类
    /// </summary>
    public class RoleModel : ModelBase
    {

        /// <summary>
        /// 角色名称
        /// </summary>
        public string? Name { get; set; }
     
        /// <summary>
        /// 备注
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 绑定公司
        /// </summary>
        public string? CompanyId { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        public string? CompanyName { get; set; }
    }
}
