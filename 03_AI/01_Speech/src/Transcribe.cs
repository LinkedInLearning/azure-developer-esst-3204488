using System.Text.Json;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace LiL.AI.Speech;

public class Transcription
{

    string _speechKey = "";
    string _speechLocation = ""; 
    string _outputFolder;

    bool _successfulProcessing = false;

    public Transcription(string speechKey, string speechLocation, string outputFolder)
    {
        _speechKey = speechKey;
        _speechLocation = speechLocation;
        _outputFolder = outputFolder;

    }

    public async Task<bool> TranscribeSpeech(string audioFile)
    {

        AudioConfig audioConfig; 
        audioConfig = AudioConfig.FromWavFileInput(audioFile);
        
        SpeechConfig speechConfig;
        
        speechConfig = SpeechConfig.FromSubscription(_speechKey, _speechLocation);
        speechConfig.SpeechRecognitionLanguage = "en-US"; 
        speechConfig.OutputFormat = OutputFormat.Detailed; 
        speechConfig.EnableDictation();

        await transcribeSpeech(speechConfig, audioConfig);

        return _successfulProcessing; 
    }

    async Task transcribeSpeech(SpeechConfig speechConfig, AudioConfig audioConfig )
    {
        using SpeechRecognizer speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig); 
        
        speechRecognizer.Recognizing += SpeechRecognizingHandler; 
        speechRecognizer.Recognized += SpeechRecognizedHandler; 
        
        SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
    }

    void SpeechRecognizingHandler (object? sender, SpeechRecognitionEventArgs speechRecognitionEventArgs) 
    {
        Console.WriteLine($"{speechRecognitionEventArgs.Result.Text}");     
    }

    void SpeechRecognizedHandler (object? s, SpeechRecognitionEventArgs speechRecognitionEventArgs) 
    {

        SpeechRecognitionResult speechRecognitionResult = speechRecognitionEventArgs.Result; 

        if (speechRecognitionResult.Reason == ResultReason.RecognizedSpeech) 
        {
            string fileName = ""; 
            
            //Recognized Text
            fileName = Path.Combine(_outputFolder, "Transcription.txt"); 
            File.WriteAllText(fileName, speechRecognitionResult.Text);

            //Detailed Recognition
            var detailedResult = speechRecognitionResult.Best();  
            fileName = Path.Combine(_outputFolder, "Transcription_Details.json");
            File.WriteAllText(fileName, JsonSerializer.Serialize(detailedResult)); 
        
            _successfulProcessing = true;
        } 
        else if (speechRecognitionResult.Reason == ResultReason.Canceled)
        {
            CancellationDetails cancellationDetails = CancellationDetails.FromResult(speechRecognitionResult);
            Console.WriteLine($"Canceled due to: {cancellationDetails.Reason}");
            if (cancellationDetails.Reason == CancellationReason.Error) {
                Console.WriteLine($"Canceled due to error: {cancellationDetails.ErrorDetails}");
            }
            _successfulProcessing = false;
        }
    }
}
