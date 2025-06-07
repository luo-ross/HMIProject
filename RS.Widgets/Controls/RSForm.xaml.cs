using RS.Commons;
using RS.Widgets.Interface;
using RS.Widgets.Models;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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


        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="formControl">自定义表单</param>
        /// <param name="message"></param>
        private RSForm()
        {
            InitializeComponent();
        }

        public RSForm(UserControl formControl, IFormService formService)
        {
            InitializeComponent();
            this.FormContent = formControl;
            this.FormService = formService;
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
            var formService = this.FormService;
            this.GetLoading().InvokeLoadingActionAsync(async (cancellationToken) =>
            {
                await formService.FormSubmitCommand.ExecuteAsync(null);
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


        [Description("表单接口")]
        [Browsable(false)]
        public IFormService FormService
        {
            get { return (IFormService)GetValue(FormMessageProperty); }
            set { SetValue(FormMessageProperty, value); }
        }

        public static readonly DependencyProperty FormMessageProperty =
            DependencyProperty.Register(nameof(FormService), typeof(IFormService), typeof(RSForm), new PropertyMetadata(null));
    }
}
