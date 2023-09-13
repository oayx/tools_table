

namespace tab
{
    [System.Serializable]
    public class TestTableInfo : JsonTableRow<int>
    {
        ///<summary> 
        ///注释 
        ///</summary>
        public int id { get; set; }

        ///<summary> 
        ///整数 
        ///</summary>
        public int TestInt { get; set; }

        ///<summary> 
        ///浮点数 
        ///</summary>
        public float TestFloat { get; set; }

        ///<summary> 
        ///bool 
        ///</summary>
        public bool TestBool { get; set; }

        ///<summary> 
        ///字符串 
        ///</summary>
        public string TestString { get; set; }

        ///<summary> 
        ///自定义 
        ///</summary>
        public vector3 TestVector3 { get; set; }

        ///<summary> 
        ///整形数组 
        ///</summary>
        public int[] IntArr { get; set; }

        ///<summary> 
        ///浮点数组 
        ///</summary>
        public float[] FloatArr { get; set; }

        ///<summary> 
        ///字符串数组 
        ///</summary>
        public string[] StringArr { get; set; }

        ///<summary> 
        ///二维数组 
        ///</summary>
        public int[][] IntArrArr { get; set; }

        ///<summary> 
        ///二维数组 
        ///</summary>
        public string[][] StringArrArr { get; set; }

        public override int ID{ get { return id; } }
    }

    public partial class TableLib
    {
        public static JsonTable<int, TestTableInfo> TestTable { get; private set; }
    }
}