from fastapi import FastAPI, File, UploadFile
from fastapi.responses import JSONResponse
from PIL import Image
import io

from .yolo_model import detect_objects

app = FastAPI()

@app.post("/detect/")
async def detect(file: UploadFile = File(...)):
    contents = await file.read()
    # Open the uploaded image and ensure it is in RGB mode
    image = Image.open(io.BytesIO(contents)).convert("RGB")
    # Run detection using our YOLOv11 integration
    detections = detect_objects(image)
    return JSONResponse(content={"detections": detections})
