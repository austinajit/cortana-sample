﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using ApiAiSDK;
using System.Diagnostics;
using Windows.Storage;

namespace ApiAiDemo.VoiceCommands
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

            var sessionId = RestoreSessionId();
            if (string.IsNullOrEmpty(sessionId))
            {
                PersistSessionId("my_session_id");
            }

            if (triggerDetails != null)
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
                    var recognizedText = voiceCommand.SpeechRecognitionResult?.Text;

                    switch (voiceCommand.CommandName)
                    {
                        case "greetings":
                            {
                                var aiResponse = await apiAi.TextRequestAsync(recognizedText);
                                
                                var repeatMessage = new VoiceCommandUserMessage
                                {
                                    DisplayMessage = "Repeat please",
                                    SpokenMessage = "Repeate please"
                                };

                                var processingMessage = new VoiceCommandUserMessage
                                {
                                    DisplayMessage = aiResponse?.Result?.Fulfillment?.Speech,
                                    SpokenMessage = ""
                                };

                                var resp = VoiceCommandResponse.CreateResponseForPrompt(processingMessage, repeatMessage);
                                await voiceServiceConnection.ReportSuccessAsync(resp);
                                break;
                            }

                        case "unknown":
                            {
                                if (!string.IsNullOrEmpty(recognizedText))
                                {
                                    var aiResponse = await apiAi.TextRequestAsync(recognizedText);
                                    if(aiResponse != null)
                                    {
                                        await SendResponse(aiResponse.Result.Fulfillment?.Speech);
                                    }   
                                }
                            }
                            break;
                        default:
                            await SendResponse("unknown command");
                            break;
                    }
                    
                }
                catch(Exception e)
                {
                    var message = e.ToString();
                    Debug.WriteLine(message);
                }
                finally
                {
                    serviceDeferral?.Complete();
                }
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            serviceDeferral?.Complete();            
        }

        private void VoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            serviceDeferral?.Complete();
        }

        private async Task SendResponse(string textResponse)
        {
            // Take action and determine when the next trip to destination
            // Inset code here

            // Replace the hardcoded strings used here with strings 
            // appropriate for your application.

            // First, create the VoiceCommandUserMessage with the strings 
            // that Cortana will show and speak.
            var userMessage = new VoiceCommandUserMessage();
            userMessage.DisplayMessage = textResponse;
            userMessage.SpokenMessage = textResponse;

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

        private void PersistSessionId(string sessionId)
        {
            var roamingSettings = ApplicationData.Current.LocalSettings;
            roamingSettings.Values["SessionId"] = sessionId;
        }

        private string RestoreSessionId()
        {
            var roamingSettings = ApplicationData.Current.LocalSettings;
            var sessionId = Convert.ToString(roamingSettings.Values["SessionId"]);
            return sessionId;
        }
    }
}
