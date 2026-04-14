using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ImageBatchProcessor.Models;
using ImageBatchProcessor.Services;

using Microsoft.Win32;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace ImageBatchProcessor.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        // Services
        private readonly IImageScannerService _scannerService;
        private readonly IImageProcessingService _imageProcessingService;

        public TransformType[] AvailableTransforms { get; } = Enum.GetValues<TransformType>();

        // ObservableCollection notifies the View when items are added or removed.
        // A regular List<string> would NOT update the ListView automatically.
        public ObservableCollection<ImageFile> ImageFiles { get; } = [];

        [ObservableProperty]
        private string _selectedFolderPath = string.Empty;
        [ObservableProperty]
        private ImageFile? _selectedImage;
        [ObservableProperty]
        private BitmapImage? _previewImage;
        [ObservableProperty]
        private TransformType _selectedTransform = TransformType.Grayscale;
        [ObservableProperty]
        private string _statusMessage = "Select a folder to get started.";
        [ObservableProperty]
        private int _progressPercent;
        [ObservableProperty]
        private bool _isProcessing;

        public MainWindowViewModel()
        {
            // For now, we create the service directly.
            // In a larger app, you'd inject it through the constructor:
            //   public MainWindowViewModel(IImageScannerService scannerService)
            // and use a DI container to wire it up.
            _scannerService = new ImageScannerService();
            _imageProcessingService = new ImageProcessingService();
        }

        [RelayCommand]
        private void SelectFolder()
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            dialog.Title = "Select a folder";

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            SelectedFolderPath = dialog.FolderName;

            // Call the service — the ViewModel doesn't know or care
            // HOW the scanning works, just that it gets results back.
            List<ImageFile> images = _scannerService.ScanFolder(SelectedFolderPath);

            // Update the observable collection
            ImageFiles.Clear();
            foreach(ImageFile image in images)
            {
                ImageFiles.Add(image);
            }

            PreviewImage = null;

            StatusMessage = ImageFiles.Count > 0
                ? $"Found {ImageFiles.Count} image(s) in folder"
                : $"No images found in folder";

            ConvertCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanConvert))]
        private async Task Convert()
        {
            IsProcessing = true;
            ProgressPercent = 0;

            Progress<int> progress = new Progress<int>(percent =>
            {
                ProgressPercent = percent;
            });

            ProcessingResult result = await _imageProcessingService.BatchConvert(
                SelectedTransform,
                ImageFiles.ToList(),
                progress);

            IsProcessing = false;
            ProgressPercent = 100;
            StatusMessage = $"Batch {result.transformType} result: converted {result.numSuccessfulConversions} images.";

            // Open directory with converted images
            var process = new System.Diagnostics.ProcessStartInfo
            {
                FileName = result.outputDirectory,
                UseShellExecute = true
            };

            System.Diagnostics.Process.Start(process);
        }

        private bool CanConvert() => ImageFiles.Count > 0 && !IsProcessing;

        [RelayCommand]
        private void OpenImage()
        {
            if (SelectedImage == null)
                return;

            // This opens the image in the user's default image viewer
            // (Photos app, Paint, whatever they have set up)
            var process = new System.Diagnostics.ProcessStartInfo
            {
                FileName = SelectedImage.FilePath,
                UseShellExecute = true
            };

            System.Diagnostics.Process.Start(process);
        }

        partial void OnSelectedImageChanged(ImageFile? value)
        {
            if (value == null)
            {
                PreviewImage = null;
                return;
            }

            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(value.FilePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 300; // Don't load full resolution for preview
                bitmap.EndInit();
                bitmap.Freeze();
                PreviewImage = bitmap;
            }
            catch
            {
                PreviewImage = null;
                StatusMessage = $"Could not preview {value.FileName}";
            }
        }

        partial void OnIsProcessingChanged(bool oldValue, bool newValue)
        {
            ConvertCommand.NotifyCanExecuteChanged();
            SelectFolderCommand.NotifyCanExecuteChanged();
        }

        partial void OnProgressPercentChanged(int oldValue, int newValue)
        {
            Debug.WriteLine($"Progress : {newValue}");
        }
    }
}