using Azure;
using Azure.AI.OpenAI;

namespace LiL.AI.OpenAI;

public class ChatCompletion
{

    string _aiKey = "";
    string _aiModelDeploymentName = "";
    string _aiEndpoint = ""; 

    bool _successfulProcessing = false;

    OpenAIClient _openAIClient;

    public ChatCompletion(string aiKey, string aiEndpoint, string modelDeploymentName)
    {
        _aiKey = aiKey;
        _aiEndpoint = aiEndpoint;
        _aiModelDeploymentName = modelDeploymentName;

        AzureKeyCredential azureKeyCredential = new AzureKeyCredential(_aiKey);
        _openAIClient = new OpenAIClient(new Uri(aiEndpoint), azureKeyCredential);
    }

    public async Task<(bool, string)> GetCompletion(string systemMessage, string userMessage, bool jsonResponse = false)
    {
        try {
            ChatCompletionsOptions chatCompletionsOptions = new ChatCompletionsOptions()
            {
                ResponseFormat = jsonResponse ? ChatCompletionsResponseFormat.JsonObject : ChatCompletionsResponseFormat.Text,
                DeploymentName = _aiModelDeploymentName,
                Messages =
                {
                    new ChatRequestSystemMessage(systemMessage),
                    new ChatRequestUserMessage(userMessage),
                },
                MaxTokens = 100, 
                NucleusSamplingFactor = 0.1f,
                Temperature = 0.1f
            };
            Response<ChatCompletions> response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions);
            return (true, response.Value.Choices[0].Message.Content);
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            _successfulProcessing = false;
            return (false, "");
        }
        
    }

    public async Task<(bool,string)> GetCompletion(string systemMessage, string userMessage, string imageUri, bool jsonResponse = false)
    {
        try {
            ChatCompletionsOptions chatCompletionsOptions = new ChatCompletionsOptions()
            {

                ResponseFormat = jsonResponse ? ChatCompletionsResponseFormat.JsonObject : ChatCompletionsResponseFormat.Text,
                DeploymentName = _aiModelDeploymentName,
                Messages =
                {
                    new ChatRequestSystemMessage(systemMessage),
                    new ChatRequestUserMessage(userMessage),
                    new ChatRequestUserMessage(
                        new ChatMessageTextContentItem(userMessage),
                        new ChatMessageImageContentItem(new Uri(imageUri))
                    )
                },
                MaxTokens = 100, 
                NucleusSamplingFactor = 0.1f,
                Temperature = 0.1f,
            };
            Response<ChatCompletions> response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions);
            return (true, response.Value.Choices[0].Message.Content);
        } catch {
            _successfulProcessing = false;
            return (false, "");
        }
    }
}
