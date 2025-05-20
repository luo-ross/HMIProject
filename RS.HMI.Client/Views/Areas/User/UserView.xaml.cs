using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.HMI.Client.Messages;
using RS.Widgets.Controls;
using RS.Widgets.Interface;
using RS.Widgets.Models;
using System.Windows;

namespace RS.HMI.Client.Views.Areas
{

    public partial class UserView : RSDialog
    {
        public UserView()
        {
            InitializeComponent();
            WeakReferenceMessenger.Default.Register<UserFormMessage>(this, HandleFormMessage);
        }

        private void HandleFormMessage(object recipient, UserFormMessage message)
        {
            var userFormView = App.ServiceProvider.GetRequiredService<UserFormView>();
            var rsForm = new RSForm(userFormView, message);
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
