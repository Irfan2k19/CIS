using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Routing;

namespace CardIssuanceSystem.Core.Helpers
{
    public static class CustomHelper
    {
        public static string GetLimitedString(string request, int length)
        {
            if (string.IsNullOrEmpty(request))
                return new string(' ', length);
            else if (request.Length > length)
                return request.Substring(0, length);
            else if (request.Length < length)
                return request.PadRight((length), ' ');

            return request;
        }

        public static string LimitTo(this string value, int length)
        {
            return GetLimitedString(value, length);
        }

        public static ExpandoObject ToExpando(this object anonymousObject)
        {
            IDictionary<string, object> anonymousDictionary = new RouteValueDictionary(anonymousObject);
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var item in anonymousDictionary)
                expando.Add(item);
            return (ExpandoObject)expando;
        }

        public static TChild Copy<TParent, TChild>(TParent parent, TChild child)
        {
            var parentProperties = parent.GetType().GetProperties(); var childProperties = child.GetType().GetProperties();
            foreach (var parentProperty in parentProperties)
            {
                foreach (var childProperty in childProperties)
                {
                    if (parentProperty.Name == childProperty.Name && parentProperty.PropertyType == childProperty.PropertyType)
                    {
                        childProperty.SetValue(child, parentProperty.GetValue(parent));
                        break;
                    }
                }
            }

            return child;
        }

        public static string ToValidDateTime(this string value, string format = "")
        {
            string resp = null;
            int y, m, d;
            if (string.IsNullOrEmpty(value))
                resp = value;

            else if (value.Length == 8)
            {
                int.TryParse(value.Substring(0, 4), out y);
                int.TryParse(value.Substring(4, 2), out m);
                int.TryParse(value.Substring(6, 2), out d);
                resp = (new DateTime(y, m, d)).ToString(format);
            }

            return resp;
        }


        /// <summary>
        /// Converts to min date.
        /// </summary>
        /// <param name="objDt">The obj dt.</param>
        /// <returns></returns>
        public static DateTime? ConvertToMinDate(object objDt)
        {
            try
            {
                DateTime dt = DateTime.Parse("" + objDt);
                dt = new DateTime(dt.Year, dt.Month, dt.Day, 00, 00, 00);
                return dt;
            }
            catch { }
            return null;
        }


        public static DateTime? ConvertToMaxDate(object objDt)
        {
            try
            {
                DateTime dt = DateTime.Parse("" + objDt);
                dt = new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59);
                return dt;
            }
            catch { }
            return null;
        }

        public static string GetOnlyAlphabetsAndSpaces(string str)
        {
            try
            {
                return Regex.Replace(str, "[^a-zA-Z ]*", string.Empty);
            }
            catch { }
            return string.Empty;
        }

        public static IEnumerable<int> AllIndexesOf(this string str, string searchstring)
        {
            int minIndex = str.IndexOf(searchstring);
            while (minIndex != -1)
            {
                yield return minIndex;
                minIndex = str.IndexOf(searchstring, minIndex + searchstring.Length);
            }
        }

        public static int SaveConvertToInt32(object objValue, int defaultValue)
        {
            if (objValue == null)
                return defaultValue;
            int value = 0;
            if (int.TryParse(objValue.ToString(), out value))
                return value;
            else
                return defaultValue;
        }

        /// <summary>
        /// Use to get json value in string from object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        /// <summary>
        /// Use to parse json value into your object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T ParseJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

    }
}
