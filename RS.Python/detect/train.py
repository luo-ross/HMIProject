#--coding:GBK --
from os import name
import torch
from ultralytics import YOLO
from pathlib import Path
import cv2
from ultralytics.utils import ops
from ultralytics.utils import LOGGER, TQDM

if __name__ == '__main__':
    model =YOLO("yolov8n.pt")  # load a pretrained model (recommended for training)
    # Train the model
    results = model.train(data="./detect/detect.yaml",  epochs=100, imgsz=640,batch=2,lr0=0.01,lrf=0.01,patience=100,name="test-detect")