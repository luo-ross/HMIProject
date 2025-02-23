namespace RS.Commons.Extensions
{
    public static class IntExtensions
    {
        /// <summary>
        /// 判断是否是奇数
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool IsOdd(this int intValue)
        {
            return (intValue & 1) == 1;
        }
    }
}
