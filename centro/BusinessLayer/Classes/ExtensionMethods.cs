using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Data.Objects;
using System.Web.Mvc;
using System.Linq.Expressions;
//using System.Web.Mvc;

namespace BusinessLayer.Classes
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Get the enum description value
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public static string ToEnumDescription(this Enum en) //ext method
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }
            return en.ToString();
        }

        /// <summary>
        /// Get the whilespace separated values of an enum
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public static string ToEnumWordify(this Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            string pascalCaseString = memInfo[0].Name;
            Regex r = new Regex("(?<=[a-z])(?<x>[A-Z])|(?<=.)(?<x>[A-Z])(?=[a-z])");
            return r.Replace(pascalCaseString, " ${x}");
        }

        /// <summary>
        /// Get the quesry hit bu\y linq to entities
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ToTraceString<T>(this IQueryable<T> t)
        {
            string sql = "";
            ObjectQuery<T> oqt = t as ObjectQuery<T>;
            if (oqt != null)
                sql = oqt.ToTraceString();
            return sql;
        }

        /// <summary>
        /// Get the int list from comma separated ids
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static IEnumerable<long> ToLongList(this string str)
        {
            if (String.IsNullOrEmpty(str))
                yield break;

            foreach (var s in str.Split(','))
            {
                int num;
                if (int.TryParse(s, out num))
                    yield return num;
            }
        }

        /// <summary>
        /// This Method is used to remove spaces and special character from a string and replace them with -(underscore)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceSpecialCharacters(this string str)
        {
            string s = Regex.Replace(Regex.Replace(str, "[\\\\/]", "-"), @"[^0-9a-zA-Z\.-]", "-");
            s = s.Replace("---", "-");
            return s.Replace("--", "-");


        }

        /// <summary>
        /// This Method is used to get an substring
        /// </summary>
        /// <param name="str"></param>
        /// <param name="StartString"></param>
        /// <param name="EndString"></param>
        /// <returns></returns>
        public static string Substring(this string str, string StartString, string EndString)
        {
            if (str.Contains(StartString))
            {
                int iStart = str.IndexOf(StartString) + StartString.Length;
                int iEnd = str.IndexOf(EndString, iStart);
                return str.Substring(iStart, (iEnd - iStart));
            }
            return null;
        }

        /// <summary>
        /// Delete all files from this folder
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static void Empty(this System.IO.DirectoryInfo directory)
        {
            if (directory.Exists)
            {
                foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
                foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
            }
        }

        public static Expression<Func<TElement, bool>> BuildContainsExpression<TElement, TValue>(
                                Expression<Func<TElement, TValue>> valueSelector, IEnumerable<TValue> values)
        {
            if (null == valueSelector) { throw new ArgumentNullException("valueSelector"); }
            if (null == values) { throw new ArgumentNullException("values"); }
            ParameterExpression p = valueSelector.Parameters.Single();
            // p => valueSelector(p) == values[0] || valueSelector(p) == ...
            if (!values.Any())
            {
                return e => false;
            }
            var equals = values.Select(value => (Expression)Expression.Equal(valueSelector.Body, Expression.Constant(value, typeof(TValue))));
            var body = equals.Aggregate<Expression>((accumulate, equal) => Expression.Or(accumulate, equal));
            return Expression.Lambda<Func<TElement, bool>>(body, p);
        }


        /// <summary>
        /// Allows case insensitive checks
        /// </summary>
        public static bool Contains(this List<string> list, string value, bool ignoreCase = false)
        {
            return ignoreCase ? list.Any(s => s.Equals(value, StringComparison.OrdinalIgnoreCase)) : list.Contains(value);
        }

    }
}
