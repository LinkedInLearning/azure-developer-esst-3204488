using DotNetEnv;
using LiL.AI.Vision;

namespace LiL.AI.Speech.Test;

public class VisionTest
{
    string _dIKey = "";
    string _dIEndpoint = "";
    string _demoDocumentFile = "";
    string _demoOutputFolder = "";

    string _aiKey = "";
    string _visionEndpoint = ""; 
    string _demoImageFile = "";
    Extract _extract;
    Analyze _analyze;

    public VisionTest()
    {
        string configurationFile = "../../../../CreateEnv/Configuration.env";
        Env.Load(configurationFile);
    
        _dIKey = Environment.GetEnvironmentVariable("LI4DEVS.VISION.DIKEY")??""; 
        _dIEndpoint = Environment.GetEnvironmentVariable("LI4DEVS.VISION.DIENDPOINT")??"";

        _aiKey = Environment.GetEnvironmentVariable("LI4DEVS.VISION.AIKEY")??"";
        _visionEndpoint = Environment.GetEnvironmentVariable("LI4DEVS.VISION.VISIONENDPOINT")??"";

        _demoDocumentFile = "../../../../assets/Input/Invoice_03.jpg";
        _demoImageFile = "../../../../assets/Input/LittleMermaid.jpg";
        _demoOutputFolder = "../../../../assets/Output";

        _extract = new Extract(_dIKey, _dIEndpoint, _demoOutputFolder);   
        _analyze = new Analyze(_aiKey, _visionEndpoint, _demoOutputFolder);
    }

    [Fact]
    public async Task ExtractTextDocumentIntelligence_Dev()
    {
        bool success = await _extract.ExtractTextDocumentIntelligence(_demoDocumentFile);
        Assert.True(success);
    }

    [Fact]
    public async Task DenseCaptioning_Dev()
    {
        bool success = await _analyze.DenseCaptioning(_demoImageFile);
        Assert.True(success);
    }
}