using System.Collections.Generic;

namespace ExcelTool
{
    public class Config
    {
        /// <summary>
        /// 表格地址
        /// </summary>
        public string EXCEL;
        /// <summary>
        /// 分割粒度：只有大于0才会分割
        /// </summary>
        public int SPLIT;
        /// <summary>
        /// 生成pb代码地址
        /// </summary>
        public string CLIENT_CODE_PB;
        /// <summary>
        /// 生成客户端json代码地址
        /// </summary>
        public string CLIENT_CODE_JSON;
        /// <summary>
        /// 生成服务器json代码地址
        /// </summary>
        public string SERVER_CODE_JSON;
        /// <summary>
        /// 客户端pb文件地址
        /// </summary>
        public string CLIENT_PB;
        /// <summary>
        /// 客户端lua文件地址
        /// </summary>
        public string CLIENT_LUA;
        /// <summary>
        /// 客户端json文件地址
        /// </summary>
        public string CLIENT_JSON;
        /// <summary>
        /// 服务器json地址
        /// </summary>
        public string SERVER_JSON;
        /// <summary>
        /// 服务器lua地址
        /// </summary>
        public string SERVER_LUA;
        /// <summary>
        /// 命名空间
        /// </summary>
        public string NAMESPACE;
        /// <summary>
        /// 后缀
        /// </summary>
        public List<string> EXTENSION;
    }
}
