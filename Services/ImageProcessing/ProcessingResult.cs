using ImageBatchProcessor.Models;

namespace ImageBatchProcessor.Services
{
    public class ProcessingResult
    {
        public TransformType transformType;
        public string outputDirectory;
        public int numSuccessfulConversions;

        public ProcessingResult(TransformType type)
        {
            transformType = type;
            outputDirectory = string.Empty;
            numSuccessfulConversions = 0;
        }
    }
}