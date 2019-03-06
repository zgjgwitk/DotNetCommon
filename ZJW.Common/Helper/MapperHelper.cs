using Nelibur.ObjectMapper;

namespace ZJW.Common.Helper
{
    public static class MapperHelper
    {
        /// <summary>
        /// 相同实体复制
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Clone<T>(T source)
        {
            var cloneObj = TinyMapper.Map<T>(source);
            return cloneObj;
        }

        /// <summary>
        /// 实体转换
        /// </summary>
        /// <typeparam name="Tsource">源数据实体</typeparam>
        /// <typeparam name="Ttarget">目标实体</typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Ttarget Trans<Tsource, Ttarget>(Tsource source)
        {
            var targetObj = TinyMapper.Map<Tsource, Ttarget>(source);
            return targetObj;
        }
    }
}
