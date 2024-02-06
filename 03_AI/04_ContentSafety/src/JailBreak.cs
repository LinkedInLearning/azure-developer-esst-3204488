using System.Text;
using System.Text.Json;

namespace LiL.AI.ContentSafety;


public class JailBreak
{
    string _aiKey = "";
    string _aiEndpoint = ""; 
    bool _successfulProcessing = false;

    public JailBreak(string aiKey, string aiEndpoint)
    {
        _aiKey = aiKey;
        _aiEndpoint = aiEndpoint;
        
    }

    public async Task<(bool, bool)> Analyze(string prompt)
    {

        bool jailBreakDetected = false;

        string endpoint = _aiEndpoint + "contentsafety/text:detectJailbreak?api-version=2023-10-15-preview";
        
        string payload = JsonSerializer.Serialize(
            new { text = prompt }
        );

        try {
            using HttpClient httpClient = new HttpClient();


            httpClient.BaseAddress = new Uri(endpoint); 
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _aiKey);
            
            StringContent stringContent = new StringContent(payload, Encoding.UTF8, "application/json");
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(endpoint, stringContent);
            
            var response = JsonSerializer.Deserialize<dynamic>(await httpResponseMessage.Content.ReadAsStringAsync());
            jailBreakDetected = response.GetProperty("jailbreakAnalysis")
                                        .GetProperty("detected")
                                        .GetBoolean(); 
            
            _successfulProcessing = true;

        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            _successfulProcessing = false;
        }

        return (_successfulProcessing, jailBreakDetected);
    }
}
