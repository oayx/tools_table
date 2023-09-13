using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ExcelTool
{
    internal class LuaHelper
    {
        /// <summary>
        /// 开启emmylua语法提示
        /// @author hannibal
        /// </summary>
        private const bool _enableEmmyLua = true;

        internal static string Excel2Lua(TableInfo table, ExportFlags flags)
        {
            if (table == null || table.columns.Count == 0)
            {
                return string.Empty;
            }
            string className = table.name;
            StringBuilder sb = new StringBuilder();

            //emmylua
            if(_enableEmmyLua)
            {
                sb.AppendLine($"---@class {className}");
                for (int i = 1; i < table.columns.Count; i++)
                {
                    ColumnInfo colunm = table.columns[i];
                    if ((colunm.flags & flags) == 0) continue;
                    sb.AppendLine($"---@field {colunm.name} {ConverToLuaType(colunm.type)}");
                }
                sb.AppendLine();
            }

            string content = "";
            for (int i = 1; i < table.columns.Count; i++)
            {
                ColumnInfo colunm = table.columns[i];
                if ((colunm.flags & flags) == 0) continue;

                if (i == table.columns.Count - 1)
                {
                    content += "['" + colunm.name + "']=" + i.ToString();
                }
                else
                {
                    content += "['" + colunm.name + "']=" + i.ToString() + ",";
                }
            }
            string title = "local title = {" + content + "}\r\n";
            sb.Append(title);

            //emmylua
            if (_enableEmmyLua)
            {
                sb.AppendLine();
                sb.AppendLine($"---@type table<{ConverToLuaType(table.columns[0].type)},{className}>");
            }
            sb.AppendLine($"local {className} = {{");

            for (int i = 0; i < table.sheets.Count; i++)
            {
                Worksheet sheet = table.sheets[i];
                Excel2Lua(sb, sheet, table, flags);
                if(i < table.sheets.Count - 1)
                {
                    sb.Append(",");
                    sb.Append("\r\n");
                }
            }

            sb.Append("\r\n");
            sb.AppendLine("}");
            sb.Append("\r\n");

            sb.Append("local mt = {__index = function (table,key)\r\n");
            sb.Append("    local temp = title[key]\r\n");
            sb.Append("    if temp then\r\n");
            sb.Append("        return table[temp]\r\n    end\r\n");
            sb.Append("    return nil\r\nend}\r\n");
            sb.Append($"for k, v in pairs({className}) do\r\n");
            sb.Append("    setmetatable(v, mt)\r\n");
            sb.Append("    v.id = k\r\n");
            sb.Append("end\r\n");
            sb.Append($"return {className}\r\n");

            return sb.ToString();
        }

        static void Excel2Lua(StringBuilder sb, Worksheet sheet, TableInfo table, ExportFlags flags)
        {
            int rowCount = sheet.Cells.MaxDataRow + 1;
            int nowColumnIndex = 0;
            int nowRowIndex = 0;
            for (int i = Helper.StartContentIndex; i < rowCount; i++)
            {
                string id = sheet.Cells.Rows[i][0].StringValue;
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }
                if (nowRowIndex > 0 && i < rowCount)
                {
                    sb.Append(",");
                    sb.Append("\r\n");
                }
                if (table.columns[0].type == "string")
                {
                    sb.Append("[\"" + sheet.Cells.Rows[i][table.columns[0].index].StringValue + "\"]={ ");
                }
                else
                {
                    sb.Append("[" + sheet.Cells.Rows[i][table.columns[0].index].StringValue + "]={ ");
                }
                nowColumnIndex = 0;
                int columnCount = table.columns.Count;
                for (int j = 1; j < columnCount; j++)
                {
                    ColumnInfo column = table.columns[j];
                    if ((column.flags & flags) == 0) continue;

                    string fieldString = GetFieldString(column.type, column.name, sheet.Cells.Rows[i][column.index].StringValue, column.defValue, table);
                    if (string.IsNullOrEmpty(fieldString))
                    {
                        continue;
                    }
                    if (nowColumnIndex > 0 && j < columnCount)
                    {
                        sb.Append(",");
                    }
                    sb.Append(fieldString);
                    nowColumnIndex++;
                }
                sb.Append(" }");
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
                    return "nil";
                }
                else
                {
                    useDef = true;
                }
            }
            if (type == "string")
            {
                string strValue = (useDef ? defValue : value);
                strValue = strValue.Replace("\n", @"\n");
                strValue = strValue.Replace("\r", @"\r");
                strValue = strValue.Replace("\"", "\\\"");
                strValue = strValue.Replace("'", @"\'");
                return "\"" + strValue + "\"";
            }
            string fieldValue = string.Format("{0}", (useDef ? defValue : value));
            if (Type.GetType(type) == null)
            {
                Type customType = Program.DataAssembly.GetType(string.Format("{0}.{1}", table.nameSpace, type));
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
                        fieldValue = Helper.GetCustomTypeString(customType, isArray, fieldValue);
                    }
                }
            }

            fieldValue = fieldValue.Replace('[', '{');
            fieldValue = fieldValue.Replace(']', '}');
            string pattern = "\"[A-Za-z0-9]\"\\s*:";
            List<string> strs = new List<string>();
            foreach (Match match in Regex.Matches(fieldValue, pattern))
            {
                strs.Add(match.Value);
            }

            for (int i = 0; i < strs.Count; i++)
            {
                string stringvalue = strs[i].Replace("\"", "");
                stringvalue = stringvalue.Replace(":", "=");
                fieldValue = fieldValue.Replace(strs[i], stringvalue);
            }

            return fieldValue;
        }

        /// <summary>
        /// C#类型转lua类型
        /// </summary>
        /// <param name="csharpType"></param>
        /// <returns></returns>
        static string ConverToLuaType(string csharpType)
        {
            string luaType = csharpType;
            luaType = luaType.Replace("byte", "number");
            luaType = luaType.Replace("short", "number");
            luaType = luaType.Replace("ushort", "number");
            luaType = luaType.Replace("int", "number");
            luaType = luaType.Replace("uint", "number");
            luaType = luaType.Replace("long", "number");
            luaType = luaType.Replace("ulong", "number");
            luaType = luaType.Replace("float", "number");
            luaType = luaType.Replace("double", "number");
            luaType = luaType.Replace("bool", "boolean");
            return luaType;
        }
    }
}
