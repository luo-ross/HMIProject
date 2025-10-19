using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;

namespace RS.Widgets.Behaviors
{
    /// <summary>
    /// 用于绑定 RichTextBox 选择状态的行为
    /// </summary>
    public class RichTextBoxSelectionBehavior : Behavior<RichTextBox>
    {
        // 选中的文本内容
        public static readonly DependencyProperty SelectedTextProperty =
            DependencyProperty.Register("SelectedText", typeof(string), typeof(RichTextBoxSelectionBehavior),
                new PropertyMetadata(string.Empty, OnSelectedTextChanged));

        // 选择起始位置（字符索引）
        public static readonly DependencyProperty SelectionStartProperty =
            DependencyProperty.Register("SelectionStart", typeof(int), typeof(RichTextBoxSelectionBehavior),
                new PropertyMetadata(0, OnSelectionStartChanged));

        // 选择长度
        public static readonly DependencyProperty SelectionLengthProperty =
            DependencyProperty.Register("SelectionLength", typeof(int), typeof(RichTextBoxSelectionBehavior),
                new PropertyMetadata(0, OnSelectionLengthChanged));

        // 公开的属性
        public string SelectedText
        {
            get => (string)GetValue(SelectedTextProperty);
            set => SetValue(SelectedTextProperty, value);
        }

        public int SelectionStart
        {
            get => (int)GetValue(SelectionStartProperty);
            set => SetValue(SelectionStartProperty, value);
        }

        public int SelectionLength
        {
            get => (int)GetValue(SelectionLengthProperty);
            set => SetValue(SelectionLengthProperty, value);
        }

        // 行为附加到 RichTextBox 时
        protected override void OnAttached()
        {
            base.OnAttached();
            // 监听 RichTextBox 的选择变化事件
            AssociatedObject.SelectionChanged += RichTextBox_SelectionChanged;
        }

        // 行为脱离时清理
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= RichTextBox_SelectionChanged;
        }

        // RichTextBox 选择变化时，同步到行为的属性（供 ViewModel 读取）
        private void RichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var richTextBox = AssociatedObject;
            // 获取选中的文本
            SelectedText = new TextRange(richTextBox.Selection.Start, richTextBox.Selection.End).Text;
            // 获取选择起始位置（字符索引）
            SelectionStart = GetCharacterIndex(richTextBox, richTextBox.Selection.Start);
            // 获取选择长度
            SelectionLength = SelectedText.Length;
        }

        // 当 ViewModel 设置 SelectedText 时，更新 RichTextBox 的选择（双向绑定）
        private static void OnSelectedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (RichTextBoxSelectionBehavior)d;
            var richTextBox = behavior.AssociatedObject;
            if (richTextBox == null) return;

            string newText = e.NewValue as string;
            if (newText == null) return;

            // 查找文本并选中（简化逻辑，实际可能需要更复杂的匹配）
            var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            int startIndex = textRange.Text.IndexOf(newText);
            if (startIndex >= 0)
            {
                behavior.SelectionStart = startIndex;
                behavior.SelectionLength = newText.Length;
            }
        }

        // 当 ViewModel 设置 SelectionStart 时，更新 RichTextBox 的选择起始位置
        private static void OnSelectionStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (RichTextBoxSelectionBehavior)d;
            var richTextBox = behavior.AssociatedObject;
            if (richTextBox == null) return;

            int start = (int)e.NewValue;
            int length = behavior.SelectionLength;

            // 设置选择范围
            var startPos = GetPositionAtCharacterIndex(richTextBox, start);
            var endPos = GetPositionAtCharacterIndex(richTextBox, start + length);
            if (startPos != null && endPos != null)
            {
                richTextBox.Selection.Select(startPos, endPos);
            }
        }

        // 当 ViewModel 设置 SelectionLength 时，更新选择长度
        private static void OnSelectionLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (RichTextBoxSelectionBehavior)d;
            // 触发 SelectionStart 变化，间接更新选择范围
            behavior.SelectionStart = behavior.SelectionStart;
        }

        // 辅助方法：将字符索引转换为 TextPointer（RichTextBox 中的位置）
        private static TextPointer GetPositionAtCharacterIndex(RichTextBox richTextBox, int index)
        {
            if (index < 0) return null;

            TextPointer position = richTextBox.Document.ContentStart;
            int currentIndex = 0;

            while (position != null && currentIndex < index)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string text = position.GetTextInRun(LogicalDirection.Forward);
                    if (currentIndex + text.Length > index)
                    {
                        return position.GetPositionAtOffset(index - currentIndex);
                    }
                    currentIndex += text.Length;
                }
                else
                {
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                }
            }

            return position;
        }

        // 辅助方法：将 TextPointer 转换为字符索引
        private static int GetCharacterIndex(RichTextBox richTextBox, TextPointer position)
        {
            TextPointer start = richTextBox.Document.ContentStart;
            int index = 0;

            while (start != null && start.CompareTo(position) < 0)
            {
                if (start.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string text = start.GetTextInRun(LogicalDirection.Forward);
                    TextPointer next = start.GetPositionAtOffset(text.Length);
                    if (next.CompareTo(position) > 0)
                    {
                        return index + position.GetOffsetToPosition(next) * -1;
                    }
                    index += text.Length;
                    start = next;
                }
                else
                {
                    start = start.GetNextContextPosition(LogicalDirection.Forward);
                }
            }

            return index;
        }
    }
}
