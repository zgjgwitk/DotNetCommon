using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace ZJW.Common.Helper
{
    public class EntityHelper
    {
        public class EntityProperty
        {
            public string Key { get; set; }
            public EntityDescription Value { get; set; }
        }

        public class EntityDescription
        {
            public string Description { get; set; }
            public string FieldName { get; set; }
            public bool IsRequire { get; set; }
        }

        public static List<EntityProperty> GetPropertyDescriptions<T>(bool isName) where T : class
        {
            var list = new List<EntityProperty>();

            //System.Reflection.PropertyInfo[] properties =
            //    typeof(T).GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            //if (properties.Length <= 0)
            //{
            //    return null;
            //}
            //foreach (System.Reflection.PropertyInfo item in properties)
            //{
            //    string name = item.Name;
            //    //object value = item.GetValue(t, null);
            //    if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
            //    {
            //        tStr += string.Format("{0}:{1},", name, value);
            //    }
            //    else
            //    {
            //        getProperties(value);
            //    }
            //}

            var fields = typeof(T).GetType().GetFields();
            foreach (FieldInfo item in fields)
            {
                var e = new EntityProperty();
                DescriptionAttribute[] EnumAttributes = (DescriptionAttribute[])item.
                    GetCustomAttributes(typeof(DescriptionAttribute), false);
                e.Value = new EntityDescription();
                e.Value.Description = EnumAttributes[0].Description;
                e.Value.FieldName = item.Name;
                list.Add(e);
            }
            return list;
        }

    }
}
