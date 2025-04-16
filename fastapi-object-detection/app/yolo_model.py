from ultralytics import YOLO
import numpy as np
from PIL import Image

# Load the YOLOv11 nano model; change to a different weight file (e.g., "yolo11s.pt") if desired.
model = YOLO("yolo11n.pt")  # Make sure this file is available or downloaded automatically

def detect_objects(image: Image.Image):
    # Convert PIL image to numpy array
    image_array = np.array(image)
    # Run inference; YOLO returns a list of results (one per image)
    results = model(image_array)
    detections = []
    # Loop through each result and extract boxes, confidences, and labels.
    for result in results:
        for box in result.boxes:
            cls_id = int(box.cls[0])
            confidence = float(box.conf[0])
            label = model.names[cls_id]
            # Get bounding box coordinates in [x1, y1, x2, y2] format
            bbox = box.xyxy[0].tolist()
            detections.append({
                "label": label,
                "confidence": round(confidence, 3),
                "bbox": bbox
            })

    print("YOLO model was executed. Detected:", [d["label"] for d in detections])  # <-- Debug output
    return detections
