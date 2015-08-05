using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using ApiAiSDK;

namespace ApiAiDemo
{
    public sealed class ApiAiVoiceCommandService : IBackgroundTask
    {
        private BackgroundTaskDeferral serviceDeferral;
        private VoiceCommandServiceConnection voiceServiceConnection;
        private ApiAi apiAi;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            serviceDeferral = taskInstance.GetDeferral();
     
            taskInstance.Canceled += OnTaskCanceled;

            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            if (triggerDetails != null && (triggerDetails.Name?.Contains("ApiAiVoice") ?? false))
            {

                var config = new AIConfiguration("cb9693af-85ce-4fbf-844a-5563722fc27f",
                                "fa16c9b66e5d4823bbf47640619ad86c",
                                SupportedLanguage.English);

                apiAi = new ApiAi(config);

                try
                {
                    voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);
                    voiceServiceConnection.VoiceCommandCompleted += VoiceCommandCompleted;
                    var voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();

                    switch (voiceCommand.CommandName)
                    {
                        case "greetings":
                            {
                                //var destination =
                                //  voiceCommand.Properties["destination"][0];
                                SendCompletionMessageForDestination("greet");
                                break;
                            }

                        // As a last resort launch the app in the foreground
                        default:
                            
                            break;
                    }
                }
                finally
                {
                    if (this.serviceDeferral != null)
                    {
                        //Complete the service deferral
                      //  this.serviceDeferral.Complete();
                    }
                }
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (serviceDeferral != null)
            {
                serviceDeferral.Complete();
            }
            
        }

        private void VoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (serviceDeferral != null)
            {
                // Insert your code here
                //Complete the service deferral
               // serviceDeferral.Complete();
            }
        }

        private async void SendCompletionMessageForDestination(string destination)
        {
            // Take action and determine when the next trip to destination
            // Inset code here

            // Replace the hardcoded strings used here with strings 
            // appropriate for your application.

            // First, create the VoiceCommandUserMessage with the strings 
            // that Cortana will show and speak.
            var userMessage = new VoiceCommandUserMessage();
            userMessage.DisplayMessage = "Hello! What kind of pizza would you like?";
            userMessage.SpokenMessage = "Hello! What kind of pizza would you like?";

            // Optionally, present visual information about the answer.
            // For this example, create a VoiceCommandContentTile with an 
            // icon and a string.
            //var destinationsContentTiles = new List<VoiceCommandContentTile>();

            //var destinationTile = new VoiceCommandContentTile();
            //destinationTile.ContentTileType =
            //  VoiceCommandContentTileType.TitleWith68x68IconAndText;
            // The user can tap on the visual content to launch the app. 
            // Pass in a launch argument to enable the app to deep link to a 
            // page relevant to the item displayed on the content tile.
            //destinationTile.AppLaunchArgument =
            //  string.Format("destination={0}”, “Las Vegas");
            //destinationTile.Title = "Las Vegas";
            //destinationTile.TextLine1 = "August 3rd 2015";
            //destinationsContentTiles.Add(destinationTile);

            // Create the VoiceCommandResponse from the userMessage and list    
            // of content tiles.
            var response = VoiceCommandResponse.CreateResponse(userMessage);

            // Cortana will present a “Go to app_name” link that the user 
            // can tap to launch the app. 
            // Pass in a launch to enable the app to deep link to a page 
            // relevant to the voice command.
            //response.AppLaunchArgument =
            //  string.Format("destination={0}”, “Las Vegas");

            // Ask Cortana to display the user message and content tile and 
            // also speak the user message.
            await voiceServiceConnection.ReportSuccessAsync(response);
        }
    }
}
