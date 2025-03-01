using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Client.Views.Home
{
    public class Class1 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }


        private string myName="123123123";

        public string MyName
        {
            get { return myName; }
            set
            {
                myName = value;
                this.OnPropertyChanged(nameof(MyName));
            }
        }

    }
}
