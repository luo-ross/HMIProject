using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RS.Commons.Web.TreeGrid
{
    public static class TreeGrid
    {
        /// <summary>
        /// 该方法将树数据转化为Json 
        /// </summary> 
        /// <param name="data">树数据</param>
        /// <returns></returns>
        public static string TreeGridJson(this List<TreeGridModel> data, string parentId = "0", byte sortType = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{ \"rows\": [");
            sb.Append(TreeGridJson(data, -1, parentId, sortType));
            sb.Append("]}");
            return sb.ToString();
        }

        /// <summary>
        /// 递归将树转化为Json 详情科参考该方法得实例  
        /// </summary>
        /// <param name="data">树数据</param>
        /// <param name="index">处理数据得条数</param>
        /// <param name="parentId">父级</param>
        /// <returns></returns>
        private static string TreeGridJson(List<TreeGridModel> data, int index, string parentId, byte sortType = 0)
        {
            StringBuilder sb = new StringBuilder();
            List<TreeGridModel> ChildNodeList;
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
         

            if (ChildNodeList.Count > 0) { index++; }

            foreach (TreeGridModel entity in ChildNodeList)
            {
                string strJson = entity.entityJson;
                strJson = strJson.Insert(1, "\"loaded\":" + (entity.loaded == true ? false : true).ToString().ToLower() + ",");
                strJson = strJson.Insert(1, "\"expanded\":" + (entity.expanded).ToString().ToLower() + ",");
                strJson = strJson.Insert(1, "\"isLeaf\":" + (entity.isLeaf == true ? false : true).ToString().ToLower() + ",");
                strJson = strJson.Insert(1, "\"parent\":\"" + parentId + "\",");
                strJson = strJson.Insert(1, "\"level\":" + index + ",");
                sb.Append(strJson);
                sb.Append(TreeGridJson(data, index, entity.id));
            }
            return sb.ToString().Replace("}{", "},{");
        }
    }
}
