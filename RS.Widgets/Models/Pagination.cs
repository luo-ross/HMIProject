﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace RS.Widgets.Models
{
    /// <summary>
    /// 分页信息 分页核心  
    /// </summary>
    public partial class Pagination : NotifyBase
    {

        public event Action<Pagination> OnRowsChanged;
        public event Action<Pagination> OnPageChanged;

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
                if (this. SetProperty(ref _Rows, value))
                {
                    Page = 1;
                    this.OnRowsChanged?.Invoke(this);
                }
                this.OnPropertyChanged(nameof(Total));
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

                if (this.SetProperty(ref _Page, value))
                {
                    this.OnPageChanged?.Invoke(this);
                }
            }
        }


        /// <summary>
        /// 排序列  
        /// </summary>
        [ObservableProperty]
        private string sidx = "Id";
      
        


        private List<int> rowlist;
        /// <summary>
        /// 行数 
        /// </summary>
        public List<int> Rowlist
        {
            get
            {
                if (rowlist == null)
                {
                    rowlist = new List<int>() { 30, 50, 70, 100 };
                }
                return rowlist;
            }
            set { this.SetProperty(ref rowlist, value); }
        }


        /// <summary>
        /// 排序类型  
        /// </summary>
        [ObservableProperty]
        private string sord = "desc";
       
      

        private int _Records;
        /// <summary>
        /// 总记录数  
        /// </summary>
        public int Records
        {
            get { return _Records; }
            set
            {
                this.SetProperty(ref _Records, value);
                this.OnPropertyChanged(nameof(Total));
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
