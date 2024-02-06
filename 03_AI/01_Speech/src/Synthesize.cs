using System.Text.Json;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;

namespace LiL.AI.Speech;

public class Synthesize
{

    string _speechKey = "";
    string _speechLocation = ""; 
    string _outputFolder;

    bool _successfulProcessing = false;

    public Synthesize(string speechKey, string speechLocation, string outputFolder)
    {
        _speechKey = speechKey;
        _speechLocation = speechLocation;
        _outputFolder = outputFolder;

    }

    public async Task<bool> SynthesizeText(string textFile)
    {
        SpeechConfig speechConfig;
        
        speechConfig = SpeechConfig.FromSubscription(_speechKey, _speechLocation);      
        speechConfig.SpeechSynthesisVoiceName = "de-DE-KatjaNeural";
        speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff24Khz16BitMonoPcm);

        string text = File.ReadAllText(textFile);

        SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer(speechConfig, null);

        speechSynthesizer.Synthesizing += SynthesizeSynthesizingHandler; 
        speechSynthesizer.SynthesisCompleted += SynthesizeSynthesisCompletedHandler;

        await speechSynthesizer.SpeakTextAsync(text);

        return _successfulProcessing;
    }

    private void SynthesizeSynthesizingHandler(object? sender, SpeechSynthesisEventArgs speechSynthesisEventArgs)
    {
        Console.WriteLine($"{speechSynthesisEventArgs.Result.AudioData.Length} bytes received.");
    }


    private void SynthesizeSynthesisCompletedHandler(object? sender, SpeechSynthesisEventArgs speechSynthesisEventArgs)
    {
        SpeechSynthesisResult speechSynthesisResult = speechSynthesisEventArgs.Result;

        if (speechSynthesisResult.Reason == ResultReason.SynthesizingAudioCompleted)
        {
            File.WriteAllBytes($"{_outputFolder}/SpeechSynthesize.wav", speechSynthesisResult.AudioData);
            _successfulProcessing = true;
        }
        else if (speechSynthesisResult.Reason == ResultReason.Canceled)
        {
            SpeechSynthesisCancellationDetails speechSynthesisCancellationDetails = 
                SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);

            Console.WriteLine($"Canceled due to: {speechSynthesisCancellationDetails.Reason}");
            if (speechSynthesisCancellationDetails.Reason == CancellationReason.Error) 
            {
                Console.WriteLine($"Canceled due to error: {speechSynthesisCancellationDetails.ErrorDetails}");
            }

           _successfulProcessing = false;
        }
    }
}
