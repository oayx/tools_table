using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.IO;

namespace ExcelTool
{
    /// <summary>
    /// excel工具
    /// @author hannibal
    /// </summary>
    internal class ExcelHelper
    {
        public static List<TableInfo> InitExcelData(string srcPath, Config congfig)
        {
            if (!File.Exists(srcPath))
            {
                Console.WriteLine("找不到源文件！");
                return null;
            }
            List<TableInfo> ret = new List<TableInfo>();
            Dictionary<string, TableInfo> tableInfos = new Dictionary<string, TableInfo>();
            Workbook excel = new Workbook(srcPath);
            for (int i = 0; i < excel.Worksheets.Count; i++)
            {
                Worksheet sheet = excel.Worksheets[i];
                string name = sheet.Name.Split('_')[0];
                TableInfo tableInfo = null;
                if (!tableInfos.TryGetValue(name, out tableInfo))
                {
                    tableInfo = new TableInfo();
                    tableInfo.name = name.EndsWith("Info") ? name : name + "Info";
                    tableInfo.nameSpace = congfig.NAMESPACE;
                    tableInfos[name] = tableInfo;
                }
                tableInfo.sheets.Add(sheet);
            }
            foreach (var item in tableInfos)
            {
                InitSheetData(item.Value);
                if (item.Value.columns.Count > 1)
                {
                    ret.Add(item.Value);
                }
                else
                {
                    Console.WriteLine(string.Format("{0}不符合导出规范！", item.Value.name));
                }
            }
            return ret;
        }

        static void InitSheetData(TableInfo tableInfo)
        {
            if (tableInfo.sheets.Count > 0)
            {
                Worksheet sheet = tableInfo.sheets[0];
                tableInfo.client = new HashSet<string>();
                string client = sheet.Cells.Rows[0][0].StringValue;
                string[] v = client.Split('|');
                for (int i = 0; i < v.Length; i++)
                {
                    tableInfo.client.Add(v[i]);
                }
                tableInfo.server = sheet.Cells.Rows[0][1].StringValue;
                int columnCount = sheet.Cells.MaxDataColumn + 1;
                tableInfo.columns = new List<ColumnInfo>();

                //都不导出
                if (tableInfo.client.Count == 0 && string.IsNullOrEmpty(tableInfo.server))
                    return;

                for (int i = 0; i < columnCount; i++)
                {
                    //type不填的话就是注释列
                    string type = sheet.Cells.Rows[2][i].StringValue;
                    if (!string.IsNullOrEmpty(type))
                    {
                        ColumnInfo column = new ColumnInfo();
                        column.index = i;
                        column.type = type;
                        column.name = sheet.Cells.Rows[1][i].StringValue;
                        column.defValue = sheet.Cells.Rows[4][i].StringValue;
                        column.note = sheet.Cells.Rows[5][i].StringValue;
                        string flags = sheet.Cells.Rows[3][i].StringValue.ToLower();
                        if(!string.IsNullOrWhiteSpace(flags))
                        {
                            column.flags = ExportFlags.None;
                            if (flags.Contains("c")) column.flags |= ExportFlags.Client;
                            if (flags.Contains("s")) column.flags |= ExportFlags.Server;
                        }
                        tableInfo.columns.Add(column);
                    }
                }
            }
        }
    }
}
