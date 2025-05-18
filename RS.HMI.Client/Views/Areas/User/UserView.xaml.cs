using CommunityToolkit.Mvvm.Messaging;
using IdGen;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.HMI.Client.Messages;
using RS.HMI.Client.Models;
using RS.Widgets.Controls;
using RS.Widgets.Enums;
using RS.Widgets.Interface;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RS.HMI.Client.Views.Areas
{

    public partial class UserView : RSUserControl, ILoadingService
    {
        private UserViewModel ViewModel { get; set; }
        public UserView()
        {
            InitializeComponent();
            WeakReferenceMessenger.Default.Register<UserLoadingMessage>(this, HandleLoadingMessage);
            WeakReferenceMessenger.Default.Register<UserFormMessage>(this, HandleFormMessage);
            this.Loaded += UserView_Loaded;
        }

        private void UserView_Loaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel = this.DataContext as UserViewModel;
        }

        private void HandleFormMessage(object recipient, UserFormMessage message)
        {
            var userFormView = App.ServiceProvider.GetRequiredService<UserFormView>();
            var rsForm = new RSForm(userFormView, message);
            rsForm.Owner = this.ParentWin;
            rsForm.Closed += RsForm_Closed;    
            rsForm.Show();
        }

        private void RsForm_Closed(object? sender, EventArgs e)
        {
            this.ParentWin?.Activate();
        }

        private async void HandleLoadingMessage(object recipient, UserLoadingMessage message)
        {
            LoadingConfig loadingConfig = new LoadingConfig(); 
            //在这里决定UI使用哪个加载进度条进行数据显示
            var operateResult = await this.InvokeLoadingActionAsync(async (can) =>
            {
                if (message.LoadingFuncAsync != null)
                {
                    return await message.LoadingFuncAsync.Invoke(loadingConfig);
                }
                return OperateResult.CreateSuccessResult();
            },loadingConfig: loadingConfig);
            if (!operateResult.IsSuccess)
            {
                await this.MessageBox.ShowAsync(operateResult.Message);
            }
        }

      
    }
}
