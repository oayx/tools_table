using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExcelTool
{
    /// <summary>
    /// 导出pb
    /// @author hannibal
    /// </summary>
    internal class PBHelper
    {
        public static void GeneratedCode(string destPath, TableInfo table, ExportFlags flags)
        {
            string keyType = table.columns[0].type;
            StringBuilder sb = new StringBuilder();
            //生成头部
            sb.Append(string.Format(@"
using ProtoBuf;

namespace {0}
{{", table.nameSpace));
            
            sb.Append(string.Format(@"
    [ProtoContract]
    public class {0} : PBTableRow<{1}>
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
        [ProtoMember({1})]
        public {2} {3} {{ get; set; }}", note, i + 1, colunm.type, colunm.name));
            }

            //生成ID
            sb.AppendLine(string.Format(@"
        public override {0} ID{{ get {{ return {1}; }} }}
    }}",keyType,table.columns[0].name));
            
            //生成TableLib
            sb.Append(@"
    public partial class TableLib
    {");
            sb.Append(string.Format(@"
        public static PBTable<{0}, {1}> {2} {{ get; private set; }}", keyType, table.name, RemoveTabFromName(table.name)));
            sb.Append(@"
    }");

            //生成尾部
            sb.Append(@"
}");
            try
            {
                if (!Directory.Exists(destPath))
                    Directory.CreateDirectory(destPath);
                File.WriteAllText(string.Format("{0}/{1}.cs", destPath, table.name), sb.ToString(), Encoding.UTF8);
            }
            catch(System.Exception ex)
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
            StringBuilder sb = new StringBuilder(" -r:protobuf-net.dll -target:library");
            if (File.Exists("common/pb/PBTable.cs"))
                sb.Append(" common/pb/PBTable.cs");
            if (File.Exists("common/pb/CustomTable.cs"))
                sb.Append(" common/pb/CustomTable.cs");
            if (File.Exists("common/pb/CustomUnityTable.cs"))
                sb.Append(" common/pb/CustomUnityTable.cs");
            sb.Append(" ");
            foreach (string code in codes)
            {
                System.Console.WriteLine(code);
                sb.Append(code).Append(" ");
            }
            sb.Append("-out:temp/table.dll");
            return sb.ToString();
        }

        /// <summary>
        /// 转换表名
        /// </summary>
        private static string AddTabFromName(string tableName)
        {
            if (!tableName.StartsWith("Tab"))
                return "Tab" + tableName;
            else
                return tableName;
        }
        public static string RemoveTabFromName(string tableName)
        {
            if (tableName.StartsWith("Tab"))
                return tableName.Substring("Tab".Length);
            else
                return tableName;
        }
    }
}
