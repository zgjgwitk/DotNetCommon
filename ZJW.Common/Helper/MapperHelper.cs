using Nelibur.ObjectMapper;

namespace ZJW.Common.Helper
{
    public static class MapperHelper
    {
        public static T Clone<T>(T source)
        {
            var cloneObj = TinyMapper.Map<T>(source);
            return cloneObj;
        }
    }
}
