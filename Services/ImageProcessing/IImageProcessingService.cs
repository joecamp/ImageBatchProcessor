using ImageBatchProcessor.Models;

namespace ImageBatchProcessor.Services
{
    interface IImageProcessingService
    {
        Task<ProcessingResult> BatchConvert(
            TransformType transformType,
            List<ImageFile> images,
            IProgress<int>? progress = null);
    }
}