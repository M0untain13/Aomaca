import sys
from PIL import Image, ImageChops, ImageEnhance

import numpy as np
from sklearn.decomposition import PCA


def CreateElaImage(path: str, quality: int) -> None:
    baseDir = "Files"
    resavedFilePath = f"{baseDir}\\resaved_image.jpg"
    elaFilePath = f"{baseDir}\\ela_image.png"

    # Resave image
    originalImage = Image.open(path).convert('L')
    originalImage.save(resavedFilePath, "JPEG", quality=quality)
    resavedImage = Image.open(resavedFilePath)

    # Noice
    elaImage = ImageChops.difference(originalImage, resavedImage)

    # Extract high frequency noise
    noise_flat = np.array(elaImage)
    pca = PCA(n_components=5)
    image_compressed = pca.fit_transform(noise_flat)

    image_decompressed = pca.inverse_transform(image_compressed)

    high_noise = Image.fromarray(image_decompressed).convert('L')

    extrema = high_noise.getextrema()  # Коэффициенты масштабирования рассчитываются по экстремумам пикселей
    maxDifference = max(extrema)
    if maxDifference == 0:
        maxDifference = 1
    scale = 350.0 / maxDifference
    high_noise = ImageEnhance.Brightness(high_noise).enhance(scale)  # Выравниваем яркость изображения

    high_noise.save(elaFilePath)


def Main(args: list) -> None:
    if len(args) < 2:
        print('Необходим параметр: путь к изображению.')
    else:
        filePath = args[1]
        if len(args) > 2:
            quality = int(args[2])
        else:
            quality = 80
        CreateElaImage(filePath, quality)
        print('Изображение успешно создано.')


if __name__ == "__main__":
    Main(sys.argv)
