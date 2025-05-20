using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IdGen;
using Microsoft.Extensions.DependencyInjection;
using NPOI.Util;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Enums;
using RS.Commons.Extensions;
using RS.HMI.Client.Messages;
using RS.HMI.Client.Models;
using RS.RESTfulApi;
using RS.Widgets.Controls;
using RS.Widgets.Interface;
using RS.Widgets.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace RS.HMI.Client.Views.Areas
{
    [ServiceInjectConfig(ServiceLifetime.Scoped)]
    public partial class UserViewModel : ModelBase
    {
        private readonly IIdGenerator<long> IdGenerator;
        public List<UserModel> TestData = new List<UserModel>();


        /// <summary>
        /// 默认构造方法
        /// </summary>
        /// <param name="idGenerator"></param>
        public UserViewModel(IIdGenerator<long> idGenerator)
        {
            this.IdGenerator = idGenerator;

            for (int i = 0; i < 100; i++)
            {
                this.TestData.Add(new UserModel()
                {
                    Id = this.IdGenerator.CreateId(),
                    Email = $"184596029{i}@qq.com",
                    NickName = "Ross",
                    Phone = "111111111",
                    UserPic = "sfsdfsdf",
                });
            }
        }


        #region 依赖属性

        /// <summary>
        /// 需要使用的Dialog主键
        /// </summary>
        [ObservableProperty]
        private string dialogKey = Guid.NewGuid().ToString();

        private ObservableCollection<UserModel> userModelList;
        /// <summary>
        /// 用户数据
        /// </summary>
        public ObservableCollection<UserModel> UserModelList
        {
            get
            {
                if (userModelList == null)
                {
                    userModelList = new ObservableCollection<UserModel>();
                }
                return userModelList;
            }
            set
            {
                this.SetProperty(ref userModelList, value);
            }
        }

        /// <summary>
        /// 编辑实体
        /// </summary>
        [ObservableProperty]
        private UserModel userModelEdit;


        /// <summary>
        /// 选中实体
        /// </summary>
        [ObservableProperty]
        private UserModel userModelSelect;
        #endregion

        #region 命令
        /// <summary>
        /// 关闭命令
        /// </summary>
        [RelayCommand]
        public void CloseClick(object obj)
        {

        }

        /// <summary>
        /// 新增数据
        /// </summary>
        [RelayCommand]
        public async Task AddClick(object obj)
        {
            this.UserModelEdit = new UserModel();
            WeakReferenceMessenger.Default.Send(new UserFormMessage()
            {
                CRUD = CRUD.Add,
                ViewModel = this,
                FormData = this.UserModelEdit
            });
        }


        /// <summary>
        /// 删除数据
        /// </summary>
        [RelayCommand]
        public void DeleteClick(object obj)
        {
            if (this.UserModelSelect != null)
            {
                this.UserModelList.Remove(this.UserModelSelect);
            }

        }

        /// <summary>
        /// 修改数据
        /// </summary>
        [RelayCommand]
        public void UpdateClick(object obj)
        {
            if (this.UserModelSelect == null)
            {
                return;
            }

            this.UserModelEdit = this.UserModelSelect.Copy();
            WeakReferenceMessenger.Default.Send(new UserFormMessage()
            {
                CRUD = CRUD.Update,
                ViewModel = this,
                FormData = this.UserModelEdit
            });
        }

        /// <summary>
        /// 查看数据
        /// </summary>
        [RelayCommand]
        public void DetailsClick(object obj)
        {

        }


        /// <summary>
        /// 导出数据
        /// </summary>
        [RelayCommand]
        public void ExportClick(object obj)
        {

        }

        private bool CanPaginationAsync(Pagination pagination)
        {
            return true;
        }

        [RelayCommand(CanExecute = nameof(CanPaginationAsync))]
        public async Task PaginationAsync(Pagination pagination)
        {
            var dialog = DialogManager.GetDialog(this.DialogKey, true);

            LoadingConfig loadingConfig = new LoadingConfig();
            await dialog.GetLoading().InvokeLoadingActionAsync(async (cancellationToken) =>
            {
                loadingConfig.IsShowLoadingText = true;
                loadingConfig.LoadingType = Widgets.Enums.LoadingType.RotatingAnimation;
                loadingConfig.LoadingTextStringFormat = "Hello World {0}%";
                for (int i = 0; i < 100; i++)
                {
                    await Task.Delay(20);
                    loadingConfig.Value = i;
                }
                pagination.Records = this.TestData.Count();
                var pageList = this.TestData.Skip((pagination.Page - 1) * (pagination.Rows)).Take(pagination.Rows).ToList();
                // 回到UI线程更新集合
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.UserModelList = new ObservableCollection<UserModel>(pageList);
                });
                return OperateResult.CreateSuccessResult();
            }, loadingConfig: loadingConfig);

            var userFormView = App.ServiceProvider.GetRequiredService<UserFormView>();

            await dialog.GetMessageBox().ShowMessageAsync("数据加载成功");
        }


        /// <summary>
        /// 继承基类新增
        /// </summary>
        public override async Task Submit(object obj)
        {
            if (obj is UserModel userModel)
            {
                WeakReferenceMessenger.Default.Send(new UserLoadingMessage
                {
                    LoadingFuncAsync = async (config) =>
                    {
                        //在这里向WebAPI发起请求提交数据
                        var sumitResult = await RSAppAPI.User.GetUser.AESHttpPostAsync(userModel, nameof(RSAppAPI));
                        if (!sumitResult.IsSuccess)
                        {
                            return sumitResult;
                        }

                        return OperateResult.CreateSuccessResult();
                    }
                });
            }
        }

        /// <summary>
        /// 继承基类更新
        /// </summary>
        public override async Task Update(object userModel)
        {
            var sdf = 1;
            await Task.Delay(3000);
        }

        [RelayCommand]
        public async Task UserEnableClick(object obj)
        {
            var sdf = 1;
            await Task.Delay(3000);
        }


        [RelayCommand]
        public async Task UserDisableClick(object obj)
        {
            var sdf = 1;
            await Task.Delay(3000);
        }

        #endregion

    }
}
