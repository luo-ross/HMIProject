using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIClient.Models
{
    public class ComboBoxItemModel<T>
    {
        public T Key { get; set; }
        public string KeyDes { get; set; }
    }
}
