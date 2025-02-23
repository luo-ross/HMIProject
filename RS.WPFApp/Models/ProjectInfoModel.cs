using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RS.WPFApp.Models
{
    /// <summary>
    /// 项目信息类
    /// </summary>
    public class ProjectInfoModel
    {
        private List<string> projectPathList;

        public List<string> ProjectPathList
        {
            get
            {
                if (projectPathList==null)
                {
                    projectPathList = new List<string>();
                }
                return projectPathList;
            }
            set { projectPathList = value; }
        }
    }
}
