using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NPOI.Util;
using RS.Commons;
using RS.Commons.Enums;
using RS.Models;
using RS.Widgets.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RS.Widgets.Models
{
    /// <summary>
    /// 正删改查Form表单基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CRUDViewModel<T> : ViewModelBase, IFormService
        where T : class, new()
    {
        /// <summary>
        /// 获取或设置搜索按钮点击时执行的命令
        /// </summary>
        public ICommand SearchClickCommand { get; }

        /// <summary>
        /// 获取或设置清除搜索条件按钮点击时执行的命令
        /// </summary>
        public ICommand SearchClearClickCommand { get; }

        /// <summary>
        /// 获取或设置添加新项按钮点击时执行的命令
        /// </summary>
        public ICommand AddClickCommand { get; }

        /// <summary>
        /// 获取或设置删除选中项按钮点击时执行的命令
        /// </summary>
        public ICommand DeleteClickCommand { get; }

        /// <summary>
        /// 获取或设置更新选中项按钮点击时执行的命令
        /// </summary>
        public ICommand UpdateClickCommand { get; }

        /// <summary>
        /// 获取或设置查看详情按钮点击时执行的命令
        /// </summary>
        public ICommand DetailsClickCommand { get; }

        /// <summary>
        /// 获取或设置导出数据按钮点击时执行的命令
        /// </summary>
        public ICommand ExportClickCommand { get; }

        /// <summary>
        /// 获取或设置关闭当前视图/窗口按钮点击时执行的命令
        /// </summary>
        public ICommand CloseClickCommand { get; }

        /// <summary>
        /// 获取或设置分页操作时执行的异步命令
        /// </summary>
        public ICommand PaginationCommand { get; }

        /// <summary>
        /// 获取或设置表单提交按钮点击时执行的命令
        /// </summary>
        public IAsyncRelayCommand FormSubmitCommand { get; }



        /// <summary>
        /// 默认构造方法
        /// </summary>
        public CRUDViewModel()
        {
            this.SearchClickCommand = new RelayCommand(SearchClick);
            this.SearchClearClickCommand = new RelayCommand(SearchClearClick);
            this.AddClickCommand = new RelayCommand(AddClick);
            this.DeleteClickCommand = new RelayCommand<T>(DeleteClick, CanDeleteClick);
            this.UpdateClickCommand = new RelayCommand<T>(UpdateClick, CanUpdateClick);
            this.DetailsClickCommand = new RelayCommand<T>(DetailsClick, CanDetailsClick);
            this.ExportClickCommand = new RelayCommand<ObservableCollection<T>>(ExportClick, CanExportClick);
            this.CloseClickCommand = new RelayCommand(CloseClick);
            this.PaginationCommand = new AsyncRelayCommand<Pagination>(PaginationAsync, CanPaginationAsync);
            this.FormSubmitCommand = new AsyncRelayCommand(FormSubmitAsync);
        }



        #region 依赖属性

        private CRUD _CRUD;
        /// <summary>
        /// 增删改查类型
        /// </summary>
        public CRUD CRUD
        {
            get
            {
                return _CRUD;
            }
            set
            {
                this.SetProperty(ref _CRUD, value);
            }
        }


        private T modelEdit;
        /// <summary>
        /// 编辑实体
        /// </summary>
        public T ModelEdit
        {
            get { return modelEdit; }
            set
            {
                this.SetProperty(ref modelEdit, value);
            }
        }


        private T modelSelect;
        /// <summary>
        /// 选中实体
        /// </summary>
        public T ModelSelect
        {
            get { return modelSelect; }
            set
            {
                this.SetProperty(ref modelSelect, value);
            }
        }


        private T modelSearch;
        /// <summary>
        /// 搜搜实体
        /// </summary>
        public T ModelSearch
        {
            get { return modelSearch; }
            set
            {
                this.SetProperty(ref modelSearch, value);
            }
        }


        private ObservableCollection<T> modelList;
        /// <summary>
        /// 表单数据
        /// </summary>
        public ObservableCollection<T> ModelList
        {
            get
            {
                if (modelList == null)
                {
                    modelList = new ObservableCollection<T>();
                }
                return modelList;
            }
            set
            {
                this.SetProperty(ref modelList, value);
            }
        }


        #endregion


        #region 命令事件
        /// <summary>
        /// 查询
        /// </summary>
        public virtual async void SearchClick()
        {
            LoadingConfig loadingConfig = new LoadingConfig();
            var operateResult = await this.Loading.InvokeAsync(async (cancellationToken) =>
            {

                return OperateResult.CreateSuccessResult();
            }, loadingConfig: loadingConfig);

            if (!operateResult.IsSuccess)
            {
                await this.MessageBox.ShowMessageAsync(operateResult.Message, "错误提示");
            }
        }

        /// <summary>
        /// 清除查询条件
        /// </summary>
        public virtual void SearchClearClick()
        {
            this.ModelSearch = new T();
        }

        /// <summary>
        /// 新增
        /// </summary>
        public virtual void AddClick()
        {
            this.CRUD = CRUD.Add;
            this.ModelEdit = new T();
            WeakReferenceMessenger.Default.Send(this);
        }


        public virtual bool CanDeleteClick(T? modelSelect)
        {
            return modelSelect == null ? false : true;
        }


        /// <summary>
        /// 删除
        /// </summary>
        public virtual async void DeleteClick(T? modelSelect)
        {
            await this.MessageBox.ShowMessageAsync("方法未实现");
        }


        /// <summary>
        /// 是否可以更新
        /// </summary>
        /// <returns></returns>
        public virtual bool CanUpdateClick(T? modelSelect)
        {
            return modelSelect == null ? false : true;
        }




        /// <summary>
        /// 更新
        /// </summary>
        public virtual void UpdateClick(T? t)
        {
            this.CRUD = CRUD.Update;
            this.ModelEdit = this.ModelSelect.Copy();
            WeakReferenceMessenger.Default.Send(this);
        }

        public virtual bool CanDetailsClick(T? modelSelect)
        {
            return modelSelect == null ? false : true;
        }


        /// <summary>
        /// 查看详情
        /// </summary>
        public virtual async void DetailsClick(T? modelSelect)
        {
            await this.MessageBox.ShowMessageAsync("方法未实现");
        }


        public virtual bool CanExportClick(ObservableCollection<T>? collection)
        {
            return collection != null && collection.Count > 0;
        }

        /// <summary>
        /// 导出
        /// </summary>
        public virtual async void ExportClick(ObservableCollection<T>? collection)
        {
            await this.MessageBox.ShowMessageAsync("方法未实现");
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public virtual async void CloseClick()
        {
            await this.MessageBox.ShowMessageAsync("方法未实现");
        }

        /// <summary>
        /// 是否可以分页
        /// </summary>
        /// <returns></returns>
        public virtual bool CanPaginationAsync(Pagination? pagination)
        {
            return pagination == null ? false : true;
        }

        /// <summary>
        /// 数据分页
        /// </summary>
        /// <returns></returns>
        public virtual async Task PaginationAsync(Pagination pagination)
        {
            await this.MessageBox.ShowMessageAsync("方法未实现");
        }

        /// <summary>
        /// 表单提交点击事件
        /// </summary>
        private async Task FormSubmitAsync()
        {
            var modelBase = this.ModelEdit as ModelBase;
            if (modelBase == null)
            {
                throw new ArgumentNullException(nameof(modelBase));
            }

            switch (this.CRUD)
            {
                case CRUD.Add:
                    break;
                case CRUD.Update:
                    if (string.IsNullOrEmpty(modelBase.Id))
                    {
                        throw new ArgumentNullException(nameof(modelBase.Id));
                    }
                    break;
            }
            await this.OnFormSubmitAsync(this.ModelEdit);
        }

        public abstract Task OnFormSubmitAsync(T modelEidt);
        #endregion

    }

}