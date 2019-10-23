using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL
{
    public static class ReflectionHelper
    {
        /// <summary>
        /// Gets the properties for the specified type object
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Properties</returns>
        public static IList<PropertyInfo> GetPropertiesForType<T>()
        {
            Dictionary<Type, IList<PropertyInfo>> typeDictionary = new Dictionary<Type, IList<PropertyInfo>>();
            var type = typeof(T);
            if (!typeDictionary.ContainsKey(typeof(T)))
            {
                List<PropertyInfo> propInfo = new List<PropertyInfo>();
                //type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).ToList()[8].PropertyType.GetProperties().ToList();
                //foreach (var item in type.GetProperties().ToList())
                //{
                //    if (!item.PropertyType.FullName.Contains("TabaniLMS.DataAccess"))
                //        propInfo.Add(item);
                //    else
                //    {
                //        foreach (var p in item.PropertyType.GetProperties().ToList())
                //        {
                //            propInfo.Add(p);
                //        }
                //    }
                //}
                typeDictionary.Add(type, type.GetProperties().ToList());
            }

            return typeDictionary[type];
        }
        /// <summary>
        /// Converts the data table returned by the MSL query into data list of specified type
        /// </summary>
        /// <param name="dataTable">Table to convert</param>
        /// <returns>Data list</returns>
        public static List<T> CreateGenericListFromDataTable<T>(DataTable dataTable) where T : new()
        {
            List<T> genericList = new List<T>();
            IList<PropertyInfo> properties = GetPropertiesForType<T>();

            foreach (DataRow productRow in dataTable.Rows)
            {
                T pData = CreateGenericListItemfromDataRow<T>(properties, productRow);
                genericList.Add(pData);
            }
            return genericList;
        }

        /// <summary>
        /// Converts the data table returned by the MSL query into data type of specified type
        /// </summary>
        /// <param name="dataTable">Table to convert</param>
        /// <returns>Data type</returns>
        public static T CreateGenericTypeFromDataTable<T>(DataTable dataTable) where T : new()
        {
            IList<PropertyInfo> properties = GetPropertiesForType<T>();
            return CreateGenericListItemfromDataRow<T>(properties, dataTable.Rows[0]);
        }

        /// <summary>
        /// Creates a generic item from DataRow
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="properties">Type Properties</param>
        /// <param name="dataRow">Data Row</param>
        /// <returns>Generic List Item</returns>
        private static T CreateGenericListItemfromDataRow<T>(IList<PropertyInfo> properties, DataRow dataRow) where T : new()
        {
            T pData = new T();
            foreach (var property in properties)
            {
                //Assembly asm = typeof(SomeKnownType).Assembly;
                //Type type = asm.GetType(namespaceQualifiedTypeName);
                //var t = Type.GetType(property.PropertyType.AssemblyQualifiedName);
                //IList<PropertyInfo> pp = property.GetType().GetProperties().ToList();
                try
                {
                    if (dataRow.Table.Columns.Contains(property.Name))
                    {
                        if (!dataRow[property.Name].GetType().FullName.ToLower().Equals("system.dbnull"))
                            property.SetValue(pData, dataRow[property.Name], null);

                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return pData;
        }

        public static string ToHTMLTable(this DataTable dt, string url)
        {
            var builder = new StringBuilder();
            builder.Append("<form action='" + url + "' method='post'>");
            builder.Append("<input type='hidden' name='GridHtml' /><input type='hidden' name='GridCss' /><input type='submit' id='btnExport' class='btn btn-default' value='Export to pdf' style='margin: 0px 0px 2px 21px;' /></form>");
            builder.Append("<div id='GridHtml'>");
            builder.Append("<table class='table'>");

            //add header row
            builder.Append("<thead>");
            builder.Append("<tr>");

            if (dt.Columns.Count <= 0)
                builder.Append("<th>Result</th>");

            for (int i = 0; i < dt.Columns.Count; i++)
                builder.Append("<th>" + dt.Columns[i].ColumnName + "</th>");

            builder.Append("</tr>");
            builder.Append("</thead>");

            //add rows
            builder.Append("<tbody>");
            if (dt.Rows.Count <= 0)
                builder.Append("<tr><td colspan='1'>No Record Found</td></tr>");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                builder.Append("<tr>");
                for (int j = 0; j < dt.Columns.Count; j++)
                    builder.Append("<td>" + dt.Rows[i][j].ToString() + "</td>");

                builder.Append("</tr>");
            }

            builder.Append("</tbody>");
            builder.Append("</table>");
            builder.Append("</div>");
            return builder.ToString();
        }
    }
}
