using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows;
using System;

namespace AomacaApp.Converters;

/// <summary>
/// This converter facilitates a couple of requirements around images. Firstly, it automatically disposes of image streams as soon as images
/// are loaded, thus avoiding file access exceptions when attempting to delete images. Secondly, it allows images to be decoded to specific
/// widths and / or heights, thus allowing memory to be saved where images will be scaled down from their original size.
/// </summary>
public sealed class BitmapFrameConverter : IValueConverter
{
    //doubles purely to facilitate easy data binding
    public double DecodePixelWidth { get; set; }
    public double DecodePixelHeight { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        var path = value as string;

        if (string.IsNullOrEmpty(path)) 
            return DependencyProperty.UnsetValue;

        //create new stream and create bitmap frame
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = new FileStream(path, FileMode.Open, FileAccess.Read);
        bitmapImage.DecodePixelWidth = (int)DecodePixelWidth;
        bitmapImage.DecodePixelHeight = (int)DecodePixelHeight;
        //load the image now so we can immediately dispose of the stream
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();

        //clean up the stream to avoid file access exceptions when attempting to delete images
        bitmapImage.StreamSource.Dispose();

        return bitmapImage;

    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}