using DotNetEnv; 
using LiL.AI.Speech; 

namespace LiL.AI.Speech.Test;

public class SpeechTest
{

    string _speechKey = "";
    string _speechLocation = ""; 
    string _demoAudioFile = "";
    string _demoTextFile = "";
    string _outputFolder = "";

    Transcription _transcription; 
    Translate _translate;
    Synthesize _synthesize;

    public SpeechTest()
    {
        string configurationFile = "../../../../CreateEnv/Configuration.env";
        Env.Load(configurationFile);
    
        _speechKey = Environment.GetEnvironmentVariable("LI4DEVS.SPEECH.KEY")??""; 
        _speechLocation = Environment.GetEnvironmentVariable("LI4DEVS.SPEECH.LOCATION")??"";

        _demoAudioFile = "../../../../Assets/Input/ProductInformation_AudioAsset.wav";
        _demoTextFile = "../../../../Assets/Input/ProductInformation_TextAsset.txt";
        _outputFolder = "../../../../Assets/Output";

        _transcription = new Transcription(
            _speechKey, 
            _speechLocation,
            _outputFolder
        );

        _translate = new Translate(
            _speechKey, 
            _speechLocation,
            _outputFolder
        );

        _synthesize = new Synthesize(
            _speechKey, 
            _speechLocation,
            _outputFolder
        );
    
    }

    [Fact]
    public async Task TranscribeSpeech_Dev()
    {
        bool success = await _transcription.TranscribeSpeech(_demoAudioFile);
        
        Assert.True(success); 
    }

    [Fact]
    public async Task TranslateSpeech_Dev()
    {
        bool success = await _translate.TranslateSpeech(_demoAudioFile);
        
        Assert.True(success); 
    }

    [Fact]
    public async Task SynthesizeText_Dev()
    {
        bool success = await _synthesize.SynthesizeText(_demoTextFile);
        
        Assert.True(success); 
    }
}