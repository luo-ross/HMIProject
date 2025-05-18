using Microsoft.AspNetCore.Mvc.Abstractions;
using RS.Widgets.Messages;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RS.Widgets.Controls
{
    /// <summary>
    /// RSForm.xaml 的交互逻辑
    /// </summary>
    public partial class RSForm : RSWindow
    {
        public RSForm(UserControl userControl, FormMessageBase message)
        {
            InitializeComponent();
            this.FormContent=userControl;
            this.FormMessage=message;
        }

        [Description("表单内容")]
        public object FormContent
        {
            get { return (object)GetValue(FormContentProperty); }
            set { SetValue(FormContentProperty, value); }
        }

        public static readonly DependencyProperty FormContentProperty =
            DependencyProperty.Register("FormContent", typeof(object), typeof(RSForm), new PropertyMetadata(null));



        [Description("表单消息")]
        [Browsable(false)]
        public FormMessageBase FormMessage
        {
            get { return (FormMessageBase)GetValue(FormMessageProperty); }
            set { SetValue(FormMessageProperty, value); }
        }

        public static readonly DependencyProperty FormMessageProperty =
            DependencyProperty.Register(nameof(FormMessage), typeof(FormMessageBase), typeof(RSForm), new PropertyMetadata(null));



        public event Action ReturnClick;
        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
       

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            switch (this.FormMessage.CRUD)
            {
                case RS.Commons.Enums.CRUD.Add:
                    this.FormMessage.ViewModel.SubmitCommand.Execute(this.FormMessage.FormData);
                    break;
                case RS.Commons.Enums.CRUD.Update:
                    this.FormMessage.ViewModel.UpdateCommand.Execute(this.FormMessage.FormData);
                    break;
            }
           
        }
    }
}
