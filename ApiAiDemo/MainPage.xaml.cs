using ApiAiSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Media.SpeechSynthesis;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using ApiAiSDK.Model;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ApiAiDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SpeechRecognizer speechRecognizer;
        private SpeechSynthesizer speechSynthesizer;
        private ApiAi apiAi;
        //private readonly CoreDispatcher coreDispatcher;


        public MainPage()
        {
            InitializeComponent();
            
            speechSynthesizer = new SpeechSynthesizer();
        
            var config = new AIConfiguration("cb9693af-85ce-4fbf-844a-5563722fc27f",
                                 "40048a5740a1455c9737342154e86946",
                                 SupportedLanguage.English);

            apiAi = new ApiAi(config);

            mediaElement.MediaEnded += MediaElement_MediaEnded;

            //coreDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            

        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("MediaElement_MediaEnded");
            Listen_Click(listenButton, null);
        }

        private void SpeechRecognizer_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            Debug.WriteLine("SpeechRecognizer_StateChanged " + args.State);

            switch (args.State)
            {
                case SpeechRecognizerState.Idle:
                    Dispatcher.RunAsync(CoreDispatcherPriority.High, () => listenButton.Content = "Processing");
                    break;
                case SpeechRecognizerState.Capturing:
                    Dispatcher.RunAsync(CoreDispatcherPriority.High, () => listenButton.Content = "Listening...");
                    break;
                case SpeechRecognizerState.Processing:
                    break;
                case SpeechRecognizerState.SoundStarted:
                    break;
                case SpeechRecognizerState.SoundEnded:
                    break;
                case SpeechRecognizerState.SpeechDetected:
                    break;
                case SpeechRecognizerState.Paused:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var appView = ApplicationView.GetForCurrentView();
            var titleBar = appView.TitleBar;
            titleBar.BackgroundColor = Color.FromArgb(255, 43, 48, 62);
            titleBar.InactiveBackgroundColor = Color.FromArgb(255, 43, 48, 62);
            titleBar.ButtonBackgroundColor = Color.FromArgb(255, 43, 48, 62);
            titleBar.ButtonInactiveBackgroundColor = Color.FromArgb(255, 43, 48, 62);
            titleBar.ForegroundColor = Color.FromArgb(255, 247, 255, 255);
            

            if (e.Parameter != null)
            {
                var param = Convert.ToString(e.Parameter);
                if (!string.IsNullOrEmpty(param))
                {
                    TryLoadAiResponse(param);
                }
                
            }

            InitializeRecognizer();
        }

        private void TryLoadAiResponse(string s)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<AIResponse>(s);
                OutputJson(response);
                OutputParams(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (speechRecognizer != null)
            {
                if (speechRecognizer.State != SpeechRecognizerState.Idle)
                {
                    
                }

                speechRecognizer.StateChanged -= SpeechRecognizer_StateChanged;

                speechRecognizer.Dispose();
                speechRecognizer = null;
            }
        }

        private async Task InitializeRecognizer()
        {
            if (speechRecognizer != null)
            {
                // cleanup prior to re-initializing this scenario.
                speechRecognizer.StateChanged -= SpeechRecognizer_StateChanged;

                speechRecognizer.Dispose();
                speechRecognizer = null;
            }

            speechRecognizer = new SpeechRecognizer(new Language("en-US"));
            speechRecognizer.StateChanged += SpeechRecognizer_StateChanged;

//            var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation");
//            speechRecognizer.Constraints.Add(dictationConstraint);

            await speechRecognizer.CompileConstraintsAsync();
            listenButton.IsEnabled = true;
        }

        private async void Listen_Click(object sender, RoutedEventArgs e)
        {

            var sessionId = RestoreSessionId();
            if (!string.IsNullOrEmpty(sessionId))
            {
                apiAi.SessionId = sessionId;
            }

            if(mediaElement.CurrentState == MediaElementState.Playing)
            {
                mediaElement.Stop();
            }

            if (speechRecognizer.State != SpeechRecognizerState.Idle)
            {
                try
                {
                    await speechRecognizer.StopRecognitionAsync();
                    return;
                }
                catch (Exception)
                {
                    
                }
            }

            try
            {
                
                var recognitionResults = await speechRecognizer.RecognizeAsync();    
             
                if (recognitionResults != null && recognitionResults.Status == SpeechRecognitionResultStatus.Success)
                {
                    var requestText = recognitionResults.Text;
                    
                    if (!string.IsNullOrEmpty(requestText))
                    {
                        var aiRequest = CreateAiRequest(requestText, recognitionResults);

                        try
                        {
                            var aiResponse = await apiAi.TextRequestAsync(aiRequest);

                            if (aiResponse != null)
                            {
                                OutputJson(aiResponse);
                                OutputParams(aiResponse);
                                
                                var speechText = aiResponse.Result?.Fulfillment?.Speech;
                                if (!string.IsNullOrEmpty(speechText))
                                {
                                    var speechStream = await speechSynthesizer.SynthesizeTextToStreamAsync(speechText);
                                    mediaElement.SetSource(speechStream, speechStream.ContentType);
                                    mediaElement.Play();
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            resultTextBlock.Text = ex.ToString();
                        }
                    }
                    else
                    {
                        //resultTextBlock.Text = "Empty recognition result";
                    }
                }
                else
                {
                    //resultTextBlock.Text = "Empty or error result";
                }

                listenButton.Content = "Listen";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                resultTextBlock.Text = "Empty or error result";
                
                listenButton.Content = "Listen";
            }
            
        }

        private void OutputParams(AIResponse aiResponse)
        {
            var contextsParams = new Dictionary<string,string>();

            if (aiResponse.Result?.Contexts != null)
            {
                foreach (var context in aiResponse.Result?.Contexts)
                {
                    if (context.Parameters != null)
                    {
                        foreach (var parameter in context.Parameters)
                        {
                            if (!contextsParams.ContainsKey(parameter.Key))
                            {
                                contextsParams.Add(parameter.Key, parameter.Value);
                            }
                        }
                    }
                }
            }

            var resultBuilder = new StringBuilder();
            foreach (var contextsParam in contextsParams)
            {
                resultBuilder.AppendLine(contextsParam.Key + ": " + contextsParam.Value);
            }

            parametersTextBlock.Text = resultBuilder.ToString();
        }

        private void OutputJson(AIResponse aiResponse)
        {
            resultTextBlock.Text = JsonConvert.SerializeObject(aiResponse, Formatting.Indented);
        }

        private AIRequest CreateAiRequest(string requestText, SpeechRecognitionResult recognitionResults)
        {
            var texts = new List<string> {requestText};
            var confidences = new List<float> {ConfidenceToFloat(recognitionResults.Confidence)};

            var aiRequest = new AIRequest();

            var alternates = recognitionResults.GetAlternates(5);
            if (alternates != null)
            {
                foreach (var a in alternates)
                {
                    texts.Add(a.Text);
                    confidences.Add(ConfidenceToFloat(a.Confidence));
                }
            }
            aiRequest.Query = texts.ToArray();
            aiRequest.Confidence = confidences.ToArray();
            return aiRequest;
        }

        private float ConfidenceToFloat(SpeechRecognitionConfidence confidence)
        {
            switch (confidence)
            {
#pragma warning disable 162
                case SpeechRecognitionConfidence.High:
                    return 0.99f;
                    break;
                case SpeechRecognitionConfidence.Medium:
                    return 0.6f;
                    break;
                case SpeechRecognitionConfidence.Low:
                    return 0.3f;
                    break;
                case SpeechRecognitionConfidence.Rejected:
                    return 0;
                    break;
#pragma warning restore 162
                default:
                    throw new ArgumentOutOfRangeException(nameof(confidence), confidence, null);
            }
        }

        private async void InstallVoiceCommands_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///VoiceCommands.xml"));
                await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(storageFile);

                parametersTextBlock.Text = "Voice commands installed";
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                resultTextBlock.Text = ex.ToString();
            }
        }

        private async void UninstallCommands_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///UninstallCommands.xml"));
                await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(storageFile);

                parametersTextBlock.Text = "Voice commands uninstalled";

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                resultTextBlock.Text = ex.ToString();
            }
        }

        private string RestoreSessionId()
        {
            var roamingSettings = ApplicationData.Current.LocalSettings;
            if (roamingSettings.Values.ContainsKey("SessionId"))
            {
                var sessionId = Convert.ToString(roamingSettings.Values["SessionId"]);
                return sessionId;
            }
            return string.Empty;
        }

        private void JsonButton_Click(object sender, RoutedEventArgs e)
        {
            jsonContaner.Visibility = jsonContaner.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
            
        }
    }
}
