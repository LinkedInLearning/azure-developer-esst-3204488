using Azure.AI.ContentSafety;
using Azure; 

namespace LiL.AI.ContentSafety;


public class TextAnalyzer
{
    string _aiKey = "";
    string _aiEndpoint = ""; 

    public TextAnalyzer(string aiKey, string aiEndpoint)
    {
        _aiKey = aiKey;
        _aiEndpoint = aiEndpoint;
        
    }

    public async Task<(bool, TextAnalyzeResult)> Analyze(string text)
    {

        try {
            AzureKeyCredential azureKeyCredential = new AzureKeyCredential(_aiKey);
            ContentSafetyClient contentSafetyClient = new ContentSafetyClient(
                new Uri(_aiEndpoint), 
                azureKeyCredential
            );

            AnalyzeTextOptions analyzeTextOptions = new AnalyzeTextOptions(text);

            Response<AnalyzeTextResult> response;
            
            response = contentSafetyClient.AnalyzeText(analyzeTextOptions);
            
            TextAnalyzeResult textAnalyzeResult = new TextAnalyzeResult(){
                Hate = response.Value.CategoriesAnalysis.FirstOrDefault(
                    item => item.Category == TextCategory.Hate)?
                    .Severity ?? 0,
                SelfHarm = response.Value.CategoriesAnalysis.FirstOrDefault(
                    item => item.Category == TextCategory.SelfHarm)?
                    .Severity ?? 0,
                Sexual = response.Value.CategoriesAnalysis.FirstOrDefault(
                    item => item.Category == TextCategory.Sexual)?
                    .Severity ?? 0,
                Violence = response.Value.CategoriesAnalysis.FirstOrDefault(
                    item => item.Category == TextCategory.Violence)?
                    .Severity ?? 0,
            };

            return (true, textAnalyzeResult);

        } catch (Exception ex) {

            Console.WriteLine(ex.Message); 
            return (false, new TextAnalyzeResult());

        }
    }

    public class TextAnalyzeResult {
        public int Hate {get; set;} = 0;
        public int SelfHarm {get; set;} = 0;
        public int Sexual {get; set;} = 0;
        public int Violence {get; set;} = 0;
    }
}
