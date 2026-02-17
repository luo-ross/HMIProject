using RS.Widgets.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RS.Widgets.Services
{
    public class RSSelectService<T> where T : class, ISelectable
    {
        private readonly HashSet<T> SelectedItemsInternal = new HashSet<T>();

        public ReadOnlyCollection<T> SelectedItems => SelectedItemsInternal.ToList().AsReadOnly();

        public void SingleSelect(T item)
        {
            if (item == null)
            {
                ClearSelect();
                return;
            }

            // 如果已经是单选且是该项，则无需操作
            if (SelectedItemsInternal.Count == 1 && SelectedItemsInternal.Contains(item))
            {
                return;
            }

            ClearSelect();
            SelectedItemsInternal.Add(item);
            item.IsSelect = true;
            UpdateSingleSelectStatus();
        }

        public void MultiSelect(T item)
        {
            if (item == null) return;

            if (SelectedItemsInternal.Contains(item))
            {
                SelectedItemsInternal.Remove(item);
                item.IsSelect = false;
                item.IsSingleSelect = false;
            }
            else
            {
                SelectedItemsInternal.Add(item);
                item.IsSelect = true;
            }
            UpdateSingleSelectStatus();
        }

        public void AddSelect(T item)
        {
            if (item == null) return;

            if (!SelectedItemsInternal.Contains(item))
            {
                SelectedItemsInternal.Add(item);
                item.IsSelect = true;
                UpdateSingleSelectStatus();
            }
        }

        public void ClearSelect()
        {
            foreach (var item in SelectedItemsInternal)
            {
                item.IsSelect = false;
                item.IsSingleSelect = false;
            }
            SelectedItemsInternal.Clear();
        }

        private void UpdateSingleSelectStatus()
        {
            bool isSingle = SelectedItemsInternal.Count == 1;
            foreach (var item in SelectedItemsInternal)
            {
                item.IsSingleSelect = isSingle;
            }
        }
    }
}
