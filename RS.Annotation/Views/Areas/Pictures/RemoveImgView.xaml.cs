using RS.Widgets.Controls;
using RS.Annotation.Views.Home;
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
using RS.Widgets.Controls;

namespace RS.Annotation.Views.Areas.Pictures
{
    /// <summary>
    /// RemoveImgView.xaml 的交互逻辑
    /// </summary>
    public partial class RemoveImgView : RSDialog
    {

        public HomeView HomeView { get; set; }
        public PicturesView PicturesView { get; set; }
        public RemoveImgView(PicturesView picturesView, string confirmDes = null)
        {
            InitializeComponent();
            this.PicturesView = picturesView;

            if (confirmDes == null)
            {
                confirmDes = "你确定要删除所选图像吗";
            }
            this.TxtConfirmDes.Text = confirmDes;

            this.Loaded += RemoveImgView_Loaded;

        }

        private void RemoveImgView_Loaded(object sender, RoutedEventArgs e)
        {
            this.HomeView = this.TryFindParent<HomeView>();
            this.Border.Focusable = true;
            this.Border.Focus();
        }

        /// <summary>
        /// 移除图像回调 参数True 代表了点击OK，参数False代表点击了取消
        /// </summary>
        public event Action<bool> OnReveImgCallBack;

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            this.HomeView.HideModal();
            bool isDeleteFileFromSystem = this.CkIsDeleteFileFromSystem.IsChecked == true;
            this.PicturesView.RemoveImgModelSelect(isDeleteFileFromSystem);
            OnReveImgCallBack?.Invoke(true);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.HomeView.HideModal();
            OnReveImgCallBack?.Invoke(false);
        }
    }
}
