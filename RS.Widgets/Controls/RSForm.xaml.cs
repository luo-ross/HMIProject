using Microsoft.AspNetCore.Mvc.Abstractions;
using RS.Commons;
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
        public static readonly RoutedCommand SubmitCommand = new RoutedCommand();
        public static readonly RoutedCommand ReturnCommand = new RoutedCommand();

        public event Action ReturnClick;
        public RSForm(UserControl userControl, FormMessageBase message)
        {
            InitializeComponent();
            this.FormContent = userControl;
            this.FormMessage = message;
            //this.InputBindings.Add(new KeyBinding(SubmitCommand, Key.Y, ModifierKeys.Alt));
            this.CommandBindings.Add(new CommandBinding(SubmitCommand, SubmitExecuted, CanSubmitExecuted));
            this.CommandBindings.Add(new CommandBinding(ReturnCommand, ReturnExecuted, CanReturnExecuted));

            this.MouseEnter += RSForm_MouseEnter;
        }

        private void RSForm_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Focus();
        }

        private void CanReturnExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ReturnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void CanSubmitExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SubmitExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var formMessage = this.FormMessage;
            this.GetLoading().InvokeLoadingActionAsync(async (cancellationToken) =>
            {
                switch (formMessage.CRUD)
                {
                    case RS.Commons.Enums.CRUD.Add:
                        await formMessage.ViewModel.SubmitCommand.ExecuteAsync(formMessage.FormData);
                        break;
                    case RS.Commons.Enums.CRUD.Update:
                        await formMessage.ViewModel.UpdateCommand.ExecuteAsync(formMessage.FormData);
                        break;
                }


                return OperateResult.CreateSuccessResult();
            });

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






    }
}
