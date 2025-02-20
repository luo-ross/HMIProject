using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.ClientData.Entities
{
    public  sealed class CommuStation
    {
        /// <summary>
        /// 通讯站主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 通讯站名称
        /// </summary>
        public string Name { get; set; }
    }
}
