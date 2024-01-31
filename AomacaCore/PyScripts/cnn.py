import os, sys


# TODO: заменить заглушку на нормальную реализацию
def CreateFileWithAnswer(path):
    if not os.path.isdir("Files"):
        os.mkdir("Files")
    if os.path.exists("Files\\cnn_result.txt"):
        os.remove("Files\\cnn_result.txt")
    file = open("Files\\cnn_result.txt", "w", encoding='utf-8')
    file.write('94.34')
    file.close()


if __name__ == '__main__':
    file_path = sys.argv[1]
    CreateFileWithAnswer(file_path)
