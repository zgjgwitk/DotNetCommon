using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using ZJW.Common.Helper;

namespace ZJW.Common.Ext
{
    public static class MvcHtmlStringExpand
    {
        /// <summary>
        /// 单选框列表
        /// </summary>
        /// <returns>
        /// @Html.RadioButtonList("rbtCancelType", (string)ViewBag.CancelTypeId, ((IEnumerable<SelectListItemExt>)ViewBag.CancelTypeList), true)
        /// </returns>
        /// <param name="Helper"></param>
        /// <param name="FieldName">控件名</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="Source"></param>
        /// <param name="IsHorizontal"></param>
        /// <param name="htmlAttributes"></param>
        public static MvcHtmlString RadioButtonList(this HtmlHelper Helper, string FieldName, string defaultValue, IEnumerable<SelectListItemExt> Source, bool IsHorizontal = false, object htmlAttributes = null)
        {
            StringBuilder sbHtml = new StringBuilder();
            FieldName = FieldName.Replace(".", "_");
            string liStyle = " style=\"padding:2px 5px;list-style:none;\" ";
            if (IsHorizontal)
            {
                liStyle = " style=\"padding:2px 5px;list-style:none; float:left;\" ";
            }
            if (Source == null || Source.Count() == 0)
            {
                return null;
            }

            if (htmlAttributes != null)
            {
                StringBuilder sbAttr = new StringBuilder();
                foreach (PropertyInfo pi in htmlAttributes.GetType().GetProperties())
                {
                    sbAttr.AppendFormat(" {0}='{1}'", pi.Name, pi.GetValue(htmlAttributes, null));
                }
                sbHtml.AppendFormat("<ul {0}>", sbAttr);
            }
            else
            {
                sbHtml.Append("<ul>");
            }
            int i = 0;
            foreach (var item in Source)
            {
                sbHtml.AppendFormat("<li {0}>", liStyle);
                string strChecked = "";
                if (!string.IsNullOrEmpty(defaultValue) && item.Value == defaultValue)
                {
                    strChecked = " checked=\"checked\" ";
                }
                else if (string.IsNullOrEmpty(defaultValue) && item.Selected)
                {
                    strChecked = " checked=\"checked\" ";
                }

                if (item.IsDisabled)
                {
                    strChecked += " disabled=\"disabled\" ";
                }
                sbHtml.AppendFormat("<label><input id=\"{0}_{1}\" name=\"{0}\" type=\"radio\" value=\"{2}\" {4} />{3}</label>", FieldName, i, item.Value, item.Text, strChecked);
                sbHtml.Append("</li>");
                i++;
            }
            sbHtml.Append("</ul>");

            return new MvcHtmlString(sbHtml.ToString());
        }

        /// <summary>
        /// 枚举转单选列表
        /// </summary>
        /// <remarks>
        /// @(Html.RadioButtonList<EnumState.OrderRefundType>("rbtnRefunType", null, true))
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="Helper"></param>
        /// <param name="FieldName">控件名</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="IsHorizontal"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcHtmlString RadioButtonList<T>(this HtmlHelper Helper
            , string FieldName, int? defaultValue = null, bool IsHorizontal = false, object htmlAttributes = null) where T : struct
        {
            var s = EnumHelper.SelectListEnum<T>(defaultValue, false);
            List<SelectListItemExt> lst = s.Select(m => new SelectListItemExt()
            {
                Text = m.Text,
                Value = m.Value,
                Selected = m.Selected
            }).ToList();
            return RadioButtonList(Helper, FieldName, defaultValue.HasValue ? defaultValue.Value.ToString() : string.Empty, lst, IsHorizontal, htmlAttributes);
        }

        /// <summary>
        /// 复选框列表
        /// </summary>
        /// <remarks>
        /// @Html.CheckBoxList("ageType", new List《SelectListItemExt》() {
        /// new SelectListItemExt(){Text="无",Value="0"},
        /// new SelectListItemExt(){Text="儿童",Value="1"},
        /// }, true)
        /// </remarks>
        /// <param name="Helper"></param>
        /// <param name="FieldName"></param>
        /// <param name="Source"></param>
        /// <param name="IsHorizontal"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcHtmlString CheckBoxList(this HtmlHelper Helper, string FieldName, IEnumerable<SelectListItemExt> Source, bool IsHorizontal = false, object htmlAttributes = null)
        {
            StringBuilder sbHtml = new StringBuilder();
            FieldName = FieldName.Replace(".", "_");
            string liStyle = " style=\"padding:2px 5px;list-style:none;\" ";
            if (IsHorizontal)
            {
                liStyle = " style=\"padding:2px 5px;list-style:none; float:left;\" ";
            }
            if (Source == null || Source.Count() == 0)
            {
                return null;
            }

            if (htmlAttributes != null)
            {
                StringBuilder sbAttr = new StringBuilder();
                foreach (PropertyInfo pi in htmlAttributes.GetType().GetProperties())
                {
                    sbAttr.AppendFormat(" {0}='{1}'", pi.Name, pi.GetValue(htmlAttributes, null));
                }
                sbHtml.AppendFormat("<ul {0}>", sbAttr);
            }
            else
            {
                sbHtml.Append("<ul>");
            }
            int i = 0;
            foreach (var item in Source)
            {
                sbHtml.AppendFormat("<li {0}>", liStyle);
                string strChecked = "";
                if (item.Selected)
                {
                    strChecked = " checked=\"checked\" ";
                }
                if (item.IsDisabled)
                {
                    strChecked += " disabled=\"disabled\" ";
                }
                sbHtml.AppendFormat("<label><input id=\"{0}_{1}\" name=\"{0}\" type=\"checkbox\" value=\"{2}\" {4} />{3}</label>", FieldName, i, item.Value, item.Text, strChecked);
                sbHtml.Append("</li>");
                i++;
            }
            sbHtml.Append("</ul>");

            return new MvcHtmlString(sbHtml.ToString());
        }

        /// <summary>
        /// 枚举转多选列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Helper"></param>
        /// <param name="FieldName"></param>
        /// <param name="IsHorizontal"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcHtmlString CheckBoxList<T>(this HtmlHelper Helper
            , string FieldName, int? defaultValue = null, bool IsHorizontal = false, object htmlAttributes = null) where T : struct
        {
            var s = EnumHelper.SelectListEnum<T>(defaultValue, false);
            List<SelectListItemExt> lst = s.Select(m => new SelectListItemExt()
            {
                Text = m.Text,
                Value = m.Value,
                Selected = m.Selected
            }).ToList();
            return CheckBoxList(Helper, FieldName, lst, IsHorizontal, htmlAttributes);
        }
    }

    public class SelectListItemExt : SelectListItem
    {
        /// <summary>
        /// 是否不可用
        /// </summary>
        public bool IsDisabled { get; set; }
    }
}
