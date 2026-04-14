using ImageBatchProcessor.Models;

using System.IO;
using System.Windows.Media.Imaging;

namespace ImageBatchProcessor.Services
{
    /// <summary>
    /// Service: Contains the actual logic for scanning folders for images.
    /// 
    /// This class has ZERO knowledge of the UI. It doesn't know about
    /// ViewModels, bindings, XAML, or buttons. It just takes a folder path,
    /// scans it, and returns a list of ImageFile models.
    /// 
    /// The ViewModel calls this service. The service returns data.
    /// The ViewModel then exposes that data to the View. Clean separation.
    /// </summary>
    class ImageScannerService : IImageScannerService
    {
        private static readonly string[] ImageExtensions = 
            [".jpg", ".jpeg", ".png", ".bmp"];

        public List<ImageFile> ScanFolder(string folderPath)
        {
            List<ImageFile> images = new List<ImageFile>();

            if (!Directory.Exists(folderPath))
                return images;

            foreach (string filePath in Directory.GetFiles(folderPath))
            {
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                if (!ImageExtensions.Contains(extension))
                    continue;

                FileInfo fileInfo = new FileInfo(filePath);

                Uri uri = new Uri(filePath, UriKind.Absolute);
                BitmapDecoder decoder = BitmapDecoder.Create(uri, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                BitmapFrame frame = decoder.Frames[0];

                images.Add(new ImageFile
                {
                    FileName = fileInfo.Name,
                    FilePath = fileInfo.FullName,
                    FileSize = fileInfo.Length,
                    Extension = extension,
                    Width = frame.PixelWidth,
                    Height = frame.PixelHeight,
                    CreatedDate = fileInfo.CreationTime
                });
            }

            return images;
        }
    }
}