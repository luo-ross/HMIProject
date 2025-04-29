# -*- coding: gbk -*-
import os

def rename_images(folder_path):
    # ��ȡ�ļ����е������ļ�
    files = os.listdir(folder_path)
    # ���˳�ͼƬ�ļ���������������ļ�����ͼƬ�������Ҫ��������ļ����͹��ˣ�
    
    # ���ļ�����������ȷ������˳��
    files.sort()
    
    # ������������
    counter = 12
    
    for file in files:
        # ��ȡԭ�ļ�������·��
        old_path = os.path.join(folder_path, file)
        
        # ������ļ��������ļ��У�
        if os.path.isfile(old_path):
            # ��ȡ�ļ���չ��
            file_ext = os.path.splitext(file)[1]
            
            # �������ļ�����7λ���֣�ǰ�油0��
            new_name = f"Img{str(counter).zfill(7)}{file_ext}"
            
            # �����µ�����·��
            new_path = os.path.join(folder_path, new_name)
            
            # �������ļ�
            os.rename(old_path, new_path)
            print(f"��������: {file} -> {new_name}")
            
            counter += 1

# ָ���ļ���·��
folder_path = r"D:\Users\Administrator\Desktop\�½��ļ��� (2)"

try:
    rename_images(folder_path)
    print("��������ɣ�")
except Exception as e:
    print(f"��������: {str(e)}")