using Newtonsoft.Json;
using System;

namespace ExcelTool
{
    internal enum ExportFlags
    {
        None = 0,
        Client = 1,
        Server = 2,
        All = Client | Server,
    }

    internal static class Helper
    {
        /// <summary>
        /// 内容起始行
        /// </summary>
        public const int StartContentIndex = 6;

        public static string GetCustomTypeString(Type type, bool isArray, string value)
        {
            var elementInst = Activator.CreateInstance(type);
            if (isArray)
            {
                if (value.StartsWith("["))
                {
                    value = value.Substring(1);
                }
                if (value.EndsWith("]"))
                {
                    value = value.Substring(0, value.Length - 1);
                }
                string[] strArr = value.Split(',');
                object[] objArr = new object[strArr.Length];
                for (int i = 0; i < strArr.Length; i++)
                {
                    var m = type.GetMethod("Analyze");
                    objArr[i] = m.Invoke(elementInst, new object[] { strArr[i] });
                }
                return JsonConvert.SerializeObject(objArr);
            }
            else
            {
                var m = type.GetMethod("Analyze");
                object obj = m.Invoke(elementInst, new object[] { value });
                return JsonConvert.SerializeObject(obj);
            }
        }
    }
}
