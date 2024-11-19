using System.Data;
using System.Text;
using System.Xml;

namespace CBC.WebCore.Common.DataHelpers
{
    /// <summary>
    /// Xml 数据操作类。
    /// </summary>
    public class XmlHelper
    {
        #region 将数据对象转换成 Xml

        /// <summary>
        /// 将 DataTable 对象转换成 Xml 字符串。
        /// </summary>
        /// <param name="dt">DataTable 对象。</param>
        /// <returns>返回 Xml 字符串。</returns>
        public static string DataTableToXml(DataTable dt)
        {
            if (dt != null)
            {
                MemoryStream ms = null;
                XmlTextWriter XmlWt = null;
                try
                {
                    ms = new MemoryStream();
                    //根据 ms 实例化 XmlWt。
                    XmlWt = new XmlTextWriter(ms, Encoding.Unicode);
                    //获取 ds 中的数据。
                    dt.WriteXml(XmlWt);
                    int count = (int)ms.Length;
                    byte[] temp = new byte[count];
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(temp, 0, count);
                    //返回 Unicode 编码的文本。
                    UnicodeEncoding ucode = new UnicodeEncoding();
                    string returnValue = ucode.GetString(temp).Trim();
                    return returnValue;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    //释放资源。
                    if (XmlWt != null)
                    {
                        XmlWt.Close();
                        ms.Close();
                        ms.Dispose();
                    }
                }
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 将 DataSet 对象中指定的表转换成 Xml 字符串。
        /// </summary>
        /// <param name="ds">DataSet 对象。</param>
        /// <param name="index">DataSet 对象中的表索引。</param>
        /// <returns>返回 Xml 字符串。</returns>
        public static string DataSetToXml(DataSet ds, int index)
        {
            if (index != -1)
            {
                return DataTableToXml(ds.Tables[index]);
            }
            else
            {
                return DataTableToXml(ds.Tables[0]);
            }
        }

        /// <summary>
        /// 将 DataSet 对象转换成 Xml 字符串。
        /// </summary>
        /// <param name="ds">DataSet 对象。</param>
        /// <returns>返回 Xml 字符串。</returns>
        public static string DataSetToXml(DataSet ds)
        {
            return DataSetToXml(ds, -1);
        }

        /// <summary>
        /// 将 DataView 对象转换成 Xml 字符串。
        /// </summary>
        /// <param name="dv">DataView 对象。</param>
        /// <returns>返回 Xml 字符串。</returns>
        public static string DataViewToXml(DataView dv)
        {
            return DataTableToXml(dv.Table);
        }

        /// <summary>
        /// 将 DataTable 对象数据保存为 Xml 文件。
        /// </summary>
        /// <param name="dt">DataTable 对象。</param>
        /// <param name="xmlFilePath">Xml 文件。</param>
        /// <returns>返回是否保存成功。</returns>
        public static bool DataTableToXmlFile(DataTable dt, string xmlFilePath)
        {
            if (dt != null && !string.IsNullOrEmpty(xmlFilePath))
            {
                string path = xmlFilePath;
                MemoryStream ms = null;
                XmlTextWriter XmlWt = null;
                try
                {
                    ms = new MemoryStream();
                    //根据 ms 实例化 XmlWt。
                    XmlWt = new XmlTextWriter(ms, Encoding.Unicode);
                    //获取 ds 中的数据。
                    dt.WriteXml(XmlWt);
                    int count = (int)ms.Length;
                    byte[] temp = new byte[count];
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(temp, 0, count);
                    //返回 Unicode 编码的文本。
                    UnicodeEncoding ucode = new UnicodeEncoding();
                    //写文件。
                    StreamWriter sw = new StreamWriter(path);
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    sw.WriteLine(ucode.GetString(temp).Trim());
                    sw.Close();
                    return true;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    //释放资源。
                    if (XmlWt != null)
                    {
                        XmlWt.Close();
                        ms.Close();
                        ms.Dispose();
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 将 DataSet 对象中指定的表转换成 Xml 文件。
        /// </summary>
        /// <param name="ds">DataSet 对象。</param>
        /// <param name="xmlFilePath">Xml 文件。</param>
        /// <param name="index">DataSet 对象中的表索引。</param>
        /// <returns>返回是否保存成功。</returns>
        public static bool DataSetToXmlFile(DataSet ds, string xmlFilePath, int index)
        {
            if (index != -1)
            {
                return DataTableToXmlFile(ds.Tables[index], xmlFilePath);
            }
            else
            {
                return DataTableToXmlFile(ds.Tables[0], xmlFilePath);
            }
        }

        /// <summary>
        /// 将 DataSet 对象转换成 Xml 文件。
        /// </summary>
        /// <param name="ds">DataSet 对象。</param>
        /// <param name="xmlFilePath">Xml 文件。</param>
        /// <returns>返回是否保存成功。</returns>
        public static bool DataSetToXmlFile(DataSet ds, string xmlFilePath)
        {
            return DataSetToXmlFile(ds, xmlFilePath, -1);
        }

        /// <summary>
        /// 将 DataView 对象转换成 Xml 文件。
        /// </summary>
        /// <param name="dv">DataView 对象。</param>
        /// <param name="xmlFilePath">Xml文件。</param>
        /// <returns>返回是否保存成功。</returns>
        public static bool DataViewToXmlFile(DataView dv, string xmlFilePath)
        {
            return DataTableToXmlFile(dv.Table, xmlFilePath);
        }

        #endregion

        #region 将 Xml 转换成数据对象

        /// <summary>
        /// 将 Xml 字符串转换成 DataSet 对象。
        /// </summary>
        /// <param name="xmlStr">Xml 内容字符串。</param>
        /// <returns>返回一个数据集。</returns>
        public static DataSet XmlToDataSet(string xmlStr)
        {
            if (!string.IsNullOrEmpty(xmlStr))
            {
                StringReader StrStream = null;
                XmlTextReader Xmlrdr = null;
                try
                {
                    DataSet ds = new DataSet();
                    //读取字符串中的信息。
                    StrStream = new StringReader(xmlStr);
                    //获取 StrStream 中的数据。
                    Xmlrdr = new XmlTextReader(StrStream);
                    //ds 获取 Xmlrdr 中的数据。
                    ds.ReadXml(Xmlrdr);
                    return ds;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    //释放资源。
                    if (Xmlrdr != null)
                    {
                        Xmlrdr.Close();
                        StrStream.Close();
                        StrStream.Dispose();
                    }
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 将 Xml 字符串转换成 DataTable 对象。
        /// </summary>
        /// <param name="xmlStr">Xml 字符串。</param>
        /// <param name="index">表索引。</param>
        /// <returns>返回一个数据表。</returns>
        public static DataTable XmlToDataTable(string xmlStr, int index)
        {
            return XmlToDataSet(xmlStr).Tables[index];
        }

        /// <summary>
        /// 将 Xml 字符串转换成 DataTable 对象。
        /// </summary>
        /// <param name="xmlStr">Xml 字符串。</param>
        /// <returns>返回一个数据表。</returns>
        public static DataTable XmlToDataTable(string xmlStr)
        {
            return XmlToDataSet(xmlStr).Tables[0];
        }

        /// <summary>
        /// 读取 Xml 文件，并转换成 DataSet 对象。
        /// </summary>
        /// <param name="xmlFilePath">Xml 文件。</param>
        /// <returns>返回一个数据集。</returns>
        public static DataSet XmlFileToDataSet(string xmlFilePath)
        {
            if (!string.IsNullOrEmpty(xmlFilePath))
            {
                string path = xmlFilePath;
                StringReader StrStream = null;
                XmlTextReader Xmlrdr = null;
                try
                {
                    XmlDocument xmldoc = new XmlDocument();
                    //根据地址加载 Xml 文件。
                    xmldoc.Load(path);

                    DataSet ds = new DataSet();
                    //读取文件中的字符流。
                    StrStream = new StringReader(xmldoc.InnerXml);
                    //获取 StrStream 中的数据。
                    Xmlrdr = new XmlTextReader(StrStream);
                    //ds 获取 Xmlrdr 中的数据。
                    ds.ReadXml(Xmlrdr);
                    return ds;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    //释放资源。
                    if (Xmlrdr != null)
                    {
                        Xmlrdr.Close();
                        StrStream.Close();
                        StrStream.Dispose();
                    }
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 读取 Xml 文件，并转换成 DataTable 对象。
        /// </summary>
        /// <param name="xmlFilePath">Xml 文件。</param>
        /// <param name="index">表索引。</param>
        /// <returns>返回一个数据表。</returns>
        public static DataTable XmlFileToDataTable(string xmlFilePath, int index)
        {
            return XmlFileToDataSet(xmlFilePath).Tables[index];
        }

        /// <summary>
        /// 读取Xml文件，并转换成 DataTable 对象。
        /// </summary>
        /// <param name="xmlFilePath">Xml 文件。</param>
        /// <returns>返回一个数据表。</returns>
        public static DataTable XmlFileToDataTable(string xmlFilePath)
        {
            return XmlFileToDataSet(xmlFilePath).Tables[0];
        }

        #endregion
    }
}