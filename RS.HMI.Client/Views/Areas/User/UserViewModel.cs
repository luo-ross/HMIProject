using IdGen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.HMI.Client.Models;
using RS.Widgets.Interface;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RS.HMI.Client.Views.Areas
{
    [ServiceInjectConfig(ServiceLifetime.Scoped)]
    public class UserViewModel : NotifyBase
    {
        private readonly IIdGenerator<long> IdGenerator;
        private readonly IRSLoading rsLoading;
        public List<UserModel> TestData = new List<UserModel>();
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

            this.LoadUserDataCommand = new AsyncRelayCommand<LoadDataArgs, int>(LoadUserDataExecuteAsync, CanLoadUserDataExecuteAsync);
            this.AddCommand = new RelayCommand(Add);
            this.DeleteCommand = new RelayCommand(Delete);
            this.UpdateCommand = new RelayCommand(Update);
            this.DetailsCommand = new RelayCommand(Details);
            this.ExportCommand = new RelayCommand(Export);
            this.CloseCommand = new RelayCommand(Close);
        }

        private void Close(object obj)
        {
          
        }


        #region CRUD实现
        private void Update(object obj)
        {
            
        }

        private void Details(object obj)
        {
            
        }

        private void Delete(object obj)
        {
           
        }

        private void Add(object obj)
        {
            
        }

        private void Export(object obj)
        {
           
        }
        #endregion
        private bool CanLoadUserDataExecuteAsync(LoadDataArgs args)
        {
            return true;
        }

        private async Task<int> LoadUserDataExecuteAsync(LoadDataArgs args)
        {
            var pageList = this.TestData.Skip((args.Page - 1) * (args.Rows)).Take(args.Rows).ToList();
            //var dataList = this.UserModelList.ToList();
            //dataList = dataList.Concat(pageList).ToList();
            // 回到UI线程更新集合
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.UserModelList = new ObservableCollection<UserModel>(pageList);
            });
            return this.TestData.Count;
        }

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
                this.OnPropertyChanged(ref userModelList, value);
            }
        }


        private ICommand loadUserDataCommand;
        /// <summary>
        /// 加载用户数据
        /// </summary>
        public ICommand LoadUserDataCommand
        {
            get { return loadUserDataCommand; }
            set
            {
                this.OnPropertyChanged(ref loadUserDataCommand, value);
            }
        }

        #region CRUD命令

        private ICommand adCommand;
        /// <summary>
        /// 新增数据
        /// </summary>
        public ICommand AddCommand
        {
            get { return adCommand; }
            set
            {
                this.OnPropertyChanged(ref adCommand, value);
            }
        }

        private ICommand deleteCommand;
        /// <summary>
        /// 删除数据
        /// </summary>
        public ICommand DeleteCommand
        {
            get { return deleteCommand; }
            set
            {
                this.OnPropertyChanged(ref deleteCommand, value);
            }
        }

        private ICommand updateCommand;
        /// <summary>
        /// 修改数据
        /// </summary>
        public ICommand UpdateCommand
        {
            get { return updateCommand; }
            set
            {
                this.OnPropertyChanged(ref updateCommand, value);
            }
        }

        private ICommand detailsCommand;
        /// <summary>
        /// 查看数据
        /// </summary>
        public ICommand DetailsCommand
        {
            get { return detailsCommand; }
            set
            {
                this.OnPropertyChanged(ref detailsCommand, value);
            }
        }


        private ICommand exportCommand;
        /// <summary>
        /// 导出数据
        /// </summary>
        public ICommand ExportCommand
        {
            get { return exportCommand; }
            set
            {
                this.OnPropertyChanged(ref exportCommand, value);
            }
        }


        private ICommand closeCommand;
        /// <summary>
        /// 关闭命令
        /// </summary>
        public ICommand CloseCommand
        {
            get { return closeCommand; }
            set
            {
                this.OnPropertyChanged(ref closeCommand, value);
            }
        }

        #endregion

    }
}
