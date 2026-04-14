using ImageBatchProcessor.Models;

using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageBatchProcessor.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        public async Task<ProcessingResult> BatchConvert(
            TransformType transformType,
            List<ImageFile> images,
            IProgress<int>? progress = null)
        {
            if (images.Count == 0)
                return new ProcessingResult(transformType);

            // Determine output folder name based on transform type
            string dirLabel = transformType switch
            {
                TransformType.Grayscale => "Grayscale",
                TransformType.Brighten => "Brightened",
                TransformType.RotateClockwise => "RotatedCW",
                TransformType.RotateCounterClockwise => "RotatedCCW",
                _ => "Converted"
            };

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string? sourceDirectory = Path.GetDirectoryName(images[0].FilePath);
            string outputDirectory = Path.Combine(sourceDirectory!, $"{dirLabel}_{timestamp}");
            Directory.CreateDirectory(outputDirectory);

            int converted = 0;

            await Task.Run(() =>
            {
                foreach (ImageFile image in images)
                {
                    // Load the source image
                    BitmapImage source = new BitmapImage(new Uri(image.FilePath));
                    source.Freeze();

                    // Apply the selected transform
                    BitmapSource transformed = transformType switch
                    {
                        TransformType.Grayscale => ConvertGrayscale(source),
                        TransformType.Brighten => ConvertBrighten(source),
                        TransformType.RotateClockwise => ConvertRotate(source, 90),
                        TransformType.RotateCounterClockwise => ConvertRotate(source, 270),
                        _ => source
                    };

                    // Encode and save
                    BitmapEncoder encoder = image.Extension switch
                    {
                        ".jpg" or ".jpeg" => new JpegBitmapEncoder(),
                        ".png" => new PngBitmapEncoder(),
                        ".bmp" => new BmpBitmapEncoder(),
                        ".gif" => new GifBitmapEncoder(),
                        ".tiff" => new TiffBitmapEncoder(),
                        _ => new PngBitmapEncoder()
                    };

                    encoder.Frames.Add(BitmapFrame.Create(transformed));

                    string outputPath = Path.Combine(outputDirectory, image.FileName);
                    using FileStream stream = File.OpenWrite(outputPath);
                    encoder.Save(stream);

                    converted++;
                    progress?.Report((int)(converted * 100.0 / images.Count));
                }
            });

            return new ProcessingResult(transformType)
            {
                numSuccessfulConversions = converted,
                outputDirectory = outputDirectory
            };
        }

        private BitmapSource ConvertGrayscale(BitmapImage source)
        {
            var gray = new FormatConvertedBitmap(source, PixelFormats.Gray8, null, 0);
            gray.Freeze();
            return gray;
        }

        private BitmapSource ConvertBrighten(BitmapImage source, byte brightenAmount = 50)
        {
            var visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawImage(source, new Rect(0, 0, source.PixelWidth, source.PixelHeight));
                dc.DrawRectangle(
                    new SolidColorBrush(Color.FromArgb(brightenAmount, 255, 255, 255)),
                    null,
                    new Rect(0, 0, source.PixelWidth, source.PixelHeight));
            }

            var rtb = new RenderTargetBitmap(
                source.PixelWidth, source.PixelHeight,
                source.DpiX, source.DpiY,
                PixelFormats.Pbgra32);
            rtb.Render(visual);
            rtb.Freeze();
            return rtb;
        }

        private BitmapSource ConvertRotate(BitmapImage source, double degrees)
        {
            var rotated = new TransformedBitmap(source, new RotateTransform(degrees));
            rotated.Freeze();
            return rotated;
        }
    }
}