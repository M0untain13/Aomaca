import sys, exif, ela, cnn


def Main():
    print('Obsolete script!')
    return
    scriptName = sys.argv[1]
    if scriptName == 'ela':
        ela.CreateElaImage(sys.argv[2], int(sys.argv[3]))
    elif scriptName == 'exif':
        exif.CreateMetadataFile(sys.argv[2])
    elif scriptName == 'cnn':
        cnn.PrintResult(sys.argv[2])
    

if __name__ == "__main__":
    Main()
