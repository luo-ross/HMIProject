using CommunityToolkit.Mvvm.ComponentModel;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Client.Models
{
    public partial class RoleModel : ModelBase
    {

        /// <summary>
        /// 角色名称
        /// </summary>
        [ObservableProperty]
        private string? name;

        /// <summary>
        /// 备注
        /// </summary>
        [ObservableProperty]
        private string? description;

        /// <summary>
        /// 绑定公司Id
        /// </summary>
        public long? CompanyId { get; set; }

        /// <summary>
        /// 绑定公司名称
        /// </summary>
        [ObservableProperty]
        private string? companyName;

      
    }
}
