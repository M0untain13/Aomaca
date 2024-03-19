import sys
import numpy as np
from keras.models import load_model
from PIL import Image


def prepare_image(path):
    file = Image.open(path).convert("RGB")
    image_size = (128, 128)
    return np.array(file.resize(image_size)).flatten() / 255.0


def predict_result(path):
    model = load_model("trained_model.h5")  # load the trained model
    class_names = ["Forged", "Authentic"]  # classification outputs
    test_image = prepare_image(path)
    test_image = test_image.reshape(-1, 128, 128, 3)

    y_pred = model.predict(test_image)
    y_pred_class = round(y_pred[0][0])

    prediction = class_names[y_pred_class]
    if y_pred <= 0.5:
        confidence = f"{(1-(y_pred[0][0])) * 100:0.2f}"
    else:
        confidence = f"{(y_pred[0][0]) * 100:0.2f}"
    return prediction, confidence


def PrintResult(path):
    (prediction, confidence) = predict_result(path)
    print(f'{confidence}')


if __name__ == '__main__':
    file_path = sys.argv[1]
    PrintResult(file_path)
