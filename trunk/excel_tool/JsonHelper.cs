using Aspose.Cells;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExcelTool
{
    internal class JsonHelper
    {
        #region json数据部分
        /// <summary>
        /// 生成数组json格式
        /// </summary>
        internal static string Excel2Json(TableInfo table, ExportFlags flags)
        {
            if (table == null)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"array\":[\r\n");
            for (int i = 0; i < table.sheets.Count; i++)
            {
                Worksheet sheet = table.sheets[i];
                Excel2JsonRows(sb, sheet, table, flags);
                if (i < table.sheets.Count - 1)
                {
                    sb.Append(",");
                    sb.Append("\r\n");
                }
            }
            sb.Append("]}");
            return sb.ToString();
        }
        /// <summary>
        /// 所有行转成json
        /// </summary>
        static void Excel2JsonRows(StringBuilder sb, Worksheet sheet, TableInfo table, ExportFlags flags)
        {
            int rowCount = sheet.Cells.MaxDataRow + 1;
            int nowColumnIndex = 0;
            int nowRowIndex = 0;
            for (int i = Helper.StartContentIndex; i < rowCount; i++)
            {
                //如果id是空则跳过这一行
                string id = sheet.Cells.Rows[i][0].StringValue;
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }
                //补上分隔符[,]
                if (nowRowIndex > 0 && i < rowCount)
                {
                    sb.Append(",");
                    sb.Append("\r\n");
                }

                nowColumnIndex = 0;
                sb.Append("{");
                int columnCount = table.columns.Count;
                for (int j = 0; j < columnCount; j++)
                {
                    ColumnInfo column = table.columns[j];
                    if ((column.flags & flags) == 0) continue;

                    //如果内容为空则跳过这个字段
                    string fieldString = GetFieldString(column.type, column.name, sheet.Cells.Rows[i][column.index].StringValue, column.defValue, table);
                    if (string.IsNullOrEmpty(fieldString))
                    {
                        continue;
                    }
                    //补上分隔符[,]
                    if (nowColumnIndex > 0 && j < columnCount)
                    {
                        sb.Append(",");
                    }
                    sb.Append(fieldString);
                    nowColumnIndex++;
                }
                sb.Append("}");
                nowRowIndex++;
            }
        }

        static string GetFieldString(string type, string name, string value, string defValue, TableInfo table)
        {
            bool useDef = false;
            if (string.IsNullOrEmpty(value))
            {
                if (string.IsNullOrEmpty(defValue))
                {
                    return string.Empty;
                }
                else
                {
                    useDef = true;
                }
            }

            if (type == "string")
            {
                return string.Format("\"{0}\":\"{1}\"", name,(useDef ? defValue : value));
            }
            else if(Type.GetType(type) == null)
            {
                Type customType = Program.DataAssembly.GetType(string.Format("{0}.{1}", table.nameSpace,type));
                if (customType != null)
                {
                    bool isArray = false;
                    if (customType.IsArray)
                    {
                        customType = customType.GetElementType();
                        isArray = true;
                    }
                    if (customType.GetInterface("ICustomType") != null)
                    {
                        return string.Format("\"{0}\":{1}", name, Helper.GetCustomTypeString(customType, isArray, useDef ? defValue : value));
                    }
                }
            }

            return string.Format("\"{0}\":{1}", name, (useDef ? defValue : value));
        }
        #endregion
        /// <summary>
        /// 生成代码
        /// @author hannibal
        /// </summary>
        /// <param name="destPath">路径</param>
        /// <param name="table">表信息</param>
        /// <param name="isArray">是否数组形式的json</param>
        internal static void GeneratedCode(string destPath, TableInfo table, ExportFlags flags)
        {
            string keyType = table.columns[0].type;
            StringBuilder sb = new StringBuilder();
            //生成头部
            sb.Append(string.Format(@"

namespace {0}
{{", table.nameSpace));

            //生成Info
            GeneratedCodeInfo(sb, table, flags);

            //生成TableLib
            sb.Append(@"
    public partial class TableLib
    {");
            sb.Append(string.Format(@"
        public static JsonTable<{0}, {1}> {2} {{ get; private set; }}", keyType, table.name, RemoveTabFromName(table.name)));
            sb.Append(@"
    }");

            //生成尾部
            sb.Append(@"
}");

            //保存
            Save2File(destPath, table, sb);
        }

        /// <summary>
        /// 生成info字段
        /// </summary>
        private static void GeneratedCodeInfo(StringBuilder sb, TableInfo table, ExportFlags flags)
        {
            string keyType = table.columns[0].type;
            //生成头部
            sb.Append(string.Format(@"
    [System.Serializable]
    public class {0} : JsonTableRow<{1}>
    {{", table.name, keyType));

            //生成字段
            for (int i = 0; i < table.columns.Count; i++)
            {
                ColumnInfo colunm = table.columns[i];
                if ((colunm.flags & flags) == 0) continue;

                string note = colunm.note.Replace("\r\n", "\n        ///");
                note = note.Replace("\n", "\n        ///");
                sb.AppendLine(string.Format(@"
        ///<summary> 
        ///{0} 
        ///</summary>
        public {1} {2} {{ get; set; }}", note, colunm.type, colunm.name));
            }

            //生成ID
            sb.Append(string.Format(@"
        public override {0} ID{{ get {{ return {1}; }} }}", keyType, table.columns[0].name));

            //生成尾部
            sb.AppendLine(string.Format(@"
    }}"));
        }
        /// <summary>
        /// 保存
        /// </summary>
        private static void Save2File(string destPath, TableInfo table, StringBuilder sb)
        {
            try
            {
                if (!Directory.Exists(destPath))
                    Directory.CreateDirectory(destPath);
                File.WriteAllText(string.Format("{0}/{1}.cs", destPath, table.name), sb.ToString(), Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(string.Format("写入文件失败{0},原因:{1}", table.name, ex.ToString()));
            }
        }

        /// <summary>
        /// 把C#源代码转换成dll
        /// </summary>
        /// <param name="codes"></param>
        /// <returns></returns>
        public static string Generated_DLLBAT(List<string> codes)
        {
            StringBuilder sb = new StringBuilder(" -r:JsonFx.Json.dll -target:library");
            if (File.Exists("common/json/JsonTable.cs"))
                sb.Append(" common/json/JsonTable.cs");
            if (File.Exists("common/json/CustomTable.cs"))
                sb.Append(" common/json/CustomTable.cs");
            if (File.Exists("common/json/CustomUnityTable.cs"))
                sb.Append(" common/json/CustomUnityTable.cs");
            sb.Append(" ");
            foreach (string code in codes)
            {
                sb.Append(code).Append(" ");
            }
            sb.Append("-out:temp/table.dll");
            return sb.ToString();
        }

        /// <summary>
        /// 转换表名为info
        /// </summary>
        private static string AddTabFromName(string tableName)
        {
            if (!tableName.EndsWith("Info"))
                return tableName + "Info";
            else
                return tableName;
        }
        private static string RemoveTabFromName(string tableName)
        {
            if (tableName.EndsWith("Info"))
                return tableName.Substring(0, tableName.Length - "Info".Length);
            else
                return tableName;
        }
    }
}
