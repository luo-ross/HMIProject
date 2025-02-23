1、首先了解WPF的着色器
2、下载安装Shazzam 使用HLSL编写着色器代码
3、Shazzam会自动生成C#和VB.net的着色代码 就是继承WPF ShaderEffect
4、点击Shazzam软件顶部Tools选择Explore Compile Share 导出.ps和.cs文件
5、把2个文件拷贝到项目 然后创建着色器Effect
6、在空间里添加Effect，引入着色器命名空间 使用着色器