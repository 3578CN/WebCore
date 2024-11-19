using System.Data;
using System.Reflection;

namespace CBC.WebCore.Common.DataHelpers
{
    /// <summary>
    /// Linq 泛型集合转换成数据表。
    /// </summary>
    public class LinqHelper
    {
        /// <summary>
        /// Linq 泛型集合转换成数据表。
        /// </summary>
        /// <typeparam name="T">实体类型。</typeparam>
        /// <param name="varlist">泛型集合。</param>
        /// <returns>返回一个数据表。</returns>
        public static DataTable LINQToDataTable<T>(IEnumerable<T> varlist)
        {
            DataTable dtReturn = new DataTable();

            PropertyInfo[] oProps = null;

            if (varlist == null) return dtReturn;

            foreach (T rec in varlist)
            {
                if (oProps == null)
                {
                    oProps = rec.GetType().GetProperties();
                    foreach (PropertyInfo pi in oProps)
                    {
                        Type colType = pi.PropertyType;

                        if (colType.IsGenericType && colType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }

                        dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                    }
                }

                DataRow dr = dtReturn.NewRow();

                foreach (PropertyInfo pi in oProps)
                {
                    dr[pi.Name] = pi.GetValue(rec, null) == null ? DBNull.Value : pi.GetValue
                    (rec, null);
                }

                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }
    }
}