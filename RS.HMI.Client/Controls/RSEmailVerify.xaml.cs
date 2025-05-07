using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML.Runtime;
using OpenTK.Windowing.GraphicsLibraryFramework;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.HMI.IBLL;
using RS.Models;
using RS.RESTfulApi;
using RS.Widgets.Controls;
using RS.Widgets.Enums;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace RS.HMI.Client.Controls
{

    public partial class RSEmailVerify : RSUserControl
    {
        private List<TextBox> VerifyCodeInputList { get; set; }
        public event Action<string> OnVerifyConfirm;
        public event Action OnBtnReturnClick;

        public RSEmailVerify()
        {
            InitializeComponent();

            VerifyCodeInputList = new List<TextBox>();
            VerifyCodeInputList.Add(this.TxtVerifyCode0);
            VerifyCodeInputList.Add(this.TxtVerifyCode1);
            VerifyCodeInputList.Add(this.TxtVerifyCode2);
            VerifyCodeInputList.Add(this.TxtVerifyCode3);
            VerifyCodeInputList.Add(this.TxtVerifyCode4);
            VerifyCodeInputList.Add(this.TxtVerifyCode5);
        }


        [Description("邮箱绑定")]
        public string Email
        {
            get { return (string)GetValue(EmailProperty); }
            set { SetValue(EmailProperty, value); }
        }

        public static readonly DependencyProperty EmailProperty =
            DependencyProperty.Register("Email", typeof(string), typeof(RSEmailVerify), new PropertyMetadata(null));



        /// <summary>
        /// 验证码确认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnVerifyConfirm_Click(object sender, RoutedEventArgs e)
        {
            var textList = VerifyCodeInputList.Select(t => t.Text).ToList();
            var verify = string.Join("", textList);
            OnVerifyConfirm?.Invoke(verify);
        }

        private void TxtVerifyCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            var index = this.VerifyCodeInputList.IndexOf(textBox);
            if (!string.IsNullOrEmpty(textBox.Text) && index < 5)
            {
                this.VerifyCodeInputList[index + 1].Focus();
            }
        }

        private void TxtVerifyCode_KeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            var index = this.VerifyCodeInputList.IndexOf(textBox);
            if (Keyboard.IsKeyDown(Key.Back))
            {
                textBox.Text = null;
                if (index > 0)
                {
                    this.VerifyCodeInputList[index - 1].Focus();
                }
            }
            else if (
                (Keyboard.IsKeyDown(Key.LeftCtrl)
                || Keyboard.IsKeyDown(Key.RightCtrl))
                && Keyboard.IsKeyDown(Key.V)
                )
            {
                IDataObject data = Clipboard.GetDataObject();
                if (data != null && data.GetDataPresent(DataFormats.Text))
                {
                    string clipboardText = data.GetData(DataFormats.Text)?.ToString();
                    string replacedText = clipboardText.Replace(Environment.NewLine, "").Replace(" ", "");
                    var verifyCodeList = replacedText.Take(6).ToList();
                    for (int i = 0; i < verifyCodeList.Count; i++)
                    {
                        var code = verifyCodeList[i].ToString();
                        var textInput = this.VerifyCodeInputList[i];
                        textInput.Text = code;
                        textInput.Focus();
                        textInput.CaretIndex = 1;
                    }
                }
            }
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.OnBtnReturnClick?.Invoke();
        }
    }
}
