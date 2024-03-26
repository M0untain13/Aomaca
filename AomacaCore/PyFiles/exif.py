import sys
from PIL import Image
from PIL.ExifTags import TAGS
from datetime import datetime

def GetMetadata(path: str) -> dict:
    imageFile = Image.open(path)
    info = imageFile._getexif()
    necessaryTags = ['Software', 'DateTimeOriginal', 'DateTime']
    metadata = {}
    if info:
        for (tag, value) in info.items():
            tagName = TAGS.get(tag, tag)
            if tagName in necessaryTags:
                metadata[tagName] = value

    def Format(date: str) -> str:
        d = datetime.strptime(date, "%Y:%m:%d %H:%M:%S")
        return d.strftime("%H:%M:%S %d.%m.%Y")

    # Следующие строчки нужны лишь для порядка, т.к. внутри файла метаданные расположены в другом порядке
    result = {}
    for tag in necessaryTags:
        if tag in metadata:
            result[tag] = metadata[tag]
            
    return result


def AnalyzeImage(path: str) -> str:
    programs = [
        'Photoshop', 'GIMP', 'PhotoScape', 'Photoscape', 'PixBuilder', 'openCanvas',
        'Artweaver', 'Editor', 'Krita', 'Picasa', 'Fotor', 'ФотоМАСТЕР', 'Фотомастер',
        'Paint.NET', 'Pixlr', 'ACDSee', 'CorelDRAW', 'Фотостудия', 'Paint', 'Polarr'
    ]
    metadata = GetMetadata(path)

    result = ''
    if len(metadata) == 0:
        result += 'Error||Метаданные не обнаружены.'
    else:
        if 'Software' in metadata:
            for program in programs:
                if program.lower() in metadata['Software'].lower():
                    metadata['SoftwareAnalysis'] = 'Обнаружена программа для редактирования фото.||1'
                    break
            if 'SoftwareAnalysis' not in metadata:
                metadata['SoftwareAnalysis'] = 'Программа для редактирования фото не обнаружена.||0'
        else:
            metadata['SoftwareAnalysis'] = 'Данные о программе отсутствуют.||0'

        if 'DateTimeOriginal' in metadata:
            if 'DateTime' in metadata:
                if metadata['DateTimeOriginal'] != metadata['DateTime']:
                    metadata['DateTimeAnalysis'] = 'Дата создания и дата изменения не совпадают.||1'
                else:
                    metadata['DateTimeAnalysis'] = 'Дата создания и дата изменения совпадают.||0'
            else:
                metadata['DateTimeAnalysis'] = 'Дата изменения не обнаружена.||0'
        else:
            metadata['DateTimeAnalysis'] = 'Дата создания не обнаружена.||0'
    
        for key in metadata:
            result += f'{key}||{metadata[key]}\n'
    return result


def Main(args: list) -> None:
    if len(args) < 2:
        print('Необходим параметр: путь к изображению.')
    else:
        filePath = args[1]
        result = AnalyzeImage(filePath)
        print(result)


if __name__ == "__main__":
    Main(sys.argv)
