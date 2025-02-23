using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text;

namespace RS.Commons.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// 转Json  
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns></returns>
        public static string ToJson(this object obj)
        {
            var timeConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };
            return JsonConvert.SerializeObject(obj, timeConverter);
        }
        /// <summary>
        /// 根据格式转换Json  
        /// </summary>
        /// <param name="obj">待转换的Object</param>
        /// <param name="datetimeformats">时间格式</param>
        /// <returns></returns>
        public static string ToJson(this object obj, string datetimeformats)
        {
            var timeConverter = new IsoDateTimeConverter { DateTimeFormat = datetimeformats };
            return JsonConvert.SerializeObject(obj, timeConverter);
        }

        /// <summary>
        /// 将Json转换成实体 
        /// </summary>
        /// <typeparam name="T">要转换的实体类</typeparam>
        /// <param name="Json">待被转换的Json数据</param>
        /// <returns></returns>
        public static T ToObject<T>(this string Json)
        {
            return Json == null ? default(T) : JsonConvert.DeserializeObject<T>(Json);
        }

        public static T ToObject<T>(this string Json, params JsonConverter[] converters)
        {
            return Json == null ? default(T) : JsonConvert.DeserializeObject<T>(Json, converters);
        }

        /// <summary>
        /// 将Json转换为Object 
        /// </summary>
        /// <param name="Json">Json字符串</param>
        /// <returns></returns>
        public static JObject ToJObject(this string Json)
        {
            return Json == null ? JObject.Parse("{}") : JObject.Parse(Json.Replace("&nbsp;", ""));
        }

        public static string ToQueryString<T>(this T entity)
        {
            StringBuilder query = new StringBuilder("?");

            PropertyInfo[] propertys = entity.GetType().GetProperties();
            foreach (PropertyInfo pi in propertys)
            {
                if (pi.CanRead)
                {
                    query.Append($@"{pi.Name}={pi.GetValue(entity)}&");
                }
            }
            return query.ToString();
        }
    }
}
