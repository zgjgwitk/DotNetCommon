using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace ZJW.Common.Helper
{
    /// <summary>
    /// EnumHelper
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// 获取枚举的数据源
        /// </summary>
        /// <returns>数据源</returns>
        public static List<EnumDataModel> GetEnumDataList<T>()
        {
            return EnumUtilData<T>.enumDataList;
        }

        /// <summary>
        /// 通过枚举获取描述信息
        /// </summary>
        /// <param name="enumValue">枚举字段</param>
        /// <returns>描述信息</returns>
        public static string GetDescriptionByValue<T>(int value)
        {
            return GetDescriptionByName<T>(value.ToString());
        }

        /// <summary>
        /// 通过枚举获取描述信息
        /// </summary>
        /// <param name="enumValue">枚举字段</param>
        /// <returns>描述信息</returns>
        public static string GetDescriptionByValue<T>(int? value)
        {
            return GetDescriptionByName<T>(value.ToString());
        }
        /// <summary>
        /// 通过枚举获取描述信息
        /// </summary>
        /// <param name="enumValue">枚举字段</param>
        /// <returns>描述信息</returns>
        public static string GetDescriptionByName<T>(string name)
        {
            T t = GetEnumByName<T>(name);

            return GetDescriptionByEnum<T>(t);
        }

        /// <summary>
        /// 通过枚举获取描述信息
        /// </summary>
        /// <param name="enumInstance">枚举</param>
        /// <returns>描述信息</returns>
        public static string GetDescriptionByEnum<T>(T enumInstance)
        {
            List<EnumDataModel> enumDataList = GetEnumDataList<T>();
            EnumDataModel enumData = enumDataList.Find(m => m.Value == enumInstance.GetHashCode());
            if (enumData != null)
            {
                return enumData.Description ?? string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 通过枚举值得到枚举
        /// </summary>
        /// <param name="value">枚举值</param>
        /// <returns>枚举</returns>
        public static T GetEnumByValue<T>(int value)
        {
            return GetEnumByName<T>(value.ToString());
        }

        /// <summary>
        /// 获取枚举的description的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string GetEnumValueAndDescript<T>(T e)
        {
            FieldInfo EnumInfo = e.GetType().GetField(e.ToString());
            DescriptionAttribute[] EnumAttributes = (DescriptionAttribute[])EnumInfo.
                GetCustomAttributes(typeof(DescriptionAttribute), false);
            return EnumAttributes[0].Description;
        }

        /// <summary>
        /// 通过枚举值得到枚举
        /// </summary>
        /// <param name="name">枚举值</param>
        /// <returns>枚举</returns>
        public static T GetEnumByName<T>(string name)
        {
            Type t = typeof(T);
            return (T)System.Enum.Parse(t, name);
        }

        /// <summary>
        /// 尝试转换枚举，失败则返回false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="parsed"></param>
        /// <returns></returns>
        public static bool TryToEnum<T>(object value, out T parsed) where T : struct
        {
            bool isParsed = false;
            if (System.Enum.IsDefined(typeof(T), value))
            {
                parsed = (T)System.Enum.Parse(typeof(T), value.ToString());
                isParsed = true;
            }
            else
            {
                parsed = (T)System.Enum.Parse(typeof(T), System.Enum.GetNames(typeof(T))[0]);
            }
            return isParsed;
        }

        /// <summary>
        /// 根据枚举获取下拉框
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue"></param>
        /// <param name="addChoose"></param>
        /// <param name="removeEnum">需要移除的枚举值</param>
        /// <returns></returns>
        public static List<SelectListItem> SelectListEnum<T>(int? defaultValue = null, bool addChoose = true, List<T> removeEnum = null) where T : struct
        {
            var enumSelectListItem = new List<SelectListItem>();
            if (addChoose)
            {
                var listItem = new SelectListItem { Text = "请选择", Value = "-1" };
                enumSelectListItem.Add(listItem);
            }
            var enumDataList = EnumHelper.GetEnumDataList<T>();
            foreach (var item in enumDataList)
            {
                if (removeEnum != null)
                {
                    var isNext = removeEnum.All(rItem => item.Value != rItem.GetHashCode());
                    if (!isNext)
                    {
                        continue;
                    }
                }
                bool bl = defaultValue != null && item.Value == defaultValue;
                enumSelectListItem.Add(new SelectListItem { Text = item.Description, Value = item.Value.ToString(), Selected = bl });
            }

            return enumSelectListItem;
        }

        /// <summary>
        /// 根据枚举获取下拉框
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue"></param>
        /// <param name="addChoose"></param>
        /// <param name="validEnum">有效的枚举值</param>
        /// <returns></returns>
        public static List<SelectListItem> SelectListEnumAdd<T>(int? defaultValue = null, bool addChoose = true, List<T> validEnum = null) where T : struct
        {
            var enumSelectListItem = new List<SelectListItem>();
            if (addChoose)
            {
                var listItem = new SelectListItem { Text = "请选择", Value = "-1" };
                enumSelectListItem.Add(listItem);
            }
            var enumDataList = EnumHelper.GetEnumDataList<T>();
            foreach (var item in enumDataList)
            {
                if (validEnum != null)
                {
                    var isNext = validEnum.Any(rItem => item.Value == rItem.GetHashCode());
                    if (isNext)
                    {
                        bool bl = defaultValue != null && item.Value == defaultValue;
                        enumSelectListItem.Add(new SelectListItem { Text = item.Description, Value = item.Value.ToString(), Selected = bl });
                    }
                }
            }

            return enumSelectListItem;
        }

        /// <summary>
        /// 内部实现类，缓存
        /// </summary>
        /// <typeparam name="Tenum">枚举类型</typeparam>
        private static class EnumUtilData<Tenum>
        {
            /// <summary>
            /// 缓存数据
            /// </summary>
            internal static readonly List<EnumDataModel> enumDataList;

            static EnumUtilData()
            {
                enumDataList = InitData();
            }

            /// <summary>
            /// 初始化数据，生成枚举和描述的数据表
            /// </summary>
            private static List<EnumDataModel> InitData()
            {
                List<EnumDataModel> enumDataList = new List<EnumDataModel>();

                EnumDataModel enumData = new EnumDataModel();
                Type t = typeof(Tenum);
                FieldInfo[] fieldInfoList = t.GetFields();
                foreach (FieldInfo tField in fieldInfoList)
                {
                    if (!tField.IsSpecialName)
                    {
                        enumData = new EnumDataModel();
                        enumData.Name = tField.Name;
                        enumData.Value = ((Tenum)System.Enum.Parse(t, enumData.Name)).GetHashCode();

                        DescriptionAttribute[] enumAttributelist = (DescriptionAttribute[])tField.GetCustomAttributes(typeof(DescriptionAttribute), false);
                        if (enumAttributelist != null && enumAttributelist.Length > 0)
                        {
                            enumData.Description = enumAttributelist[0].Description;
                        }
                        else
                        {
                            enumData.Description = tField.Name;
                        }
                        enumDataList.Add(enumData);
                    }
                }
                return enumDataList;
            }
        }

        /// <summary>
        /// 枚举数据实体
        /// </summary>
        public class EnumDataModel
        {
            /// <summary>
            /// get or set 枚举名称
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// get or set 枚举值
            /// </summary>
            public int Value { get; set; }

            /// <summary>
            /// get or set 枚举描述
            /// </summary>
            public string Description { get; set; }
        }
    }
}
