using RS.BorderWindowDemo.Views.Home;
using RS.Widgets.Controls;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Collections.Specialized.BitVector32;

namespace RS.HMIClient.Views.Logoin
{
    /// <summary>
    /// LoginView.xaml 的交互逻辑
    /// </summary>
    public partial class LoginView : RSWindow
    {
        /// <summary>
        /// 数据实体
        /// </summary>
        public LoginViewModel ViewModel { get; set; }

        /// <summary>
        /// 默认构造方法
        /// </summary>
        public LoginView()
        {
            InitializeComponent();
            this.ViewModel = this.DataContext as LoginViewModel;
        }

        /// <summary>
        /// 超级连接跳转
        /// </summary>
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start("explorer.exe", e.Uri.ToString());
        }

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnVerify_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 登录点击事件
        /// </summary>
        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // 验证用户名和密码数据
            var validResult = this.ViewModel.LoginModel.ValidObject();
            if (!validResult)
            {
                return;
            }
            var loadingConfig = new LoadingConfig()
            {
                LoadingType = LoadingType.BorderSurroundingAnimation,
                //LoadingColor = new SolidColorBrush(Colors.Blue),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                IsIndeterminate = true
            };

            var sdf = await this.InvokeLoadingActionAsync(async () =>
              {
                  try
                  {
                      for (int i = 0; i < 100; i++)
                      {
                          loadingConfig.Value = i;
                          await Task.Delay(20);
                      }
                      await Task.Delay(1000);

                      return true;
                  }
                  catch (Exception)
                  {
                      throw;
                  }
                  finally
                  {

                  }
              }, loadingConfig);

            if (sdf)
            {
                MessageBox.Show("完成啦");
            }
        }





        /// <summary>
        /// 注册点击事件
        /// </summary>
        private void BtnSignUp_Click(object sender, RoutedEventArgs e)
        {
            //注册信息验证
            var validResult = this.ViewModel.SignUpModel.ValidObject();
            if (!validResult)
            {
                return;
            }


        }


    }
}
