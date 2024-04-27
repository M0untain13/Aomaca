import sys
from PIL import Image, ImageChops, ImageEnhance, ImageFilter


def GetBaseDir() -> str:
    return 'Files'


def GetDif(path: str, resavedPath: str, quality: int, format: str) -> Image:
    originalImage = Image.open(path).convert(format)
    originalImage.save(resavedPath, "JPEG", quality=quality)
    resavedImage = Image.open(resavedPath)
    return ImageChops.difference(originalImage, resavedImage)


def GetEnhance(image: Image, format: str) -> Image:
    if format == 'L':
        extrema = image.getextrema()
    elif format == 'RGB':
        extrema = image.getextrema()[0]
    else:
        raise Exception(f'Invalid argument: {format}')
    maxDifference = max(extrema)
    if maxDifference == 0:
        maxDifference = 1
    scale = 350.0 / maxDifference
    return ImageEnhance.Brightness(image).enhance(scale)


def GetMask(path: str, quality: int) -> Image:
    resavedPath = f"{GetBaseDir()}\\resaved_for_mask.jpg"
    format = 'L'
    mask = GetDif(path, resavedPath, quality, format)
    mask = mask.filter(ImageFilter.GaussianBlur(radius=10))
    mask = GetEnhance(mask, format)
    return mask


def GetEla(path: str, quality: int) -> Image:
    resavedPath = f"{GetBaseDir()}\\resaved_image.jpg"
    format = 'RGB'
    image = GetDif(path, resavedPath, quality, format)
    image = GetEnhance(image, format)
    image.save(f'{GetBaseDir()}\\t1.png')
    mask = GetMask(path, quality)
    mask.save(f'{GetBaseDir()}\\t2.png')
    result = Image.composite(image, Image.new('RGB', image.size, 'black'), mask)
    return result


def Main(args: list) -> None:
    if len(args) < 2:
        print('Необходим параметр: путь к изображению.')
    else:
        filePath = args[1]
        if len(args) > 2:
            quality = int(args[2])
        else:
            quality = 80
        GetEla(filePath, quality).save(f'{GetBaseDir()}\\ela_image.png')
        print('Изображение успешно создано.')


if __name__ == "__main__":
    Main(sys.argv)
