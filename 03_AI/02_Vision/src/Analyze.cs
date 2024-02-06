using System.Text.Json;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.AI.Vision.Common;
using Azure.AI.Vision.ImageAnalysis; 

namespace LiL.AI.Vision;

public class Analyze
{
    string _aiKey = "";
    string _visionEndpoint = "";
    string _outputFolder;

    bool _successfulProcessing = false;

    public Analyze(string aiKey, string visionEndpoint, string outputFolder)
    {
        _aiKey = aiKey;
        _visionEndpoint = visionEndpoint;
        _outputFolder = outputFolder;
    }

    public async Task<bool> DenseCaptioning(string filePath)
    {
        try {
            using VisionSource visionSource = VisionSource.FromFile(filePath);
            ImageAnalysisOptions imageAnalysisOptions = new ImageAnalysisOptions()
            {
                Features = ImageAnalysisFeature.Caption 
                           | ImageAnalysisFeature.DenseCaptions,
            };

            VisionServiceOptions visionServiceOptions = new VisionServiceOptions(
                _visionEndpoint, 
                new AzureKeyCredential(_aiKey)
            );

            using ImageAnalyzer imageAnalyzer = new ImageAnalyzer(
                visionServiceOptions, 
                visionSource, 
                imageAnalysisOptions
            );

            ImageAnalysisResult imageAnalysisResult = await imageAnalyzer.AnalyzeAsync();


            string outputFile = Path.Combine(_outputFolder, "ImageDenseCaptioning.txt");
            if (File.Exists(outputFile)) File.Delete(outputFile);
            
            foreach(ContentCaption contentCaption in imageAnalysisResult.DenseCaptions) {
                string output = $"Content: {contentCaption.Content} \n";
                await File.AppendAllTextAsync(outputFile, output);

                output = $"Confidence: {contentCaption.Confidence:0.0000} \n";
                await File.AppendAllTextAsync(outputFile, output);

                output = $"Bounding box: {contentCaption.BoundingBox} \n\n";
                await File.AppendAllTextAsync(outputFile, output);
            }
            _successfulProcessing = true;
            
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            _successfulProcessing = false;
        }

        return _successfulProcessing;
    }
}
