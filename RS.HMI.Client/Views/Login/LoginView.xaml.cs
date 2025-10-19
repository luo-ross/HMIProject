using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.HMI.IBLL;
using RS.Models;
using RS.RESTfulApi;
using RS.Widgets.Controls;
using RS.Widgets.Enums;
using RS.Widgets.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace RS.HMI.Client.Views
{

    [ServiceInjectConfig(ServiceLifetime.Singleton)]
    public partial class LoginView : RSWindow
    {
        private const string TOKEN = "MainViewModel";
        public LoginViewModel ViewModel { get; set; }
        /// <summary>
        /// 默认构造方法
        /// </summary>
        public LoginView(LoginViewModel loginViewModel)
        {
            InitializeComponent();
            this.DataContext = loginViewModel;
            this.ViewModel = loginViewModel;
            // 注册异步消息处理器
            WeakReferenceMessenger.Default.Register<DataRequestMessage, string>(this, "LoginView", OnDataRequestedAsync);

        }

        private void OnDataRequestedAsync(object recipient, DataRequestMessage message)
        {
            message.Reply(GetCurrentUserAsync());
        }

        private async Task<string> GetCurrentUserAsync()
        {
            await Task.Delay(1000);
            return "123123";
        }
    }
}
