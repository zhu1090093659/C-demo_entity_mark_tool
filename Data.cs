using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;
    using System.IO;
    using System.Reflection;

    public class EntitiesItem
    {
        
        
        
        public string type { get; set; }
        
        
        
        public int start { get; set; }
        
        
        
        public int end { get; set; }
    }

    public class RelationsItem
    {

        public string type { get; set; }

        public int head { get; set; }

        public int tail { get; set; }
    }

    public class MyData
    {
        
        public List<string> tokens { get; set; }
        
        public List<EntitiesItem> entities { get; set; }               
        
        public List<RelationsItem> relations { get; set; }
               
        
        public int orig_id { get; set; }

        public static T JsonConvertObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }

    public class ShowData
    {
        public string? tokens { get; set; }
        public string? Target { get; set; }
        public string? Opinion { get; set; }
        public string? Relation { get; set; }

    }

    public sealed class JsonHelper
    {
        /// <summary>
        /// 将对象序列化为json格式
        /// </summary>
        /// <param name="obj">序列化对象</param>
        /// <returns>json字符串</returns>
        public static string SerializeObjct(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        /// <summary>
        /// 解析JSON字符串生成对象实体
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="json">JSON字符串</param>
        /// <returns></returns>
        public static T JsonConvertObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        /// <summary>
        /// 解析JSON字符串生成对象实体
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public static T DeserializeJsonToObject<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object obj = serializer.Deserialize(new JsonTextReader(sr), typeof(T));
            T t = obj as T;
            return t;
        }
        /// <summary>
        /// 解析JSON数组生成对象实体集合
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json数组</param>
        /// <returns>对象实体集合</returns>
        public static List<T> DeserializeJsonToList<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object obj = serializer.Deserialize(new JsonTextReader(sr), typeof(List<T>));
            List<T> list = obj as List<T>;
            return list;
        }
        /// <summary>
        /// 将JSON转数组
        /// 用法:jsonArr[0]["xxxx"]
        /// </summary>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public static JArray GetToJsonList(string json)
        {
            JArray jsonArr = (JArray)JsonConvert.DeserializeObject(json);
            return jsonArr;
        }
        /// <summary>
        /// 将DataTable转换成实体类
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="dt">DataTable</param>
        /// <returns></returns>
        public static List<T> DtConvertToModel<T>(DataTable dt) where T : new()
        {
            List<T> ts = new List<T>();
            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                foreach (PropertyInfo pi in t.GetType().GetProperties())
                {
                    if (dt.Columns.Contains(pi.Name))
                    {
                        if (!pi.CanWrite) continue;
                        var value = dr[pi.Name];
                        if (value != DBNull.Value)
                        {
                            switch (pi.PropertyType.FullName)
                            {
                                case "System.Decimal":
                                    pi.SetValue(t, decimal.Parse(value.ToString()), null);
                                    break;
                                case "System.String":
                                    pi.SetValue(t, value.ToString(), null);
                                    break;
                                case "System.Int32":
                                    pi.SetValue(t, int.Parse(value.ToString()), null);
                                    break;
                                default:
                                    pi.SetValue(t, value, null);
                                    break;
                            }
                        }
                    }
                }
                ts.Add(t);
            }
            return ts;
        }
    }

}
