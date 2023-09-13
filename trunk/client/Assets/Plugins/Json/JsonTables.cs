using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using tab;
using UnityEngine;

namespace YX
{
    /// <summary>
    /// 配置表管理器
    /// @author hannibal
    /// @time 2019-6-13
    /// </summary>
    public class JsonTables
    {
        /// <summary>
        /// 路径
        /// </summary>
        private static string _tablePath = "data";
        /// <summary>
        /// 包括table字段的class
        /// </summary>
        private static System.Type _tableType = null;

        public static void SetRootPath(string path)
        {
            _tablePath = path;
        }
        public static void RegisterType(System.Type type)
        {
            _tableType = type;
        }

        public static void LoadAll()
        {
            //TODO
        }
        public static void UnloadAll()
        {
            if (_tableType == null)
                return;
            var properties = _tableType.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (PropertyInfo property in properties)
            {
                System.Type propertyType = property.PropertyType;
                // 检查属性的类型是否是泛型类型
                if (propertyType.IsGenericType)
                {
                    // 获取属性类型的泛型定义
                    System.Type genericTypeDefinition = propertyType.GetGenericTypeDefinition();

                    // 检查泛型定义是否是 Table<,>
                    if (genericTypeDefinition == typeof(JsonTable<,>))
                    {
                        object propertyValue = property.GetValue(null, null);
                        if(propertyValue != null)
                        {
                            try
                            {
                                MethodInfo methodInfo = propertyValue.GetType().GetMethod("Clear", BindingFlags.NonPublic | BindingFlags.Instance);
                                methodInfo.Invoke(propertyValue, null);
                            }
                            catch(System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }
                }
                else
                {//扩展
                    if(typeof(ITableEx).IsAssignableFrom(propertyType))
                    {
                        object propertyValue = property.GetValue(null, null);
                        if (propertyValue != null)
                        {
                            try
                            {
                                MethodInfo methodInfo = propertyValue.GetType().GetMethod("OnUnloaded");
                                methodInfo.Invoke(propertyValue, null);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 加载表
        /// </summary>
        /// <typeparam name="K">键</typeparam>
        /// <typeparam name="T">行类型</typeparam>
        public static void LoadTable<K, T>() where T : JsonTableRow<K>
        {
            string name = typeof(T).Name;
            Debug.Log("加载配置表:" + name);

            try
            {
                //加载
                string file_path = string.Format("{0}/{1}", _tablePath, name);
                string ta = ReadFile(file_path);
                if (string.IsNullOrEmpty(ta))
                {
                    Debug.Log("配置表加载失败:" + file_path);
                    return;
                }
                LoadFromData<K, T>(ta);
            }
            catch (System.Exception e)
            {
                Debug.Log(string.Format("配置表{0}读取失败:{1}", name, e.ToString()));
            }
        }
        /// <summary>
        /// 加载表
        /// </summary>
        /// <typeparam name="K">键</typeparam>
        /// <typeparam name="T">行类型</typeparam>
        /// <typeparam name="E">扩展表</typeparam>
        public static void LoadTable<K, T, E>() where T : JsonTableRow<K> where E : ITableEx, new()
        {
            if (_tableType == null)
                throw new System.Exception("需要先注册TableLib");

            LoadTable<K, T>();

            string name = typeof(E).Name;
            E ex = new E();
            PropertyInfo pi = _tableType.GetProperty(GetFieldInfoName(name));
            pi.SetValue(null, ex, null);
            ex.OnLoaded();
        }

        private static JsonTable<K, T> LoadFromData<K, T>(string ta) where T : JsonTableRow<K>
        {
            if (_tableType == null)
                throw new System.Exception("需要先注册TableLib");

            JsonDataSrc<K, T> dataSrc = Parse<K, T>(ta);
            if (dataSrc != null)
            {
                try
                {
                    string name = typeof(T).Name;
                    var table = new JsonTable<K, T>(dataSrc);
                    PropertyInfo pi = _tableType.GetProperty(GetFieldInfoName(name));
                    pi.SetValue(null, table, null);
                }
                catch (System.Exception ex)
                {
                    Debug.Log(string.Format("配置表构建失败:{0}", ex.ToString()));
                }
            }
            return null;
        }
        public static JsonDataSrc<K, T> Parse<K, T>(string ta) where T : JsonTableRow<K>
        {
            if (!string.IsNullOrEmpty(ta))
            {
                try
                {
                    return JsonConvert.DeserializeObject<JsonDataSrc<K, T>>(ta);
                }
                catch (System.Exception ex)
                {
                    Debug.Log(string.Format("配置表解析失败:{0}", ex.ToString()));
                }
            }
            return null;
        }
        private static string ReadFile(string fileName)
        {
            Debug.Log("ReadConfig:" + fileName);
            try
            {
                return Resources.Load<TextAsset>(fileName).text;
            }
            catch (System.Exception e)
            {
                Debug.Log(e.ToString());
            }
            return "";
        }
        /// <summary>
        /// 转换表名为info
        /// </summary>
        private static string GetFieldInfoName(string tableName)
        {
            if (tableName.EndsWith("Info"))
                tableName = tableName.Substring(0, tableName.Length - "Info".Length);
            return tableName;
        }
    }
}