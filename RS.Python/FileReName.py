# -*- coding: gbk -*-
import os

def rename_images(folder_path):
    # 获取文件夹中的所有文件
    files = os.listdir(folder_path)
    # 过滤出图片文件（这里假设所有文件都是图片，如果需要可以添加文件类型过滤）
    
    # 对文件名进行排序，确保命名顺序
    files.sort()
    
    # 重命名计数器
    counter = 12
    
    for file in files:
        # 获取原文件的完整路径
        old_path = os.path.join(folder_path, file)
        
        # 如果是文件（不是文件夹）
        if os.path.isfile(old_path):
            # 获取文件扩展名
            file_ext = os.path.splitext(file)[1]
            
            # 创建新文件名（7位数字，前面补0）
            new_name = f"Img{str(counter).zfill(7)}{file_ext}"
            
            # 构建新的完整路径
            new_path = os.path.join(folder_path, new_name)
            
            # 重命名文件
            os.rename(old_path, new_path)
            print(f"已重命名: {file} -> {new_name}")
            
            counter += 1

# 指定文件夹路径
folder_path = r"D:\Users\Administrator\Desktop\新建文件夹 (2)"

try:
    rename_images(folder_path)
    print("重命名完成！")
except Exception as e:
    print(f"发生错误: {str(e)}")