using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ApiAiSDK;

namespace ApiAiDemo
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public AIService AIService { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);

            InitializeComponent();
            Suspending += OnSuspending;
            Resuming += OnResuming;

            var config = new AIConfiguration("cb9693af-85ce-4fbf-844a-5563722fc27f",
                                 "40048a5740a1455c9737342154e86946",
                                 SupportedLanguage.English);

            AIService = AIService.CreateService(config);
        }

        private void OnResuming(object sender, object e)
        {
            Debug.WriteLine("OnResuming");
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            Debug.WriteLine("OnSuspending");

            var deferral = e.SuspendingOperation.GetDeferral();
            if (AIService != null)
            {
                AIService.Dispose();
                AIService = null;
            }
            deferral.Complete();
        }
        
        protected override void OnActivated(IActivatedEventArgs e)
        {
            // Was the app activated by a voice command?
            //if (e.Kind != ActivationKind.VoiceCommand)
            //{
            //    base.OnActivated(e);
            //    return;
            //}

            AIService.ProcessOnActivated(e);

            var protocolArgs = e as ProtocolActivatedEventArgs;
            var callParameter = protocolArgs?.Uri?.Query;

            if (!string.IsNullOrEmpty(callParameter))
            {
                callParameter = callParameter.Substring("?LaunchContext=".Length);
                callParameter = Uri.UnescapeDataString(callParameter);
            }
            
            //// Get the name of the voice command and the text spoken
            //var voiceCommandName = speechRecognitionResult.RulePath[0];
            //var textSpoken = speechRecognitionResult.Text;
            //// The commandMode is either "voice" or "text", and it indicates how the voice command was entered by the user.
            //// Apps should respect "text" mode by providing feedback in a silent form.
            //var commandMode = speechRecognitionResult.SemanticInterpretation;

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            
            rootFrame.Navigate(typeof(MainPage), callParameter);

            // Ensure the current window is active
            Window.Current.Activate();
        }
    }
}
