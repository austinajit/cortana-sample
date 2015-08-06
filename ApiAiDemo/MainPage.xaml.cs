using ApiAiSDK;
using System;
using System.Diagnostics;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Windows.ApplicationModel.VoiceCommands;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ApiAiDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SpeechRecognizer speechRecognizer;
        private ApiAi apiAi;

        public MainPage()
        {
            this.InitializeComponent();
            speechRecognizer = new SpeechRecognizer(new Language("en-US"));

            var config = new AIConfiguration("cb9693af-85ce-4fbf-844a-5563722fc27f",
                                 "fa16c9b66e5d4823bbf47640619ad86c",
                                 SupportedLanguage.English);

            apiAi = new ApiAi(config);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            resultTextBlock.Text = "OnNavigatedTo";

            if (e.Parameter != null)
            {
                object param = e.Parameter;
                resultTextBlock.Text = param.ToString();
            }
        }

        private async void Listen_Click(object sender, RoutedEventArgs e)
        {
            await speechRecognizer.CompileConstraintsAsync();
            var recognitionResults = await speechRecognizer.RecognizeWithUIAsync();

            if (recognitionResults !=  null)
            {
                var requestText = recognitionResults.Text;

                try
                {
                    var aiResponse = await apiAi.TextRequestAsync(requestText);
                    resultTextBlock.Text = JsonConvert.SerializeObject(aiResponse, Formatting.Indented);
                }
                catch(Exception ex)
                {
                    resultTextBlock.Text = ex.ToString();
                }
                
            }
            else
            {
                resultTextBlock.Text = "Empty or error result";
            }
        }

        private async void InstallVoiceCommands_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///VoiceCommands.xml"));
                await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(storageFile);

                resultTextBlock.Text = "Voice commands installed";
                
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

                resultTextBlock.Text = "Voice commands uninstalled";

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                resultTextBlock.Text = ex.ToString();
            }
        }
    }
}
