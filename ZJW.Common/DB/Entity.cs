using System;
using System.Linq;

namespace ZJW.Common.DB
{
    [Serializable]
    public abstract class Entity<TPrimaryKey> : IEntity<TPrimaryKey>
    {
        public static string GetTableName(Type type)
        {
            string name = string.Empty;

            var tableAttr = type.GetCustomAttributes(false).SingleOrDefault(attr => attr.GetType().Name == "TableAttribute") as dynamic;
            if (tableAttr != null)
            {
                //name = string.Format("[{0}].[dbo].[{1}]", tableAttr.DbName, tableAttr.TableName);
                name = string.Format("[{0}]", tableAttr.TableName);
            }

            return name;
        }
    }

    [Serializable]
    public abstract class Entity : Entity<int>, IEntity, IEntity<int>
    {
    }
}
