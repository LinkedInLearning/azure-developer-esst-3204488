using System.Text.Json;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace LiL.AI.Vision;

public class Extract
{
    string _visionDIKey = "";
    string _visionDIEndpoint = "";
    string _outputFolder;

    bool _successfulProcessing = false;

    public Extract(string visionDIKey, string visionDIEndpoint, string outputFolder)
    {
        _visionDIKey = visionDIKey;
        _visionDIEndpoint = visionDIEndpoint;
        _outputFolder = outputFolder;
    }


    public async Task<bool> ExtractTextDocumentIntelligence(string filePath)
    {
        try {
            AzureKeyCredential azureKeyCredential = new AzureKeyCredential(_visionDIKey);
            DocumentAnalysisClient documentAnalysisClient = new DocumentAnalysisClient(
                new Uri(_visionDIEndpoint), 
                azureKeyCredential
            );

            using FileStream fileStream = new FileStream(filePath, FileMode.Open);

            AnalyzeDocumentOperation analyzeDocumentOperation = 
                await documentAnalysisClient.AnalyzeDocumentAsync(
                    WaitUntil.Completed, 
                    "prebuilt-invoice", 
                    fileStream
                );
            
            AnalyzeResult analyzeResult = analyzeDocumentOperation.Value;

            // Extracted Text
            string outputFile = Path.Combine(_outputFolder, "DocumentText.txt");
            if (File.Exists(outputFile)) File.Delete(outputFile);
            await File.WriteAllTextAsync(outputFile, analyzeResult.Content);

            // Raw Response
            Response rawResponse = analyzeDocumentOperation.GetRawResponse();
            outputFile = Path.Combine(_outputFolder, "DocumentRawResponse.json");
            if (File.Exists(outputFile)) File.Delete(outputFile);
            await File.WriteAllTextAsync(outputFile, rawResponse.Content.ToString());

            // Sample fields
            for (int documentCount = 0; documentCount < analyzeResult.Documents.Count; documentCount++)
            {
                AnalyzedDocument analyzedDocument = analyzeResult.Documents[documentCount];
                outputFile = Path.Combine(_outputFolder, $"Document_{documentCount}_SampleFields.txt");
                if (File.Exists(outputFile)) File.Delete(outputFile);
                var recognizedObjects = analyzedDocument.Fields.GroupBy(item => item.Value.FieldType);
                foreach (var recognizedObject in recognizedObjects) {
                    string output = $"Object-Type: {recognizedObject.Key.ToString()} - Count: {recognizedObject.Count()} \n";
                    await File.AppendAllTextAsync(outputFile, output);
                    foreach (var item in recognizedObject) {
                        string fieldValue = "";
                        if (item.Value.Content != null) {
                            fieldValue = item.Value.Content.Replace("\n", " ");
                        } else {
                            fieldValue = $"JSON: {JsonSerializer.Serialize(item.Value.Value.AsList()).Substring(0,50)} ...";
                        }
                        output = $"  Field: {item.Key} - Confidence: {item.Value.Confidence} - Value: {fieldValue} \n";
                        await File.AppendAllTextAsync(outputFile, output);
                    }
                    await File.AppendAllTextAsync(outputFile, "\n");
                }
            }
            _successfulProcessing = true;
        } catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
            _successfulProcessing = false;
        }
        return _successfulProcessing; 
    }
}
