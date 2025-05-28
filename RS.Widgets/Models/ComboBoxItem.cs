using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    public class ComboBoxItem<T>
    {
        public T SelectedValue { get; set; }

        public string DisplayMember { get; set; }
    }
}
