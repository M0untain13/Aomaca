import sys
from PIL import Image, ImageChops, ImageEnhance, ImageFilter


def CreateElaImage(path: str, quality: int) -> None:
    baseDir = "Files"
    resavedFilePath = f"{baseDir}\\resaved_image.jpg"
    elaFilePath = f"{baseDir}\\ela_image.png"

    originalImage = Image.open(path).convert("L")
    originalImage.save(resavedFilePath, "JPEG", quality=quality)
    resavedImage = Image.open(resavedFilePath)

    elaImage = ImageChops.difference(originalImage, resavedImage)

    elaImage = elaImage.filter(ImageFilter.GaussianBlur(radius=4))

    extrema = elaImage.getextrema()  # Коэффициенты масштабирования рассчитываются по экстремумам пикселей
    maxDifference = max(extrema)
    if maxDifference == 0:
        maxDifference = 1
    scale = 350.0 / maxDifference
    print(scale)
    if scale > 25:
        scale = 25
    elaImage = ImageEnhance.Brightness(elaImage).enhance(scale)  # Выравниваем яркость изображения

    elaImage.save(elaFilePath)


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
