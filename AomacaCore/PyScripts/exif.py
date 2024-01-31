import os, sys
from PIL import Image
from PIL.ExifTags import TAGS


def CreateMetadataFile(path):
    texts = {'SoftDetected': 'Обнаружена программа для редактирования фото.',
             'SoftNotDetected': 'Название устройства или ПО отсутствует.',
             'DTOrigNotDetected': 'Дата создания отсутствует.',
             'DTNotDetected': 'Дата изменения отсутствует.',
             'DateDiffDetected': 'Дата создания и дата изменения не совпадают.',
             'Error': 'Метаданные не обнаружены.'
             }
    programs = [
        'Photoshop', 'GIMP', 'PhotoScape', 'Photoscape', 'PixBuilder', 'openCanvas',
        'Artweaver', 'Editor', 'Krita', 'Picasa', 'Fotor', 'ФотоМАСТЕР', 'Фотомастер',
        'Paint.NET', 'Pixlr', 'ACDSee', 'CorelDRAW', 'Фотостудия', 'Paint', 'Polarr'
    ]
    metaData = {}
    result = {}
    image_file = Image.open(path)
    info = image_file._getexif()  # Достаём метаданные из фото
    if info:
        for (tag, value) in info.items():
            tagname = TAGS.get(tag, tag)
            metaData[tagname] = value

        if 'Software' in metaData:  # Анализируем программное обеспечение
            result['Software'] = metaData['Software']
            for program in programs:
                if program in result['Software']:
                    result['SoftDetected'] = texts['SoftDetected']
                    break
        else:
            result['Software'] = texts['SoftNotDetected']

        if 'DateTimeOriginal' in metaData:  # Анализируем дату создания и дату изменения
            result['DateTimeOriginal'] = metaData['DateTimeOriginal']
        else:
            result['DateTimeOriginal'] = texts['DTOrigNotDetected']

        if 'DateTime' in metaData:
            result['DateTime'] = metaData['DateTime']
        else:
            result['DateTime'] = texts['DTNotDetected']

        if result['DateTimeOriginal'] != texts['DTOrigNotDetected'] \
                and result['DateTime'] != texts['DTNotDetected'] \
                and result['DateTimeOriginal'] != result['DateTime']:
            result['DateDiffDetected'] = texts['DateDiffDetected']
    else:
        result['Error'] = texts['Error']

    string = ''
    for key in result:
        string += f'{key}||{result[key]}\n'

    if not os.path.isdir("Files"):
        os.mkdir("Files")
    if os.path.exists("Files\\exif_result.txt"):
        os.remove("Files\\exif_result.txt")

    file = open("Files\\exif_result.txt", "w", encoding='utf-8')
    file.write(string)
    file.close()


if __name__ == "__main__":
    file_path = sys.argv[1]
    CreateMetadataFile(file_path)
