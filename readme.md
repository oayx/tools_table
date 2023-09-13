## 主要功能  
    实现了把excel导出成pb,json,lua等数据结构的功能  
## 使用方式
	第一步：编译工具  
	    打开tools_table.sln(vs 2017)，编译ExcelTool项目，把生成的ExcelTool.exe拷贝到table/Tool目录  
	第二步：配置生成路径  
		打开config.json，配置路径(路径是相对于上一步ExcelTool.exe所在目录)，以下是配置生成Json的方式  
		EXCEL：			excel文件所在目录  
		CLIENT_CODE_JSON：	客户端生成json代码目录  
		CLIENT_JSON：		客户端生成json资源目录  
		SERVER_CODE_JSON：	服务器生成json代码目录  
		SERVER_JSON：		服务器生成json资源目录  
	    	NAMESPACE：		生成的代码，默认添加的命名空间  
	    	EXTENSION：		工具对那些格式的excel文件有效  
	第三步：使用bat生成(bat文件在table目录)  
		打资源.bat：只打资源，如果只是修改数据，没有修改表结构，可以用这个  
		生成代码.bat：生成代码，如果只是修改表结构，没有修改数据，可以用  
		生成代码+打资源.bat：生成代码的同时也重新生成资源  
	附：客户端支持pb、json、lua三种格式；服务器支持json、lua两种格式  
	    a.	如果需要生成pb格式，客户端需要设置CLIENT_PB和CLIENT_CODE_PB  
	    b.	如果需要生成json格式，客户端需要设置CLIENT_JSON和CLIENT_CODE_JSON，服务器需要设置SERVER_JSON和SERVER_CODE_JSON  
	    c.	如果需要生成lua格式，客户端需要设置CLIENT_LUA，服务器需要设置SERVER_LUA  
## 支持的数据格式
	Json：C#基础数据类型，比如short、int、float、long、float等，一维数组，二维数组，自定义数据类型
 	PB：跟json相同，不过不支持二维数组
  	lua：跟json相同
 ## 自定义数据格式
