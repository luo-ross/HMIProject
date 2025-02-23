using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace RS.Widgets.Models
{
    /// <summary>
    /// 分页信息 分页核心  
    /// </summary>
    public class Pagination : NotifyBase
    {

        private int _rows;
        /// <summary>
        /// 每页行数  
        /// </summary>
        public int rows
        {
            get
            {
                if (_rows == 0 || !rowlist.Contains(_rows))
                {
                    _rows = rowlist[0];
                }
                return _rows;
            }
            set
            {
                if (OnPropertyChanged(ref _rows, value))
                {
                    page = 1;
                }
                OnPropertyChanged(nameof(total));
            }
        }

        private int _page = 1;
        /// <summary>
        /// 当前页  
        /// </summary>
        public int page
        {
            get
            {
                return _page;
            }
            set
            {
                if (value <= 0)
                {
                    value = 1;
                }
                if (total > 0 && value > total)
                {
                    value = total;
                }
                OnPropertyChanged(ref _page, value);
            }
        }



        private string _sidx = "Id";
        /// <summary>
        /// 排序列  
        /// </summary>
        public string sidx
        {
            get { return _sidx; }
            set { OnPropertyChanged(ref _sidx, value); }
        }


        private List<int> _rowlist;
        /// <summary>
        /// 行数 
        /// </summary>
        public List<int> rowlist
        {
            get
            {
                if (_rowlist == null)
                {
                    _rowlist = new List<int>() { 30, 50, 70, 100 };
                }
                return _rowlist;
            }
            set { OnPropertyChanged(ref _rowlist, value); }
        }



        private string _sord = "desc";
        /// <summary>
        /// 排序类型  
        /// </summary>
        public string sord
        {
            get { return _sord; }
            set { OnPropertyChanged(ref _sord, value); }
        }


        private int _records;
        /// <summary>
        /// 总记录数  
        /// </summary>
        public int records
        {
            get { return _records; }
            set
            {
                OnPropertyChanged(ref _records, value);
                OnPropertyChanged(nameof(total));
            }
        }


        private int _total;
        /// <summary>
        /// 总页数  
        /// </summary>
        public int total
        {
            get
            {
                if (records > 0)
                {
                    _total = records % rows == 0 ? records / rows : records / rows + 1; //分页核心算法
                }
                else
                {
                    _total = 0;
                }

                return _total;
            }
        }

    }

}
