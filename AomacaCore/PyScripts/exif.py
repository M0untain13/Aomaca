import sys
from PIL import Image
from PIL.ExifTags import TAGS


def CreateMetadataFile(path):
    texts = {'SoftDetected': 'Обнаружена программа для редактирования фото.',
             'SoftNotDetected': 'Название устройства или ПО отсутствует.',
             'DateTimeNotDetected': 'отсутствует.',
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
                program = program.lower()
                if program in result['Software'].lower():
                    result['SoftDetected'] = texts['SoftDetected']
                    break
        else:
            result['Software'] = texts['SoftNotDetected']

        if 'DateTimeOriginal' in metaData:  # Анализируем дату создания и дату изменения
            result['DateTimeOriginal'] = metaData['DateTimeOriginal']
        else:
            result['DateTimeOriginal'] = texts['DateTimeNotDetected']

        if 'DateTime' in metaData:
            result['DateTime'] = metaData['DateTime']
        else:
            result['DateTime'] = texts['DateTimeNotDetected']

        if result['DateTimeOriginal'] != texts['DateTimeNotDetected'] \
                and result['DateTime'] != texts['DateTimeNotDetected'] \
                and result['DateTimeOriginal'] != result['DateTime']:
            result['DateDiffDetected'] = texts['DateDiffDetected']
    else:
        result['Error'] = texts['Error']

    string = ''

    if 'Software' in result.keys():
        result['Software'] = 'ПО: ' + result['Software']
    if 'DateTimeOriginal' in result.keys():
        result['DateTimeOriginal'] = 'Дата создания: ' + result['DateTimeOriginal']
    if 'DateTime' in result.keys():
        result['DateTime'] = 'Дата изменения: ' + result['DateTime']

    for key in result:
        string += f'{key}||{result[key]}\n'

    print(string)


if __name__ == "__main__":
    file_path = sys.argv[1]
    CreateMetadataFile(file_path)
