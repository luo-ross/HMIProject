using CommunityToolkit.Mvvm.Input;
using IdGen;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.HMI.Client.Models;
using RS.RESTfulApi;
using RS.Widgets.Interfaces;
using RS.Widgets.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace RS.HMI.Client.Views.Areas
{

    /// <summary>
    /// 用户管理视图模型
    /// </summary>
    [ServiceInjectConfig(ServiceLifetime.Scoped)]
    public class UserViewModel : CRUDViewModel<UserModel>,INavigate
    {
        private readonly IIdGenerator<long> IdGenerator;

        /// <summary>
        /// 用户启用
        /// </summary>
        public ICommand UserEnableClickCommand { get; set; }

        /// <summary>
        /// 用户禁用
        /// </summary>
        public ICommand UserDisableClickCommand { get; set; }

        /// <summary>
        /// 默认构造方法
        /// </summary>
        /// <param name="idGenerator"></param>
        public UserViewModel(IIdGenerator<long> idGenerator)
        {
            this.IdGenerator = idGenerator;
            this.UserEnableClickCommand = new RelayCommand<UserModel>(UserEnableClick, CanUserEnableClick);
            this.UserDisableClickCommand = new RelayCommand<UserModel>(UserDisableClick, CanUserDisableClick);
        }



        #region 依赖属性

        private ObservableCollection<ComboBoxItemModel<bool>> isDisableSelectList;
        /// <summary>
        /// 用户是否禁用选择
        /// </summary>
        public ObservableCollection<ComboBoxItemModel<bool>> IsDisableSelectList
        {
            get
            {
                if (isDisableSelectList == null)
                {
                    isDisableSelectList = new ObservableCollection<ComboBoxItemModel<bool>>();
                    isDisableSelectList.Add(new ComboBoxItemModel<bool>()
                    {
                        Key = true,
                        KeyDes = "已禁用",
                    });

                    isDisableSelectList.Add(new ComboBoxItemModel<bool>()
                    {
                        Key = false,
                        KeyDes = "未禁用",
                    });
                }
                return isDisableSelectList;
            }
        }

        #endregion

        #region 命令

        /// <summary>
        /// 查询命令
        /// </summary>
        public async Task SearchClick(object obj)
        {
            LoadingConfig loadingConfig = new LoadingConfig();
            var operateResult = await this.Loading.InvokeAsync(async (cancellationToken) =>
            {
                await Task.Delay(2000);
                return OperateResult.CreateSuccessResult();
            }, loadingConfig: loadingConfig);

            if (!operateResult.IsSuccess)
            {
                await this.MessageBox.ShowMessageAsync(operateResult.Message, "错误提示");
            }
        }

        /// <summary>
        /// 查询条件清除命令
        /// </summary>
        public async Task SearchClearClick(object obj)
        {
            this.ModelSearch = new UserModel();
        }

        /// <summary>
        /// 关闭命令
        /// </summary>
        public void CloseClick(object obj)
        {

        }

        /// <summary>
        /// 删除数据
        /// </summary>
        public override void DeleteClick(UserModel userModel)
        {
            //if (this.ModelSelect != null)
            //{
            //    this.ModelList.Remove(this.ModelSelect);
            //}
        }



        /// <summary>
        /// 查看数据
        /// </summary>
        public override void DetailsClick(UserModel userModel)
        {

        }

        /// <summary>
        /// 导出数据
        /// </summary>
        public override void ExportClick(ObservableCollection<UserModel>? collection)
        {

        }

        public override async Task PaginationAsync(Pagination pagination)
        {
            LoadingConfig loadingConfig = new LoadingConfig();
            var operateResult = await this.Navigate.Loading.InvokeAsync(async (cancellationToken) =>
               {
                   var dataResult = await RSAppAPI.User.GetUser.AESHttpPostAsync<Pagination, RS.Models.PageDataModel<UserModel>>(pagination, nameof(RSAppAPI));
                   if (!dataResult.IsSuccess)
                   {
                       return dataResult;
                   }
                   var pageDataModel = dataResult.Data;
                   pagination.Records = pageDataModel.Pagination.records;

                   var pageList = pageDataModel.DataList;

                   //回到UI线程更新集合
                   Application.Current.Dispatcher.Invoke(() =>
                   {
                       this.ModelList = new ObservableCollection<UserModel>(pageList);
                   });

                   return OperateResult.CreateSuccessResult();
               }, loadingConfig: loadingConfig);

            if (!operateResult.IsSuccess)
            {
                await this.MessageBox.ShowMessageAsync(operateResult.Message, "错误提示");
            }
        }



        private bool CanUserEnableClick(UserModel? userModel)
        {
            return userModel == null ? false : true;
        }

        public async void UserEnableClick(UserModel userModel)
        {
            var sdf = 1;
            await Task.Delay(3000);
        }


        private bool CanUserDisableClick(UserModel? userModel)
        {
            return userModel == null ? false : true;
        }


        public async void UserDisableClick(UserModel userModel)
        {
            var sdf = 1;
            await Task.Delay(3000);
        }

        public  override async Task OnFormSubmitAsync(UserModel modelEidt)
        {
           
        }

        #endregion

    }
}
