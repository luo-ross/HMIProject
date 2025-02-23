using RS.Commons;
using RS.Widgets.Commons;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace RS.Widgets.Controls
{
    [TemplatePart(Name = nameof(PART_BtnDown), Type = typeof(Button))]
    [TemplatePart(Name = nameof(PART_BtnUp), Type = typeof(Button))]
    [TemplatePart(Name = nameof(PART_TxtInput), Type = typeof(TextBox))]
    public class RSNumbericUpDown : ContentControl
    {
        private Button? PART_BtnDown;
        private Button? PART_BtnUp;
        private TextBox? PART_TxtInput;
        private DispatcherTimer PressDispatcherTimer;
        private bool IsLongPress = false;

        private bool IsSelfValueChanged = false;
        static RSNumbericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSNumbericUpDown), new FrameworkPropertyMetadata(typeof(RSNumbericUpDown)));
        }
        public RSNumbericUpDown()
        {

        }

        /// <summary>
        /// 当值产生变化是触发
        /// </summary>
        public event Action<RSNumbericUpDown, double> OnValueInputChanged;

        /// <summary>
        /// 值变化率
        /// </summary>
        public double ValueChageRate
        {
            get { return (double)GetValue(ValueChageRateProperty); }
            set { SetValue(ValueChageRateProperty, value); }
        }

        public static readonly DependencyProperty ValueChageRateProperty =
            DependencyProperty.Register("ValueChageRate", typeof(double), typeof(RSNumbericUpDown), new PropertyMetadata(1D));



        /// <summary>
        /// 绑定输入值
        /// </summary>
        public double ValueInput
        {
            get { return (double)GetValue(ValueInputProperty); }
            set { SetValue(ValueInputProperty, value); }
        }

        public static readonly DependencyProperty ValueInputProperty =
            DependencyProperty.Register("ValueInput", typeof(double), typeof(RSNumbericUpDown), new FrameworkPropertyMetadata(0D, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueInputPropertyChanged));

        private static void OnValueInputPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsNumbericUpDown = d as RSNumbericUpDown;
            if (rsNumbericUpDown != null && rsNumbericUpDown.IsSelfValueChanged)
            {
                rsNumbericUpDown.IsSelfValueChanged = false;
                rsNumbericUpDown.OnValueInputChanged?.Invoke(rsNumbericUpDown, rsNumbericUpDown.ValueInput);
            }
        }


        /// <summary>
        /// 最大值
        /// </summary>
        public double MaxValueInput
        {
            get { return (double)GetValue(MaxValueInputProperty); }
            set { SetValue(MaxValueInputProperty, value); }
        }

        public static readonly DependencyProperty MaxValueInputProperty =
            DependencyProperty.Register("MaxValueInput", typeof(double), typeof(RSNumbericUpDown), new PropertyMetadata(double.MaxValue));



        /// <summary>
        /// 最小值
        /// </summary>
        public double MinValueInput
        {
            get { return (double)GetValue(MinValueInputProperty); }
            set { SetValue(MinValueInputProperty, value); }
        }

        public static readonly DependencyProperty MinValueInputProperty =
            DependencyProperty.Register("MinValueInput", typeof(double), typeof(RSNumbericUpDown), new PropertyMetadata(double.MinValue));




        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_BtnDown = this.GetTemplateChild(nameof(this.PART_BtnDown)) as Button;
            this.PART_BtnUp = this.GetTemplateChild(nameof(this.PART_BtnUp)) as Button;
            this.PART_TxtInput = this.GetTemplateChild(nameof(this.PART_TxtInput)) as TextBox;

            if (this.PART_BtnUp != null)
            {
                this.PART_BtnUp.PreviewMouseLeftButtonDown += PART_BtnUp_PreviewMouseLeftButtonDown;
                this.PART_BtnUp.PreviewMouseLeftButtonUp += PART_BtnUp_PreviewMouseLeftButtonUp;
                this.PART_BtnUp.MouseLeave += PART_BtnUp_MouseLeave;
            }

            if (this.PART_BtnDown != null)
            {
                this.PART_BtnDown.PreviewMouseLeftButtonDown += PART_BtnDown_PreviewMouseLeftButtonDown;
                this.PART_BtnDown.PreviewMouseLeftButtonUp += PART_BtnDown_PreviewMouseLeftButtonUp;
                this.PART_BtnDown.MouseLeave += PART_BtnDown_MouseLeave;
            }

            if (this.PART_TxtInput != null)
            {
                this.PART_TxtInput.TextChanged += PART_TxtInput_TextChanged;
                this.PART_TxtInput.PreviewKeyDown += PART_TxtInput_PreviewKeyDown;
                this.PART_TxtInput.PreviewKeyUp += PART_TxtInput_PreviewKeyUp;
                this.PART_TxtInput.GotFocus += PART_TxtInput_GotFocus;
                this.PART_TxtInput.LostFocus += PART_TxtInput_LostFocus;
                this.PART_TxtInput.KeyDown += PART_TxtInput_KeyDown;
                this.PART_TxtInput.KeyUp += PART_TxtInput_KeyUp;
            }
        }

        private void PART_TxtInput_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void PART_TxtInput_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void PART_TxtInput_LostFocus(object sender, RoutedEventArgs e)
        {
            this.ReleaseLongPressDispatcherTimer();
        }

        private void PART_TxtInput_GotFocus(object sender, RoutedEventArgs e)
        {
            this.SetTxtInputCaretIndex();
            InputMethod.SetPreferredImeState(this.PART_TxtInput, InputMethodState.Off);
            InputMethod.SetIsInputMethodEnabled(this.PART_TxtInput, false);
        }

        private void PART_TxtInput_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                this.ReleaseLongPressDispatcherTimer();
                if (!IsLongPress)
                {
                    this.ValueInputPlus();
                }
                this.SetTxtInputCaretIndex();
            }
            else if (e.Key == Key.W)
            {
                this.ReleaseLongPressDispatcherTimer();
                if (!IsLongPress)
                {
                    this.ValueInputPlus();
                }
                this.SetTxtInputCaretIndex();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                this.ReleaseLongPressDispatcherTimer();
                if (!IsLongPress)
                {
                    this.ValueInputMinus();
                }
                this.SetTxtInputCaretIndex();

            }
            else if (e.Key == Key.S)
            {
                this.ReleaseLongPressDispatcherTimer();
                if (!IsLongPress)
                {
                    this.ValueInputMinus();
                }
                this.SetTxtInputCaretIndex();
                e.Handled = true;
            }

        }

        private void PART_TxtInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!this.PART_TxtInput.IsFocused)
            {
                this.ReleaseLongPressDispatcherTimer();
                return;
            }
            this.IsSelfValueChanged = true;

            if (e.Key == Key.Up)
            {
                this.InitLongPressDispatcherTimer(this.PART_BtnUp);
                this.SetTxtInputCaretIndex();
            }
            else if (e.Key == Key.W)
            {
                this.InitLongPressDispatcherTimer(this.PART_BtnUp);
                this.SetTxtInputCaretIndex();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                this.InitLongPressDispatcherTimer(this.PART_BtnDown);
                this.SetTxtInputCaretIndex();
            }
            else if (e.Key == Key.S)
            {
                this.InitLongPressDispatcherTimer(this.PART_BtnDown);
                this.SetTxtInputCaretIndex();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {

            }

        }

        private void PART_TxtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            double newValue = this.ValueInput;
            newValue = Math.Max(this.MinValueInput, newValue);
            newValue = Math.Min(this.MaxValueInput, newValue);
            this.ValueInput = newValue;
        }

        private void PART_BtnDown_MouseLeave(object sender, MouseEventArgs e)
        {
            this.ReleaseLongPressDispatcherTimer();
        }


        private void PART_BtnDown_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.TextInputSetFocus();
            this.InitLongPressDispatcherTimer(sender);
        }
        private void PART_BtnDown_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ReleaseLongPressDispatcherTimer();
            if (!IsLongPress)
            {
                this.ValueInputPlus();
            }
            this.SetTxtInputCaretIndex();
        }



        private void PART_BtnUp_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.ReleaseLongPressDispatcherTimer();
        }

        private void PART_BtnUp_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.ReleaseLongPressDispatcherTimer();
            if (!IsLongPress)
            {
                this.ValueInputPlus();
            }
            this.SetTxtInputCaretIndex();
        }


        private void SetTxtInputCaretIndex()
        {
            if (this.PART_TxtInput.Text != null)
            {
                this.PART_TxtInput.CaretIndex = this.PART_TxtInput.Text.Length;
            }
        }

        private void ReleaseLongPressDispatcherTimer()
        {
            PressDispatcherTimer?.Stop();
            PressDispatcherTimer = null;
        }


        private void PART_BtnUp_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.TextInputSetFocus();
            this.InitLongPressDispatcherTimer(sender);
        }

        private void InitLongPressDispatcherTimer(object sender)
        {
            IsLongPress = false;
            if (PressDispatcherTimer == null)
            {
                PressDispatcherTimer = new DispatcherTimer();
                PressDispatcherTimer.Tag = sender;
                PressDispatcherTimer.Tick += PressDispatcherTimer_Tick;
                PressDispatcherTimer.Interval = TimeSpan.FromMilliseconds(200);
                PressDispatcherTimer.Start();
            }

        }

        private void PressDispatcherTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                IsLongPress = true;
                PressDispatcherTimer.Interval = TimeSpan.FromMilliseconds(20);

                if (PressDispatcherTimer.Tag == this.PART_BtnDown)
                {
                    this.ValueInputMinus();
                }
                else if (PressDispatcherTimer.Tag == this.PART_BtnUp)
                {
                    this.ValueInputPlus();
                }
            }
            catch (Exception)
            {

            }
        }

        private void ValueInputPlus()
        {
            this.IsSelfValueChanged = true;
            this.ValueInput = this.ValueInput + this.ValueChageRate;
            this.SetTxtInputCaretIndex();
        }

        private void ValueInputMinus()
        {
            this.IsSelfValueChanged = true;
            this.ValueInput = this.ValueInput - this.ValueChageRate;
            this.SetTxtInputCaretIndex();
        }

        private void TextInputSetFocus()
        {
            this.PART_TxtInput.Focus();
        }
    }
}
