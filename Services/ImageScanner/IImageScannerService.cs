using ImageBatchProcessor.Models;

namespace ImageBatchProcessor.Services
{
    /// <summary>
    /// Interface for the image scanning service.
    /// 
    /// Why an interface? Two reasons:
    /// 
    /// 1. The ViewModel depends on this interface, not the concrete class.
    ///    This means you could swap in a different implementation later
    ///    (e.g., one that scans a network drive or a cloud bucket) without
    ///    changing the ViewModel at all.
    /// 
    /// 2. For unit testing, you can create a fake implementation that
    ///    returns test data without touching the file system. This lets
    ///    you test your ViewModel logic in isolation.
    /// </summary>
    public interface IImageScannerService
    {
        List<ImageFile> ScanFolder(string folderPath);
    }
}