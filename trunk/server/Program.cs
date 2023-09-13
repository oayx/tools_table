using System;
using tab;
using YX;

namespace server
{
    class Program
    {
        static void Main(string[] args)
        {
            //这一句需要在加载配置表前执行
            JsonTables.RegisterType(typeof(TableLib));
            //加载
            JsonTables.LoadTable<int, TestTableInfo>();
            //使用
            var arr = TableLib.TestTable.GetArray();
            Console.WriteLine("数量:" + arr.Length);
            var info = TableLib.TestTable.GetRow(2);
            Console.WriteLine($"id:{info.id}");

            Console.ReadKey();
        }
    }
}