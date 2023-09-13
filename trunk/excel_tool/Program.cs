using Aspose.Cells;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ExcelTool
{
    /// <summary>
    /// 主逻辑
    /// @author hannibal
    /// </summary>
    public class Program
    {
        const string TempPath = "temp";
        public static Assembly DataAssembly { get; private set; }

        static void Main(string[] args)
        {
            string ExeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(ExeFolder);

            bool exportCode = false;//导出代码
            bool exportPackage = false;//导出资源
            string configPath = "./config.json";//配置表路径
            
            Console.WriteLine("参数数量:" + args.Length);
            for(int i = 0; i < args.Length; ++i)
            {
                Console.WriteLine(string.Format("参数:{0},内容:{1}", i, args[i]));
                string arg = args[i];
                string[] strArgs = arg.Split('=');
                if (strArgs.Length != 2) continue;
                switch(strArgs[0])
                {
                    case "code": exportCode = (strArgs[1] != "false" && strArgs[1] != "0"); break;
                    case "package": exportPackage = (strArgs[1] != "false" && strArgs[1] != "0"); break;
                    case "config": configPath = strArgs[1];  break;
                }
            }
            
            //判断config文件
            if (!File.Exists(configPath))
            {
                Console.WriteLine("找不到config:" + configPath);
                return;
            }

            //解析config内容
            string configBuffer = File.ReadAllText(configPath);
            Config config = JsonConvert.DeserializeObject<Config>(configBuffer);
            if(config == null)
            {
                Console.WriteLine("config:内容设置有误");
                Console.WriteLine(configBuffer);
                return;
            }

            //收集所有excel文件
            List<string> execlFiles = new List<string>();
            DirectoryInfo folder = new DirectoryInfo(config.EXCEL);
            if (folder.Exists == false)
            {
                Console.WriteLine("无法找到表格目录");
                return;
            }
            FileInfo[] childFiles = folder.GetFiles();
            foreach (FileInfo item in childFiles)
            {
                if (!Path.GetFileNameWithoutExtension(item.FullName).StartsWith("~"))
                {
                    string extension = Path.GetExtension(item.FullName);
                    if (config.EXTENSION == null || config.EXTENSION.Contains(extension))
                    {
                        execlFiles.Add(item.FullName);
                    }
                }
            }

            //读取表格文件
            List<TableInfo> tables = new List<TableInfo>();
            foreach (string path in execlFiles)
            {
                List<TableInfo> lst = ExcelHelper.InitExcelData(path, config);
                if (lst != null && lst.Count > 0)
                {
                    tables.AddRange(lst);
                }
            }

            if (tables.Count == 0)
            {
                Console.WriteLine("无法找到表格");
                return;
            }

            if (exportCode)
            {//生成c#代码
                //先删除目录下所有文件
                DeleteAllFiles(config.CLIENT_CODE_PB);
                DeleteAllFiles(config.CLIENT_CODE_JSON);
                DeleteAllFiles(config.SERVER_CODE_JSON);
                if (!string.IsNullOrEmpty(config.CLIENT_CODE_PB))
                {
                    foreach (var table in tables)
                    {
                        if (table.client.Contains("pb"))
                        {
                            PBHelper.GeneratedCode(config.CLIENT_CODE_PB, table, ExportFlags.Client);
                            Console.WriteLine(string.Format("已经生成pb C#:{0}.cs", table.name));
                        }
                    }
                }
                if (!string.IsNullOrEmpty(config.CLIENT_CODE_JSON))
                {
                    foreach (var table in tables)
                    {
                        if (table.client.Contains("json"))
                        {
                            JsonHelper.GeneratedCode(config.CLIENT_CODE_JSON, table, ExportFlags.Client);
                            Console.WriteLine(string.Format("已经生成client C#:{0}/{1}.cs", config.CLIENT_CODE_JSON, table.name));
                        }
                    }
                }
                if (!string.IsNullOrEmpty(config.SERVER_CODE_JSON))
                {
                    foreach (var table in tables)
                    {
                        if (table.server == "json")
                        {
                            JsonHelper.GeneratedCode(config.SERVER_CODE_JSON, table, ExportFlags.Server);
                            Console.WriteLine(string.Format("已经生成server C#:{0}/{1}.cs", config.SERVER_CODE_JSON, table.name));
                        }
                    }
                }
            }

            if (exportPackage)
            {//生成资源
                if (!Directory.Exists(TempPath))
                {
                    Directory.CreateDirectory(TempPath);
                }

                bool exportPB = false;
                List<string> codes = new List<string>();
                foreach (var table in tables)
                {
                    if (table.client.Contains("pb") && !string.IsNullOrEmpty(config.CLIENT_PB) && !string.IsNullOrEmpty(config.CLIENT_CODE_PB))
                    {//只有生成pb相关的才执行
                        //生成临时的code文件
                        PBHelper.GeneratedCode(TempPath, table, ExportFlags.Client);
                        codes.Add(string.Format("{0}/{1}.cs", TempPath, table.name));
                        exportPB = true;
                    }
                    else if (table.client.Contains("lua") || table.client.Contains("json") || table.server == "lua" || table.server == "json")
                    {
                        JsonHelper.GeneratedCode(TempPath, table, ExportFlags.All);
                        codes.Add(string.Format("{0}/{1}.cs", TempPath, table.name));
                    }
                }
                
                {
                    string bat = null;
                    if (exportPB)
                        bat = PBHelper.Generated_DLLBAT(codes);
                    else
                        bat = JsonHelper.Generated_DLLBAT(codes);

                    Process mcs = new Process();
                    mcs.StartInfo = new ProcessStartInfo("mcs.exe", bat);
                    mcs.StartInfo.UseShellExecute = false;
                    mcs.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    mcs.EnableRaisingEvents = true;
                    bool result = mcs.Start();
                    if(!result)
                    {
                        Console.WriteLine("执行mcs.exe失败");
                        return;
                    }
                    //读取生成的dll文件

                    mcs.Exited += (sender, e) =>
                    {
                        //先删除目录下所有文件
                        DeleteAllFiles(config.CLIENT_PB);

                        DataAssembly = Assembly.LoadFrom(string.Format("{0}/table.dll",TempPath));
                        foreach (var table in tables)
                        {
                            if (table.client.Contains("pb") && !string.IsNullOrEmpty(config.CLIENT_PB))
                            {
                                if (!Directory.Exists(config.CLIENT_PB))
                                {
                                    Directory.CreateDirectory(config.CLIENT_PB);
                                }
                                try
                                {
                                    //生成json
                                    string jsonBuffer = JsonHelper.Excel2Json(table, ExportFlags.Client);

                                    //通过反射获取PBDataSrc<V>的类型
                                    Type rowType = DataAssembly.GetType(string.Format("{0}.{1}", config.NAMESPACE,table.name));
                                    if (rowType == null)
                                    {
                                        Console.WriteLine("获取type失败:" + string.Format("{0}.{1}", config.NAMESPACE, table.name));
                                        continue;
                                    }
                                    Type dataType = typeof(PBDataSrc<>);
                                    dataType = dataType.MakeGenericType(rowType);
                                    //json反序列化成PBDataSrc<V>
                                    object obj = JsonConvert.DeserializeObject(jsonBuffer, dataType);

                                    //把PBDataSrc<V>序列化成pb
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        Serializer.NonGeneric.Serialize(ms, obj);
                                        File.WriteAllBytes(string.Format("{0}/{1}.bytes", config.CLIENT_PB, table.name), ms.ToArray());
                                        Console.WriteLine(string.Format("client : {0}打包完成", table.name));
                                    }
                                }
                                catch(System.Exception ex)
                                {
                                    Console.WriteLine("配置表解析失败:" + table.name);
                                    Console.WriteLine(ex.ToString());
                                }
                            }
                        }

                        //先删除目录下所有文件
                        DeleteAllFiles(config.CLIENT_JSON);
                        DeleteAllFiles(config.CLIENT_LUA);
                        DeleteAllFiles(config.SERVER_JSON);
                        DeleteAllFiles(config.SERVER_LUA);
                        //处理除了pb之外的表格
                        foreach (var table in tables)
                        {
                            if (table.client.Contains("json") || table.server == "json")
                            {
                                if (table.client.Contains("json") && !string.IsNullOrEmpty(config.CLIENT_JSON))
                                {
                                    string jsonBuffer = JsonHelper.Excel2Json(table, ExportFlags.Client);
                                    if (!Directory.Exists(config.CLIENT_JSON))
                                    {
                                        Directory.CreateDirectory(config.CLIENT_JSON);
                                    }
                                    string path = string.Format("{0}/{1}.bytes", config.CLIENT_JSON, table.name);
                                    File.WriteAllText(path, jsonBuffer);
                                    Console.WriteLine(string.Format("client : {0}打包完成, 保存位置:{1}", table.name, path));
                                }
                                if(table.server == "json" && !string.IsNullOrEmpty(config.SERVER_JSON))
                                {
                                    string jsonBuffer = JsonHelper.Excel2Json(table, ExportFlags.Server);
                                    if (!Directory.Exists(config.SERVER_JSON))
                                    {
                                        Directory.CreateDirectory(config.SERVER_JSON);
                                    }
                                    string path = string.Format("{0}/{1}.json", config.SERVER_JSON, table.name);
                                    File.WriteAllText(path, jsonBuffer);
                                    Console.WriteLine(string.Format("server : {0}打包完成, 保存位置:{1}", table.name, path));
                                }
                            }
                            if (table.client.Contains("lua") ||  table.server == "lua")
                            {
                                if (table.client.Contains("lua") && !string.IsNullOrEmpty(config.CLIENT_LUA))
                                {
                                    string luaBuffer = LuaHelper.Excel2Lua(table, ExportFlags.Client);
                                    if (!Directory.Exists(config.CLIENT_LUA))
                                    {
                                        Directory.CreateDirectory(config.CLIENT_LUA);
                                    }
                                    string path = string.Format("{0}/{1}.lua", config.CLIENT_LUA, table.name);
                                    File.WriteAllText(path, luaBuffer);
                                    Console.WriteLine(string.Format("client : {0}打包完成, 保存位置:{1}", table.name, path));
                                }
                                if(table.server == "lua" && !string.IsNullOrEmpty(config.SERVER_LUA))
                                {
                                    string luaBuffer = LuaHelper.Excel2Lua(table, ExportFlags.Server);
                                    if (!Directory.Exists(config.SERVER_LUA))
                                    {
                                        Directory.CreateDirectory(config.SERVER_LUA);
                                    }
                                    string path = string.Format("{0}/{1}.lua", config.SERVER_LUA, table.name);
                                    File.WriteAllText(path, luaBuffer);
                                    Console.WriteLine(string.Format("client : {0}打包完成, 保存位置:{1}", table.name, path));
                                }
                            }
                        }
                    };
                    mcs.WaitForExit();
                }
            }
        }

        /// <summary>
        /// 删除目录下所有文件，不包括子目录
        /// </summary>
        /// <param name="path"></param>
        static void DeleteAllFiles(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                if (!dir.Exists) return;
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)            //判断是否文件夹
                    {
                        continue;
                    }
                    else
                    {
                        File.Delete(i.FullName);      //删除指定文件
                    }
                } 
            }
            catch(Exception e)
            {
                Console.WriteLine(string.Format("删除目录{0}的文件失败,原因:{1}", path, e.ToString()));
            }
        }
    }

    internal class TableInfo
    {
        public List<Worksheet> sheets = new List<Worksheet>();
        public string name;
        public string nameSpace;
        public HashSet<string> client;
        public string server;
        public List<ColumnInfo> columns;
    }

    internal class ColumnInfo
    {
        public int index;
        public string name;
        public string type;
        public string defValue;
        public string note;
        public ExportFlags flags = ExportFlags.All;
    }
}
