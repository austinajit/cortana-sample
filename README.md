# API.AI Cortana Integration Sample
Sample app using Cortana with an Api.ai agent. 

[ApiAiSDK](https://www.nuget.org/packages/ApiAiSDK/) installed to project using Nuget.

## App class

In the constructor of the `App` class, initialize `AIService`:

```csharp
var config = new AIConfiguration("YOUR_SUBSCRIPTION_KEY",
                                 "YOUR_ACCESS_TOKEN",
                                 SupportedLanguage.English);

AIService = AIService.CreateService(config);
```

Add processing of `IActivatedEventArgs` received from Cortana in the `OnActivated` method:
```csharp
protected async override void OnActivated(IActivatedEventArgs e)
{
    AIResponse aiResponse = null;
    try
    {
        aiResponse = await AIService.ProcessOnActivatedAsync(e);
    }
    catch (Exception)
    {
        // ignored
    }

    NavigateToMain(aiResponse);
}
```

This code makes request to the Api.ai service and then activates **MainPage** with the `AIResponse` parameter. The app can process Api.ai response on the MainPage (e.g. it could be pronounced via a Text-to-Speech).

## MainPage class

In the `MainPage` class, implement processing of the `AIResponse` parameter:
```csharp
protected override void OnNavigatedTo(NavigationEventArgs e)
{
    ...

    var response = e.Parameter as AIResponse;
    if (response != null)
    {
        var aiResponse = response;
        OutputJson(aiResponse); // print response as JSON to a textBlock
        OutputParams(aiResponse); // print action parameters to a textBlock
    }
    
   ...
}
```

Also in the MainPage implement voice requests to the API.AI service and responses pronunciation. See methods `Listen_Click` and `ProcessResult`.

## ApiAiVoiceCommandService class

`ApiAiVoiceCommandService` class implements background processing, returns response to Cortana or launches the application.
It uses `ApiAi` object to make requests to an Api.ai agent.

Important: there are multiple ways of processing the commands - see examples below:

```csharp
 switch (voiceCommand.CommandName)
{
    case "type":
        {
            var aiResponse = await apiAi.TextRequestAsync(recognizedText);
            await apiAi.LaunchAppInForegroundAsync(voiceServiceConnection, aiResponse);
        }
        break;
    case "unknown":
        {
            if (!string.IsNullOrEmpty(recognizedText))
            {
                var aiResponse = await apiAi.TextRequestAsync(recognizedText);
                if (aiResponse != null)
                {
                    await apiAi.SendResponseToCortanaAsync(voiceServiceConnection, aiResponse);
                }
            }
        }
        break;
        ...
}
```


