import sys
import numpy as np
from keras.models import load_model
from PIL import Image


def PrepareImage(path: str) -> np.ndarray:
    file = Image.open(path).convert("RGB")
    imageSize = (128, 128)
    return np.array(file.resize(imageSize)).flatten() / 255.0


def PredictResult(path: str) -> float:
    model = load_model("trained_model.h5")

    image = PrepareImage(path)
    image = image.reshape(-1, 128, 128, 3)

    y_pred = model.predict(image)

    return y_pred[0][0]


def Main(args: dir) -> None:
    if len(args) < 2:
        print('Необходим параметр: путь к изображению.')
    else:
        filePath = args[1]
        prediction, confidence = PredictResult(filePath)
        print(f'{prediction}')


if __name__ == '__main__':
    Main(sys.argv)
