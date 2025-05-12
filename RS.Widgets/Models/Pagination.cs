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

        private int _Rows;
        /// <summary>
        /// 每页行数  
        /// </summary>
        public int Rows
        {
            get
            {
                if (_Rows == 0 || !Rowlist.Contains(_Rows))
                {
                    _Rows = Rowlist[0];
                }
                return _Rows;
            }
            set
            {
                if (OnPropertyChanged(ref _Rows, value))
                {
                    Page = 1;
                }
                OnPropertyChanged(nameof(Total));
            }
        }

        private int _Page = 1;
        /// <summary>
        /// 当前页  
        /// </summary>
        public int Page
        {
            get
            {
                return _Page;
            }
            set
            {
                if (value <= 0)
                {
                    value = 1;
                }
                if (Total > 0 && value > Total)
                {
                    value = Total;
                }
                OnPropertyChanged(ref _Page, value);
            }
        }



        private string _Sidx = "Id";
        /// <summary>
        /// 排序列  
        /// </summary>
        public string Sidx
        {
            get { return _Sidx; }
            set { OnPropertyChanged(ref _Sidx, value); }
        }


        private List<int> _Rowlist;
        /// <summary>
        /// 行数 
        /// </summary>
        public List<int> Rowlist
        {
            get
            {
                if (_Rowlist == null)
                {
                    _Rowlist = new List<int>() { 30, 50, 70, 100 };
                }
                return _Rowlist;
            }
            set { OnPropertyChanged(ref _Rowlist, value); }
        }



        private string _Sord = "desc";
        /// <summary>
        /// 排序类型  
        /// </summary>
        public string Sord
        {
            get { return _Sord; }
            set { OnPropertyChanged(ref _Sord, value); }
        }


        private int _Records;
        /// <summary>
        /// 总记录数  
        /// </summary>
        public int Records
        {
            get { return _Records; }
            set
            {
                OnPropertyChanged(ref _Records, value);
                OnPropertyChanged(nameof(Total));
            }
        }


        private int _Total;
        /// <summary>
        /// 总页数  
        /// </summary>
        public int Total
        {
            get
            {
                if (Records > 0)
                {
                    _Total = Records % Rows == 0 ? Records / Rows : Records / Rows + 1; //分页核心算法
                }
                else
                {
                    _Total = 0;
                }

                return _Total;
            }
        }

    }

}
