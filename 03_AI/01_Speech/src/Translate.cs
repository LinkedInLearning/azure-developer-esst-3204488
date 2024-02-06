using System.Text.Json;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;

namespace LiL.AI.Speech;

public class Translate
{

    string _speechKey = "";
    string _speechLocation = ""; 
    string _outputFolder;

    bool _successfulProcessing = false;

    public Translate(string speechKey, string speechLocation, string outputFolder)
    {
        _speechKey = speechKey;
        _speechLocation = speechLocation;
        _outputFolder = outputFolder;

    }

    public async Task<bool> TranslateSpeech(string audioFile)
    {

        AudioConfig audioConfig; 
        audioConfig = AudioConfig.FromWavFileInput(audioFile);
        
        SpeechTranslationConfig speechTranslationConfig;
        
        speechTranslationConfig = SpeechTranslationConfig.FromSubscription(_speechKey, _speechLocation);        
        speechTranslationConfig.SpeechRecognitionLanguage = "en-US";
        speechTranslationConfig.AddTargetLanguage("de");

        using TranslationRecognizer translationRecognizer = 
            new TranslationRecognizer(speechTranslationConfig, audioConfig);

        translationRecognizer.Recognized += TranslateRecognizedHandler;
        translationRecognizer.Recognizing += TranslateRecognizingHandler;
        
        TranslationRecognitionResult translationRecognitionResult 
            = await translationRecognizer.RecognizeOnceAsync();

        return _successfulProcessing;
    }

    private void TranslateRecognizingHandler(object? sender, TranslationRecognitionEventArgs e)
    {
        Console.WriteLine($"{e.Result.Translations["de"]}");
    }

    private void TranslateRecognizedHandler(object? sender, TranslationRecognitionEventArgs translationRecognitionEventArgs)
    {
        TranslationRecognitionResult translateRecognitionResult = translationRecognitionEventArgs.Result; 

        if (translateRecognitionResult.Reason == ResultReason.TranslatedSpeech) 
        {
            //Translated Text
            string fileName = Path.Combine(_outputFolder, "Translation.txt"); 
            File.WriteAllText(fileName, translateRecognitionResult.Translations["de"]);

            _successfulProcessing = true;
        } 
        else if (translateRecognitionResult.Reason == ResultReason.Canceled)
        {
            CancellationDetails cancellationDetails = CancellationDetails.FromResult(translateRecognitionResult);
            Console.WriteLine($"Canceled due to: {cancellationDetails.Reason}");

            if (cancellationDetails.Reason == CancellationReason.Error) {
                Console.WriteLine($"Canceled due to error: {cancellationDetails.ErrorDetails}");
            }

            _successfulProcessing = false;
        }
    }
}
