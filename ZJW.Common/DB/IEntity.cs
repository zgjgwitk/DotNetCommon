namespace ZJW.Common.DB
{
    public interface IEntity<TPrimaryKey>
    {
    }

    public interface IEntity : IEntity<int>
    {
    }
}
