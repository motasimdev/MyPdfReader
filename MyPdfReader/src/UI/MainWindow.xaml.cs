using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MyPdfReader.Core;

namespace MyPdfReader.UI
{
    /// <summary>
    /// Code-behind for the main window. Wires toolbar button clicks to the
    /// PdfEngine and keeps the page/zoom UI in sync with engine state.
    /// All PDF logic itself lives in Core/PdfEngine.cs - this class only
    /// handles user interaction and presentation.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PdfEngine _engine = new();

        // Step sizes used by the zoom buttons; mirrors App.config defaults.
        private const double ZoomStep = 0.1;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _engine.Dispose();
        }

        // ===================== OPEN =====================

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open PDF",
                Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog(this) != true)
                return;

            try
            {
                _engine.Open(dialog.FileName);
                RenderPage();
                UpdateNavigationState();
                StatusText.Text = $"Opened: {dialog.FileName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not open this PDF.\n\n{ex.Message}",
                    "MyPdfReader",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        // ===================== NAVIGATION =====================

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            _engine.NextPage();
            RenderPage();
            UpdateNavigationState();
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            _engine.PreviousPage();
            RenderPage();
            UpdateNavigationState();
        }

        private void PageNumberBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            if (int.TryParse(PageNumberBox.Text, out int requestedPage))
            {
                // UI is 1-based, engine is 0-based.
                int zeroBasedIndex = requestedPage - 1;

                if (zeroBasedIndex >= 0 && zeroBasedIndex < _engine.PageCount)
                {
                    _engine.GoToPage(zeroBasedIndex);
                    RenderPage();
                    UpdateNavigationState();
                    return;
                }
            }

            // Invalid input: reset the box back to the actual current page.
            PageNumberBox.Text = (_engine.CurrentPageIndex + 1).ToString();
        }

        // ===================== ZOOM =====================

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            _engine.ZoomIn(ZoomStep);
            RenderPage();
            UpdateZoomDisplay();
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            _engine.ZoomOut(ZoomStep);
            RenderPage();
            UpdateZoomDisplay();
        }

        // ===================== UI HELPERS =====================

        private void RenderPage()
        {
            if (!_engine.IsDocumentOpen) return;

            PageImage.Source = _engine.RenderCurrentPage();
            EmptyStateText.Visibility = Visibility.Collapsed;
        }

        private void UpdateNavigationState()
        {
            bool documentOpen = _engine.IsDocumentOpen;

            PreviousButton.IsEnabled = _engine.CanGoPrevious;
            NextButton.IsEnabled = _engine.CanGoNext;
            ZoomInButton.IsEnabled = documentOpen;
            ZoomOutButton.IsEnabled = documentOpen;
            PageNumberBox.IsEnabled = documentOpen;

            if (documentOpen)
            {
                PageNumberBox.Text = (_engine.CurrentPageIndex + 1).ToString();
                PageCountText.Text = $"/ {_engine.PageCount}";
            }

            UpdateZoomDisplay();
        }

        private void UpdateZoomDisplay()
        {
            ZoomLevelText.Text = $"{_engine.ZoomFactor * 100:0}%";
        }
    }
}
