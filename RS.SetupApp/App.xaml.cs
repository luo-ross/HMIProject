using RS.SetupApp.Views;
using System.Configuration;
using System.Data;
using System.Windows;

namespace RS.SetupApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
                
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            System.Windows.FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
            base.OnStartup(e);
            var homeView=new HomeView();
            homeView.Show();
        }
    }

}
