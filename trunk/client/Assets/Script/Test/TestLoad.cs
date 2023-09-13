using tab;
using UnityEngine;

namespace YX
{
    /// <summary>
    /// 测试加载配置表
    /// hannibal
    /// </summary>
    public class TestLoad : MonoBehaviour 
	{
        private void Start()
        {
            //这一句需要在加载配置表前执行
            JsonTables.RegisterType(typeof(TableLib));
            //加载
            JsonTables.LoadTable<int, TestTableInfo>();
            //使用
            var arr = TableLib.TestTable.GetArray();
            Debug.Log("数量:" + arr.Length);
            var info = TableLib.TestTable.GetRow(2);
            Debug.Log($"id:{info.id}");
        }
    }
}
