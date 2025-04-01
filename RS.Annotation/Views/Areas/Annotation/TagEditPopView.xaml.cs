using RS.Commons.Enums;
using RS.Widgets.Commons;
using RS.Widgets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RS.Annotation.Views.Areas
{
    /// <summary>
    /// TagEditPopView.xaml 的交互逻辑
    /// </summary>
    public partial class TagEditPopView : Popup
    {
        public AnnotationView AnnotationView { get; set; }
        public AnnotationViewModel ViewModel { get; set; }
        public TagEditPopView(AnnotationView annotationView)
        {
            InitializeComponent();
            this.DataContext = annotationView.ViewModel;
            this.AnnotationView = annotationView;
            this.ViewModel = annotationView.ViewModel;
            this.Opened += TagEditPopView_Opened;

            this.Closed += TagEditPopView_Closed;
        }

        private void TagEditPopView_Closed(object? sender, EventArgs e)
        {
            this.Opened -= TagEditPopView_Opened;
            this.Closed -= TagEditPopView_Closed;
        }

        private void TagEditPopView_Opened(object? sender, EventArgs e)
        {
            this.ResetColorSelect();
            if (this.ViewModel.CRUD == CRUD.Add)
            {
                this.TxtClassName.Focus();
            }

            if (!this.ViewModel.TagModelEdit.IsShortCutAuto)
            {
                this.TxtShortCut.Text = this.ViewModel.TagModelEdit.ShortCut;
            }
        }

        private void ResetColorSelect()
        {
            if (this.ViewModel != null && this.ViewModel.TagModelEdit != null && !(string.IsNullOrEmpty(this.ViewModel.TagModelEdit.TagColor) || string.IsNullOrWhiteSpace(this.ViewModel.TagModelEdit.TagColor)))
            {
                this.ColorPicker.ColorSelect = (Color)(ColorConverter.ConvertFromString(this.ViewModel.TagModelEdit.TagColor));
            }
        }


        public event Action OnOKCallBack;

        private async void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            //标签输入是否满足要求
            var validResult = this.ViewModel.TagModelEdit.ValidObject();
            if (!validResult)
            {
                return;
            }

            var tagModelList = this.AnnotationView.GetTagModelList();
            //判断标签名称是否存在
            if (tagModelList.Any(t => t.ClassName == this.ViewModel.TagModelEdit.ClassName && t.Id != this.ViewModel.TagModelEdit.Id))
            {
                await this.HostView.MessageBox.ShowAsync("标签名称已存在！");
                this.TxtClassName.SelectAll();
                this.TxtClassName.Focus();
                return;
            }

            //判断标签颜色是否存在
            if (tagModelList.Any(t => t.TagColor == this.ViewModel.TagModelEdit.TagColor && t.Id != this.ViewModel.TagModelEdit.Id))
            {
                await this.HostView.MessageBox.ShowAsync("标签颜色已存在！");
                this.TxtTagCor.SelectAll();
                this.TxtTagCor.Focus();
                return;
            }
            //如果当前快捷键里的值为空
            if (string.IsNullOrEmpty(this.TxtShortCut.Text) || string.IsNullOrWhiteSpace(this.TxtShortCut.Text))
            {
                var shortCutList = tagModelList.Select(t => t.ShortCut).ToList();
                this.ViewModel.TagModelEdit.ShortCut = ShortCutUtil.GetShortCutKey(shortCutList);
                this.ViewModel.TagModelEdit.IsShortCutAuto = true;
            }
            else
            {
                if (!ShortCutUtil.ShortCutList.Contains(this.TxtShortCut.Text))
                {
                    await this.HostView.MessageBox.ShowAsync("快捷键不受支持！");
                    this.TxtShortCut.SelectAll();
                    this.TxtShortCut.Focus();
                    return;
                }

                if (tagModelList.Any(t => t.ShortCut == this.TxtShortCut.Text && t.Id != this.ViewModel.TagModelEdit.Id))
                {
                    await this.HostView.MessageBox.ShowAsync("快捷键冲突！");
                    this.TxtShortCut.SelectAll();
                    this.TxtShortCut.Focus();
                    return;
                }

                this.ViewModel.TagModelEdit.ShortCut = this.TxtShortCut.Text;
                this.ViewModel.TagModelEdit.IsShortCutAuto = false;
            }


            switch (this.ViewModel.CRUD)
            {
                case CRUD.Add:
                    this.AnnotationView.ProjectsView.ViewModel.ProjectModelSelect.TagModelList.Add(this.ViewModel.TagModelEdit);
                    this.AnnotationView.ProjectsView.ViewModel.ProjectModelSelect.IsSaved = false;

                    foreach (var item in this.AnnotationView.ProjectsView.ViewModel.ProjectModelSelect.TagModelList)
                    {
                        item.IsSelect = false;
                    }
                    this.ViewModel.TagModelEdit.IsSelect = true;
                    break;
                case CRUD.Update:
                    this.ViewModel.TagModelEdit.CloneTo(this.ViewModel.TagModelSelect);
                    break;
            }

            this.IsOpen = false;
            OnOKCallBack?.Invoke();
        }

        private void ColorPicker_OnColorSelect(Color colorSelect)
        {
            this.ViewModel.TagModelEdit.TagColor = colorSelect.ToString();
        }
    }
}
