using System;
using System.Windows;

namespace MyPdfReader
{
    /// <summary>
    /// Application entry point. Handles global startup logic and
    /// catches unhandled exceptions so the app fails gracefully
    /// instead of crashing silently.
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Catch anything PdfiumViewer or the rendering pipeline throws
            // that isn't handled locally, so the user gets a message
            // instead of a silent crash.
            DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show(
                    $"An unexpected error occurred:\n\n{args.Exception.Message}",
                    "MyPdfReader - Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                args.Handled = true;
            };
        }
    }
}
