namespace ZJW.Common.Ext
{
    public class ResponseInfo
    {
        public string msg { get; set; } = string.Empty;
        public int code { get; set; } = 0;
    }

    public class ResponseInfo<T> : ResponseInfo
    {
        public T result { get; set; }
    }
}
