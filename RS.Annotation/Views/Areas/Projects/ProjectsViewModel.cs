using RS.Widgets.Models;
using RS.Annotation.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Annotation.Views.Areas
{
    public class ProjectsViewModel : ModelBase
    {
       
        private ProjectModel projectModelAdd;
        /// <summary>
        /// 项目新增
        /// </summary>
        public ProjectModel ProjectModelAdd
        {
            get
            {
                return projectModelAdd;
            }
            set
            {
                OnPropertyChanged(ref projectModelAdd, value);
            }
        }


        private ObservableCollection<ProjectModel> projectModelList;
        /// <summary>
        /// 项目列表
        /// </summary>
        public ObservableCollection<ProjectModel> ProjectModelList
        {
            get
            {
                if (projectModelList == null)
                {
                    projectModelList = new ObservableCollection<ProjectModel>();
                }
                return projectModelList;
            }
            set
            {
                OnPropertyChanged(ref projectModelList, value);
            }
        }


        private ProjectModel projectModelSelect;
        /// <summary>
        /// 项目选择
        /// </summary>
        public ProjectModel ProjectModelSelect
        {
            get
            {
                return projectModelSelect;
            }
            set
            {
                OnPropertyChanged(ref projectModelSelect, value);
            }
        }

    }
}
