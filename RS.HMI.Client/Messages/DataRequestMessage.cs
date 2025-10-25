using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Client.Messages
{
    
    public class DataRequestMessage<T,D> : AsyncRequestMessage<T>
    {
        public D Data { get; set; }
    }

   
}
