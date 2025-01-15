using RS.BorderWindowDemo.Views.Home;
using System.Configuration;
using System.Data;
using System.Windows;

namespace RS.BorderWindowDemo
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
            base.OnStartup(e);
            HomeView homeView = new HomeView();
            homeView.Show();
        }
    }

}
