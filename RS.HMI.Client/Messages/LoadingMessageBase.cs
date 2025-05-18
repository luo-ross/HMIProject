using RS.Commons;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Client.Messages
{
    public class LoadingMessageBase
    {
        public Func<LoadingConfig,Task<OperateResult>> LoadingFuncAsync { get; set; }
    }
}
