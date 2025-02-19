using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Models.Widgets
{
    public class CollectionBasedDictionary<TKey, TValue>
 : KeyedCollection<TKey, KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
    {
        #region 构造函数
        /// <summary>
        /// 初始化一个空的、具有默认初始容量并使用键类型的默认相等比较器的
        /// CollectionBasedDictionary`2 类的新实例。
        /// </summary>
        public CollectionBasedDictionary()
            : base()
        {
        }

        /// <summary>
        /// 初始化一个从指定 IDictionary`2 复制元素并使用键类型的默认相等比较器的
        /// CollectionBasedDictionary`2 类的新实例。
        /// </summary>
        /// <param name="dictionary">
        /// 元素被复制到新 CollectionBasedDictionary`2 的
        /// IDictionary`2。
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="dictionary" /> 为 null。
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="dictionary" /> 包含一个或多个重复键。
        /// </exception>
        public CollectionBasedDictionary(IDictionary<TKey, TValue> dictionary)
            : base()
        {
            foreach (var kvp in dictionary)
            {
                Add(kvp);
            }
        }

        /// <summary>
        /// 初始化一个空的、具有默认初始容量并使用指定 IEqualityComparer`1 的
        /// CollectionBasedDictionary`2 类的新实例。
        /// </summary>
        /// <param name="comparer">
        /// 比较键时要使用的 IEqualityComparer`1 实现。
        /// 如果为 null，则使用键类型的默认 EqualityComparer`1。
        /// </param>
        public CollectionBasedDictionary(IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
        }

        /// <summary>
        /// 初始化一个从指定 IDictionary`2 复制元素并使用指定 IEqualityComparer`1 的
        /// CollectionBasedDictionary`2 类的新实例。
        /// </summary>
        /// <param name="dictionary">
        /// 元素被复制到新 CollectionBasedDictionary`2 的
        /// IDictionary`2。
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
        public CollectionBasedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
            foreach (var kvp in dictionary)
            {
                Add(kvp);
            }
        }
        #endregion // 构造函数

        #region KeyedCollection<,>
        /// <summary>
        /// 从指定元素中提取键。
        /// </summary>
        /// <returns>指定元素的键。</returns>
        /// <param name="item">要从中提取键的元素。</param>
        protected override TKey GetKeyForItem(KeyValuePair<TKey, TValue> item)
        {
            return item.Key;
        }
        #endregion // KeyedCollection<,>

        #region IDictionary<>
        /// <summary>
        /// 获取包含 CollectionBasedDictionary`2 中的键的集合。
        /// </summary>
        public ICollection<TKey> Keys
        {
            get
            {
                if (Dictionary == null)
                {
                    // 如果字典未生成，则返回空数组
                    return Enumerable.Empty<TKey>().ToArray();
                }
                return Dictionary.Keys;
            }
        }

        /// <summary>
        /// 获取包含 CollectionBasedDictionary`2 中的值的集合。
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                if (Dictionary == null)
                {
                    // 如果字典未生成，则返回空数组
                    return Enumerable.Empty<TValue>().ToArray();
                }
                // 从 KeyValuePair[] 转换为 Value[]
                return Dictionary.Values
                    .Select(x => x.Value)
                    .ToArray();
            }
        }

        /// <summary>
        /// 获取或设置与指定键关联的值。
        /// </summary>
        /// <returns>
        /// 与指定键关联的值。如果未找到指定键，
        /// get 操作会抛出 KeyNotFoundException，
        /// set 操作会创建一个具有指定键的新元素。
        /// </returns>
        /// <param name="key">要获取或设置的值的键。</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> 为 null。
        /// </exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
        /// 检索了属性但集合中不存在 <paramref name="key" />。
        /// </exception>
        public new TValue this[TKey key]
        {
            get
            {
                if (Dictionary == null)
                {
                    throw new KeyNotFoundException("指定されたキーはディクショナリ内に存在しませんでした。");
                }
                return Dictionary[key].Value;
            }
            set
            {
                // 创建 KeyValuePair 并添加或替换
                var newValue = new KeyValuePair<TKey, TValue>(key, value);
                if (Dictionary.TryGetValue(key, out var oldValue))
                {
                    var index = Items.IndexOf(oldValue);
                    SetItem(index, newValue);
                }
                else
                {
                    InsertItem(Count, newValue);
                }
            }
        }

        /// <summary>
        /// 将指定的键和值添加到字典中。
        /// </summary>
        /// <param name="key">要添加的元素的键。</param>
        /// <param name="value">要添加的元素的值。对于引用类型，值可以为 null。</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> 为 null。
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// CollectionBasedDictionary`2 中已存在具有相同键的元素。
        /// </exception>
        public void Add(TKey key, TValue value)
        {
            // 创建 KeyValuePair 并添加
            var kvp = new KeyValuePair<TKey, TValue>(key, value);
            Add(kvp);
        }

        /// <summary>
        /// 确定 CollectionBasedDictionary`2 是否包含特定值。
        /// </summary>
        /// <returns>
        /// 如果 CollectionBasedDictionary`2 包含具有指定值的元素，则为 true；
        /// 否则为 false。
        /// </returns>
        /// <param name="key">
        /// 要在 CollectionBasedDictionary`2 中查找的值。
        /// 对于引用类型，值可以为 null。
        /// </param>
        public bool ContainsKey(TKey key)
        {
            return Dictionary.ContainsKey(key);
        }

        /// <summary>
        /// 获取与指定键关联的值。
        /// </summary>
        /// <returns>
        /// 如果 CollectionBasedDictionary`2 包含具有指定键的元素，则为 true；
        /// 否则为 false。
        /// </returns>
        /// <param name="key">要获取的值的键。</param>
        /// <param name="value">
        /// 当此方法返回时，如果找到键，则包含与指定键关联的值；
        /// 否则包含 <paramref name="value" /> 参数类型的默认值。
        /// 此参数未经初始化即传递。
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> 为 null。
        /// </exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            var result = Dictionary.TryGetValue(key, out var kvp);

            // 从获取的 KeyValuePair 中提取 Value
            value = kvp.Value;
            return result;
        }
        #endregion // IDictionary<>
    }
}
