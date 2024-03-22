import h5py
import onnx
import sys


# TODO: этот метод я пока не тестировал
def ConvertHDF5toONNX(path: str) -> None:
    # Load the HDF5 file
    hdf5_file = h5py.File('model.h5', 'r')

    # Extract the weights and biases from the HDF5 file
    weights = hdf5_file['weights'][:]
    biases = hdf5_file['biases'][:]

    # Create an ONNX model
    onnx_model = onnx.ModelProto()

    # Create ONNX tensors for weights and biases
    weight_tensor = onnx.numpy_helper.from_array(weights)
    bias_tensor = onnx.numpy_helper.from_array(biases)

    # Add the tensors to the ONNX model as initializers
    onnx_model.graph.initializer.extend([weight_tensor, bias_tensor])

    # Save the ONNX model to a file
    onnx.save(onnx_model, 'model.onnx')


def Main(args: list) -> None:
    if len(args) < 2:
        print('Необходим параметр: путь к файлу.')
    else:
        ConvertHDF5toONNX(args[1])


if __name__ == "__main__":
    Main(sys.argv)
