import os
import sys
import onnx




def ConvertHDF5toONNX(path: str) -> str:
    fileName, fileExt = os.path.splitext(path)
    if fileExt != '.h5':
        message = 'Формат файла должен быть .h5.'
    else:
        # Эти библиотеки импортирую только тут, т.к. они долго загружаются
        from tensorflow.python.keras.models import load_model
        import tf2onnx
        os.environ['TF_KERAS'] = '1'
        hdf5Model = load_model(path)
        onnxModel, _ = tf2onnx.convert.from_keras(hdf5Model)
        onnx.save_model(onnxModel, f'{fileName}.onnx')
        message = 'Файл успешно конвертирован.'
    return message


def Main(args: list) -> None:
    if len(args) < 2:
        print('Необходим параметр: путь к файлу.')
    else:
        print(ConvertHDF5toONNX(args[1]))

try:
    assert sys.version_info[0] == 3
    assert sys.version_info[1] == 10
    if __name__ == "__main__":
        Main(sys.argv)
except:
    print('Необходимо использовать python 3.10')
