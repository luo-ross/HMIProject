using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Models.Widgets
{
    /// <summary>
    /// 表示一个在项目添加、删除或整个列表更新时发出通知的动态键值集合。
    /// </summary>
    /// <typeparam name="TKey">字典中键的类型。</typeparam>
    /// <typeparam name="TValue">字典中值的类型。</typeparam>
    public class ObservableDictionary<TKey, TValue>
        : CollectionBasedDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region 构造函数
        /// <summary>
        /// 初始化一个空的、具有默认初始容量并使用键类型的默认相等比较器的
        /// ObservableDictionary`2 类的新实例。
        /// </summary>
        public ObservableDictionary()
            : base()
        {
        }

        /// <summary>
        /// 初始化一个从指定 IDictionary`2 复制元素并使用键类型的默认相等比较器的
        /// ObservableDictionary`2 类的新实例。
        /// </summary>
        /// <param name="dictionary">
        /// 元素被复制到新 ObservableDictionary`2 的 IDictionary`2。
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="dictionary" /> 为 null。
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="dictionary" /> 包含一个或多个重复键。
        /// </exception>
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
            : base(dictionary)
        {
        }

        /// <summary>
        /// 初始化一个空的、具有默认初始容量并使用指定 IEqualityComparer`1 的
        /// ObservableDictionary`2 类的新实例。
        /// </summary>
        /// <param name="comparer">
        /// 比较键时要使用的 IEqualityComparer`1 实现。
        /// 如果为 null，则使用键类型的默认 EqualityComparer`1。
        /// </param>
        public ObservableDictionary(IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
        }

        /// <summary>
        /// 初始化一个从指定 IDictionary`2 复制元素并使用指定 IEqualityComparer`1 的
        /// ObservableDictionary`2 类的新实例。
        /// </summary>
        /// <param name="dictionary">
        /// 元素被复制到新 ObservableDictionary`2 的 IDictionary`2。
        /// </param>
        /// <param name="comparer">
        /// 比较键时要使用的 IEqualityComparer`1 实现。
        /// 如果为 null，则使用键类型的默认 EqualityComparer`1。
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="dictionary" /> 为 null。
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="dictionary" /> 包含一个或多个重复键。
        /// </exception>
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(dictionary, comparer)
        {
        }
        #endregion // 构造函数

        #region 字段
        private const string DictionaryName = "Dictionary";
        private const string ItemsName = "Items[]";
        private const string KeysName = "Keys[]";
        private const string ValuesName = "Values[]";
        private const string CountName = "Count";
        #endregion // 字段

        #region 事件
        /// <summary>
        /// 当项目被添加、删除、修改、移动或整个列表更新时发生。
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// 当属性值更改时发生。
        /// </summary>
        protected virtual event PropertyChangedEventHandler PropertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                PropertyChanged += value;
            }
            remove
            {
                PropertyChanged -= value;
            }
        }

        /// <summary>
        /// 使用指定的参数引发 PropertyChanged 事件。
        /// </summary>
        /// <param name="propertyName">更改的属性名称。</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 使用指定的参数引发 CollectionChanged 事件。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        private void OnCollectionAdded(KeyValuePair<TKey, TValue> changedItem, int startingIndex)
        {
            OnPropertyChanged(DictionaryName);
            OnPropertyChanged(ItemsName);
            OnPropertyChanged(CountName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItem, startingIndex));
        }

        private void OnCollectionRemoved(KeyValuePair<TKey, TValue> changedItem, int startingIndex)
        {
            OnPropertyChanged(DictionaryName);
            OnPropertyChanged(ItemsName);
            OnPropertyChanged(KeysName);
            OnPropertyChanged(ValuesName);
            OnPropertyChanged(CountName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItem, startingIndex));
        }

        private void OnCollectionMoved(KeyValuePair<TKey, TValue> changedItem, int index, int oldIndex)
        {
            OnPropertyChanged(DictionaryName);
            OnPropertyChanged(ItemsName);
            OnPropertyChanged(KeysName);
            OnPropertyChanged(ValuesName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, changedItem, index, oldIndex));
        }

        private void OnCollectionReplaced(KeyValuePair<TKey, TValue> newItem, KeyValuePair<TKey, TValue> oldItem)
        {
            OnPropertyChanged(DictionaryName);
            OnPropertyChanged(ItemsName);
            OnPropertyChanged(KeysName);
            OnPropertyChanged(ValuesName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem));
        }

        private void OnCollectionReset()
        {
            OnPropertyChanged(DictionaryName);
            OnPropertyChanged(ItemsName);
            OnPropertyChanged(KeysName);
            OnPropertyChanged(ValuesName);
            OnPropertyChanged(CountName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion // 事件

        #region 方法
        /// <summary>
        /// 用指定项目替换指定索引处的项目。
        /// </summary>
        /// <param name="index">要替换的项目的从零开始的索引。</param>
        /// <param name="item">新项目。</param>
        protected override void SetItem(int index, KeyValuePair<TKey, TValue> item)
        {
            var oldItem = this[index];
            base.SetItem(index, item);
            OnCollectionReplaced(item, oldItem);
        }

        /// <summary>
        /// 在 ObservableDictionary`2 中的指定索引处插入元素。
        /// </summary>
        /// <param name="index">
        /// 插入 item 的从零开始的索引。
        /// </param>
        /// <param name="item">
        /// 要插入的对象。
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> 小于 0。
        /// 或 <paramref name="index" /> 大于 Count。
        /// </exception>
        protected override void InsertItem(int index, KeyValuePair<TKey, TValue> item)
        {
            base.InsertItem(index, item);
            OnCollectionAdded(item, index);
        }

        /// <summary>
        /// 移除 ObservableDictionary`2 中指定索引处的元素。
        /// </summary>
        /// <param name="index">要移除的元素的索引。</param>
        protected override void RemoveItem(int index)
        {
            var removedItem = this[index];
            base.RemoveItem(index);
            OnCollectionRemoved(removedItem, index);
        }

        /// <summary>
        /// 从 ObservableDictionary`2 中移除所有元素。
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();
            OnCollectionReset();
        }
        #endregion // 方法
    }
}
