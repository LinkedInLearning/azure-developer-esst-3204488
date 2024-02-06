using DotNetEnv;
using static LiL.AI.ContentSafety.TextAnalyzer;

namespace LiL.AI.ContentSafety.Test;

public class AnalyzeTextTest
{

    string _aiKey = "";
    string _aiEndpoint = "";

    string _defaultText = "";
    
    TextAnalyzer _textAnalyzer; 
    
    public AnalyzeTextTest()
    {
        string configurationFile = "../../../../CreateEnv/Configuration.env";
        Env.Load(configurationFile);
    
        _aiKey = Environment.GetEnvironmentVariable("LI4DEVS.CONTENTSAFETY.KEY")??""; 
        _aiEndpoint = Environment.GetEnvironmentVariable("LI4DEVS.CONTENTSAFETY.ENDPOINT")??"";

        _defaultText = "Einer meiner Freunde hat sich mit mir über Selbstmord unterhalten!";

        _textAnalyzer = new TextAnalyzer(_aiKey, _aiEndpoint);
    }


    [Fact]
    public async Task Analyze_Dev()
    {
        (bool success, TextAnalyzeResult textAnalyzeResult) result = await _textAnalyzer.Analyze(_defaultText);
        
        Assert.True (
            result.success &&
            result.textAnalyzeResult.SelfHarm > 0
        ); 
    }
}