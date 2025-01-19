using RS.BorderWindowDemo.Views.Home;
using RS.HMIClient.Views.Logoin;
using System.Configuration;
using System.Data;
using System.Windows;

namespace RS.BorderWindowDemo
{
   
    public partial class App : Application
    {
        public App()
        {

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            HomeView view = new HomeView();
            view.Show();
        }
    }

}
