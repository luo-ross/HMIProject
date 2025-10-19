using CommunityToolkit.Mvvm.ComponentModel;
using RS.Commons.Enums;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace RS.HMI.Client.DataConfigs
{
    /// <summary>
    /// 三菱数据配置
    /// </summary>
    public class MitsubishiDataConfig : DataConfigBase
    {


  

        /// <summary>
        /// 这里是手动克隆 肯定是最快的
        /// </summary>
        /// <returns></returns>
        public override MitsubishiDataConfig Clone()
        {
            throw new NotImplementedException();
        }
    }
}
