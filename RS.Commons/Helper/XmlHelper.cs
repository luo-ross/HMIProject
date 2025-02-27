using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace RS.Commons.Helper
{
    public static class XmlHelper
    {
        public static string IndentXmlString(string strXml)
        {
            string outXml = string.Empty;
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlTextWriter xtw = new XmlTextWriter(ms, Encoding.Unicode))
                {
                    XmlDocument doc = new XmlDocument();
                    try
                    {
                        // Load the unformatted XML text string into an instance
                        // of the XML Document Object Model (DOM)
                        doc.LoadXml(strXml);

                        // Set the formatting property of the XML Text Writer to indented
                        // the text writer is where the indenting will be performed
                        xtw.Formatting = Formatting.Indented;

                        // write dom xml to the xmltextwriter
                        doc.WriteContentTo(xtw);
                        // Flush the contents of the text writer
                        // to the memory stream, which is simply a memory file
                        xtw.Flush();

                        // set to start of the memory stream (file)
                        ms.Seek(0, SeekOrigin.Begin);
                        // create a reader to read the contents of
                        // the memory stream (file)
                        StreamReader sr = new StreamReader(ms);
                        // return the formatted string to caller
                        return sr.ReadToEnd();
                    }
                    catch (Exception)
                    {
                        return string.Empty;
                    }
                }
              
            }
        }

        public static XmlDocument GetXmlDoc(string path)
        {
            XmlDocument xmlDocument = null;
            try
            {
                xmlDocument = new XmlDocument();
                xmlDocument.Load(path);
                return xmlDocument;
            }
            catch (Exception)
            {
                return xmlDocument;
            }
        }

        public static XmlDocument GetXmlDocLoadXml(string loadxml)
        {
            XmlDocument xmlDocument = null;
            try
            {
                xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(loadxml);
                return xmlDocument;
            }
            catch (Exception)
            {
                return xmlDocument;
            }
        }

        #region 反序列化 
        /// <summary> 
        /// 反序列化 
        /// </summary> 
        /// <param name="type">类型</param> 
        /// <param name="xml">XML字符串</param> 
        /// <returns></returns> 
        public static T Deserialize<T>(string xml) where T : class
        {
            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    XmlSerializer xmldes = new XmlSerializer(typeof(T));
                    return (T)xmldes.Deserialize(sr);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 将实体对象转换成XML
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="obj">实体对象</param>
        public static string XmlSerialize<T>(T obj)
        {
            try
            {
                using (StringWriter sw = new StringWriter())
                {
                    Type t = obj.GetType();
                    XmlSerializer serializer = new XmlSerializer(obj.GetType());
                    serializer.Serialize(sw, obj);
                    sw.Close();
                    return sw.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new WarningException("将实体对象转换成XML异常", ex);
            }
        }

        /// <summary> 
        /// 反序列化 
        /// </summary> 
        /// <param name="type">类型</param> 
        /// <param name="xml">XML字符串</param> 
        /// <returns></returns> 
        public static T Deserialize<T>(Stream xmlStream) where T : class
        {
            try
            {
                XmlSerializer xmldes = new XmlSerializer(typeof(T));
                return (T)xmldes.Deserialize(xmlStream);
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion
    }
}
