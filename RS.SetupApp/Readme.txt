说明
这是自定义程序安装程序

我需要在安装程序里处理以下一个事情

第1：需要设置自定义文件格式rsdl打开方式为当前程序
手动方式：
1、使用Cmd命令输入regedit 进入注册表
2、在HKEY_CLASSES_ROOT节点下创建节点 .rsdl
3、在.rsdl节点下创建节点 OpenWithProgids
4、在节点OpenWithProgids里添加字符串值 RsdlFile
5、在节点HKEY_CLASSES_ROOT下再次创建节点 RsdlFile
6、在节点RsdlFile 下添加节点DefaultIcon
7、在节点RsdlFile 下添加节点shell
8、在节点shell 下添加节点open
9、在节点open 下添加节点command
10、在节点DefaultIcon 中找到（默认）双击打开设置值(就是Icon的路径) 比如D:\icon.ico 备注:完整路径
11、在节点open 中找到 (默认) 双击打开设置值为 RsdlFile
12、在节点open 中右键添加字符串名称 FriendlyAppName 设置值为RsdlFile
13、在节点command 中找到 (默认) 双击打开设置启动程序全路径 比如D:\RS.WPFApp.exe %1 
备注：
这里%1就是代表打开程序后会给程序里的main方法里的参数arg[] 传递参数。
这个参数值就是打开当前文件的全路径

程序设计就是操作注册表类来完成上面的手动操作动作

第2:为用户创建桌面快捷方式
添加Com引用Windows Script Host Object Model 

第3:为用户创建开始菜单
目录为：C:\ProgramData\Microsoft\Windows\Start Menu\程序快捷方式
C:\ProgramData\Microsoft\Windows\Start Menu\程序名称\程序快捷方（备注 这里的快捷方式地址不能父级一样）

第4:注册程序和功能在下面的节点添加注册
(这个进去直接参考其他的安装程序很好理解，节点繁多)
32位程序配置节点
计算机\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall
64位程序配置节点
HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\51d76d93-5c99-52d8-8eaf-e2973435dc9b

第5:注册表设置开机启动项


备注：如果提示找不到MyPublish,这个就是MyPublish.Zip文件
这个文件就是程序的压缩包，自己创建一个就可以了






