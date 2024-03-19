import sys
from PIL import Image, ImageChops, ImageEnhance


def CreateElaImage(path: str, quality: int) -> None:
    baseDir = "Files"
    resavedFilePath = f"{baseDir}\\resaved_image.jpg"
    elaFilePath = f"{baseDir}\\ela_image.png"

    originalImage = Image.open(path).convert("RGB")
    originalImage.save(resavedFilePath, "JPEG", quality=quality)
    resavedImage = Image.open(resavedFilePath)

    elaImage = ImageChops.difference(originalImage, resavedImage)

    extrema = elaImage.getextrema()  # Коэффициенты масштабирования рассчитываются по экстремумам пикселей
    maxDifference = max([pix[1] for pix in extrema])
    if maxDifference == 0:
        maxDifference = 1
    scale = 350.0 / maxDifference
    elaImage = ImageEnhance.Brightness(elaImage).enhance(scale)  # Выравниваем яркость изображения

    elaImage.save(elaFilePath)


def Main(args: dir) -> None:
    if len(args) < 2:
        print('Необходим параметр: путь к изображению.')
    else:
        filePath = args[1]
        if len(args) > 2:
            quality = int(args[2])
        else:
            quality = 100
        CreateElaImage(filePath, quality)
        print('Изображение успешно создано.')


if __name__ == "__main__":
    Main(sys.argv)
