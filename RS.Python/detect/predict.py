#--coding:GBK --
from ultralytics import YOLO

# Load a model

model = YOLO("./runs/detect/test-detect/weights/best.pt")  # pretrained YOLO11n model

# Run batched inference on a list of images
results = model(r"D:\Users\Administrator\Downloads\def3fc2986e2c041e0f34bff978ba07e.jpg")  # return a list of Results objects

# Process results list
for result in results:
    boxes = result.boxes  # Boxes object for bounding box outputs
    masks = result.masks  # Masks object for segmentation masks outputs
    keypoints = result.keypoints  # Keypoints object for pose outputs
    probs = result.probs  # Probs object for classification outputs
    obb = result.obb  # Oriented boxes object for OBB outputs
    result.show()  # display to screen
    result.save(filename="result.jpg")  # save to disk