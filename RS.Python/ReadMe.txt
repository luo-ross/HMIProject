配置永久镜像源
pip config set global.index-url https://mirror.baidu.com/pypi/simple

清华镜像：https://pypi.tuna.tsinghua.edu.cn/simple
科大镜像：https://pypi.mirrors.ustc.edu.cn/simple
豆瓣镜像：http://pypi.douban.com/simple/
阿里镜像：https://mirrors.aliyun.com/pypi/simple/
百度镜像：https://mirror.baidu.com/pypi/simple

Tensorflow配置

清除缓存
pip cache purge

创建虚拟环境
conda create -n tensorflow  python=3.9

激活虚拟环境 
conda activate tensorflow

安装Tensorflow 	
pip install tensorflow

测试是否安装成功
python -c "import tensorflow as tf;print(tf.reduce_sum(tf.random.normal([1000, 1000])))"

配置jupyter kernel
安装
pip install ipykernel
列出内核
jupyter kernelspec list
安装一个内核
python -m ipykernel install --user --name tensorflow
#删除一个内核
jupyter kernelspec remove tensorflow

#安装whl
pip install whl文件全路径

//安装环境方法
1、安装Conda
2、创建虚拟环境
3、在虚拟环境下安装Pytorch
4、在虚拟环境下安装Ultralytics 就是YoloV8
5、在VS2022里创建Python项目，如果没有Pyhton 请去Visual Studio Installer 里安装Python
6、在项目Python环境右键 ->添加环境->选择配置好的虚拟环境
最后预祝你能成功！~







