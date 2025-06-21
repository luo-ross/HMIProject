using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.HMI.Client.Models;
using RS.Widgets.Controls;
using RS.Widgets.Interfaces;
using RS.Widgets.Models;
using System.Windows;

namespace RS.HMI.Client.Views.Areas
{
    /// <summary>
    /// 用户管理视图
    /// </summary>
    public partial class UserView : RSDialog
    {
        public UserView()
        {
            InitializeComponent();
            WeakReferenceMessenger.Default.Register<CRUDViewModel<UserModel>>(this, HandleFormMessage);
        }

        private void HandleFormMessage(object recipient, CRUDViewModel<UserModel> message)
        {
            var formView = App.ServiceProvider.GetRequiredService<UserFormView>();
            var rsForm = new RSForm(formView, message);
            rsForm.Owner = (Window)this.GetParentWin();
            rsForm.Closed += RsForm_Closed;
            rsForm.Show();
        }

        private void RsForm_Closed(object? sender, EventArgs e)
        {
            var window = (Window)this.GetParentWin();
            window?.Activate();
        }

    }
}
