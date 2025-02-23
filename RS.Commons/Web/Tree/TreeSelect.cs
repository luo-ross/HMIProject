using System.Collections.Generic;
using System.Linq;
using System.Text;
using RS.Commons.Extensions;

namespace RS.Commons.Web.Tree
{
    /// <summary>
    /// 该类主要是生成DropdownlistTree得核心 
    /// </summary>
    public static class TreeSelect
    {
        /// <summary>
        /// 将Tree转换为Json  
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string TreeSelectJson(this List<TreeSelectModel> data, string parentId = "0", byte sortType = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append(TreeSelectJson(data, parentId, "", sortType));
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// 将Tree转换为Json 
        /// </summary>
        /// <param name="data">tree数据</param>
        /// <param name="parentId">父键</param>
        /// <param name="blank">空格</param>
        /// <returns></returns>
        public static string TreeSelectJson(List<TreeSelectModel> data, string parentId, string blank, byte sortType = 0)
        {
            StringBuilder sb = new StringBuilder();

            List<TreeSelectModel> ChildNodeList;
            switch (sortType)
            {
                case 0:
                default:
                    ChildNodeList = data.FindAll(t => t.parentId == parentId).OrderBy(t => t.sortby).ThenBy(t => t.thenby).ToList();
                    break;
                case 1:
                    ChildNodeList = data.FindAll(t => t.parentId == parentId).OrderByDescending(t => t.sortby).ThenByDescending(t => t.thenby).ToList();
                    break;
                case 2:
                    ChildNodeList = data.FindAll(t => t.parentId == parentId).OrderBy(t => t.sortby).ThenByDescending(t => t.thenby).ToList();
                    break;
                case 3:
                    ChildNodeList = data.FindAll(t => t.parentId == parentId).OrderByDescending(t => t.sortby).ThenBy(t => t.thenby).ToList();
                    break;
            }



            var tabline = "";
            if (parentId != "0")
            {
                tabline = "　　";
            }
            if (ChildNodeList.Count > 0)
            {
                tabline = tabline + blank;
            }
            foreach (TreeSelectModel entity in ChildNodeList)
            {
                entity.text = tabline + entity.text;
                string strJson = entity.ToJson();
                sb.Append(strJson);
                sb.Append(TreeSelectJson(data, entity.id, tabline));
            }
            return sb.ToString().Replace("}{", "},{");
        }


        public static List<TreeSelectModel> TreeSelecttion(this List<TreeSelectModel> data, string parentId, string blank, List<TreeSelectModel> returnList = null)
        {
            if (returnList == null)
            {
                returnList = new List<TreeSelectModel>();
            }
            var ChildNodeList = data.FindAll(t => t.parentId == parentId);
            var tabline = "";
            if (parentId != "0")
            {
                tabline = "　　";
            }
            if (ChildNodeList.Count > 0)
            {
                tabline = tabline + blank;
            }
            foreach (TreeSelectModel entity in ChildNodeList)
            {
                entity.text = tabline + entity.text;
                returnList.Add(entity);
                data.TreeSelecttion(entity.id, tabline, returnList);
            }
            return returnList;
        }
    }
}
