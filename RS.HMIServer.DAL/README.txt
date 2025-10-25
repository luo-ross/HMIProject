使用方法：点击工具栏->Nuget包管理器->程序包管理控制台
默认项目->RS.HMIServer.DAL

//使用迁移命令
//Enables these commonly used commands:
//Add-Migration
//Bundle-Migration
//Drop-Database
//Get-DbContext
//Get-Migration
//Optimize-DbContext
//Remove-Migration
//Scaffold-DbContext
//Script-Migration
//Update-Database

//常用命令如
//Add-Migration  RSAppMigration0  创建
//Update-Database RSAppMigration0  更新
//Remove-Migration 撤销

//常见问题
问题1：
如果出现使用“1”个参数调用“.ctor”时发生异常:“参数“frameworkName”不能是空字符串。参数名: frameworkName”
解决方案：
请将RS.HMIServer 设为默认启动项目 右键设为启动项或者工具栏里选择RS.HMIServer