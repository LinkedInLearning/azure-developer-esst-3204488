using DotNetEnv;
using LiL.AI.OpenAI;

namespace LiL.AI.Speech.Test;

public class OpenAITest
{
    string _aiKey = "";
    string _aiEndpoint = "";
    string _modelDeploymentName = "";
    string _imageUrl = "";

    ChatCompletion _chatCompletion;
    
    public OpenAITest()
    {
        string configurationFile = "../../../../CreateEnv/Configuration.env";
        Env.Load(configurationFile);
    
        _aiKey = Environment.GetEnvironmentVariable("LI4DEVS.OPENAI.AIKEY")??""; 
        _aiEndpoint = Environment.GetEnvironmentVariable("LI4DEVS.OPENAI.AIENDPOINT")??"";
        _modelDeploymentName = Environment.GetEnvironmentVariable("LI4DEVS.OPENAI.MODELDEPLOYMENTNAME")??"";
        _imageUrl = Environment.GetEnvironmentVariable("LI4DEVS.OPENAI.IMAGEURL")??"";

        _chatCompletion = new ChatCompletion(_aiKey, _aiEndpoint, _modelDeploymentName);

    }

    [Fact]
    public async Task GetCompletion_Text_Dev()
    {
        string systemMessage = @"
            Du bist ein Assistent, der Menschen hilft, interessante Sehenswürdigkeiten 
            auf der ganzen Welt zu finden. Du benennst die Sehenswürdigkeit, 
            die Stadt und das Land in dem die Sehenswürdigkeit liegt. 
            Du lieferst keine weiteren Informationen.
        "; 

        string userMessage = @"
            Eine Statue einer Meerjungfrau auf einem Felsen im Meer. 
            Um welche Sehenswürdigkeit handelt es sich hier?
        ";

        (bool success, string result) = await _chatCompletion.GetCompletion(systemMessage, userMessage);
        Assert.True(success && result == "Die Kleine Meerjungfrau, Kopenhagen, Dänemark.");
    }

    [Fact]
    public async Task GetCompletion_Text_JSON_Dev()
    {
        string systemMessage = @"
            Du bist ein Assistent, der Menschen hilft, interessante Sehenswürdigkeiten 
            auf der ganzen Welt zu finden. Du benennst die Sehenswürdigkeit, 
            die Stadt und das Land in dem die Sehenswürdigkeit liegt. 
            Du lieferst keine weiteren Informationen. Du lieferst die Ergebnisse als JSON.
        "; 

        string userMessage = @"
            Eine Statue einer Meerjungfrau auf einem Felsen im Meer. 
            Um welche Sehenswürdigkeit handelt es sich hier?
        ";

        (bool success, string result) = await _chatCompletion.GetCompletion(systemMessage, userMessage, true);
        Assert.True(success && result == "{\n    \"Sehenswürdigkeit\": \"Die Kleine Meerjungfrau\",\n    \"Stadt\": \"Kopenhagen\",\n    \"Land\": \"Dänemark\"\n}");
    }

    [Fact]
    public async Task GetCompletion_Image_Dev()
    {
        string systemMessage = @"
            Du bist ein Assistent, der Menschen hilft, interessante Sehenswürdigkeiten 
            auf der ganzen Welt zu finden. Du benennst die Sehenswürdigkeit, 
            die Stadt und das Land in dem die Sehenswürdigkeit liegt. 
            Du lieferst keine weiteren Informationen.
        "; 

        string userMessage = @"
            Um welche Sehenswürdigkeit handelt es sich hier?
        ";

        (bool success, string result) = await _chatCompletion.GetCompletion(systemMessage, userMessage, _imageUrl);
        Assert.True(success && result == "Die Kleine Meerjungfrau, Kopenhagen, Dänemark.");
    }
}