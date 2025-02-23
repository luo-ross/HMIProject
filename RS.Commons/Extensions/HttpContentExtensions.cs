using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RS.Commons.Extensions
{
    public static class HttpContentExtensions
    {
        /// <summary>
        /// 转StringContent
        /// </summary>
        /// <typeparam name="TPost">Post数据类型</typeparam>
        /// <param name="postModel">Post数据</param>
        /// <returns></returns>
        public static StringContent ToStringContent<TPost>(this TPost postModel)
        {
            var jsonStrPost = postModel.ToJson();
            StringContent stringContent = new StringContent(jsonStrPost, encoding: Encoding.UTF8, mediaType: "application/json");
            return stringContent;
        }
    }
}
