using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using PdfiumViewer;

namespace MyPdfReader.Core
{
    /// <summary>
    /// Wraps the PDFium native library (via PdfiumViewer.Core) and exposes
    /// a small, WPF-friendly API for the UI layer: open a file, render the
    /// current page to a bitmap, move between pages, and zoom.
    ///
    /// This class owns the lifetime of the underlying PdfDocument and the
    /// native pdfium.dll handles, so it implements IDisposable.
    /// </summary>
    public class PdfEngine : IDisposable
    {
        private PdfDocument? _document;
        private bool _disposed;

        /// <summary>Full path of the currently open PDF file, or null if none.</summary>
        public string? FilePath { get; private set; }

        /// <summary>Total number of pages in the open document.</summary>
        public int PageCount => _document?.PageCount ?? 0;

        /// <summary>Index (0-based) of the page currently being viewed.</summary>
        public int CurrentPageIndex { get; private set; }

        /// <summary>Current zoom factor, where 1.0 = 100%.</summary>
        public double ZoomFactor { get; set; } = 1.0;

        public bool IsDocumentOpen => _document != null;

        /// <summary>
        /// Opens a PDF file from disk. Throws if the file is missing,
        /// not a valid PDF, or password protected (callers should catch
        /// PdfiumViewer's PdfException / general exceptions and show a
        /// friendly message in the UI).
        /// </summary>
        public void Open(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("The selected PDF file could not be found.", path);

            // Dispose any previously open document before loading a new one.
            CloseInternal();

            _document = PdfDocument.Load(path);
            FilePath = path;
            CurrentPageIndex = 0;
            ZoomFactor = 1.0;
        }

        /// <summary>
        /// Renders the current page at the current zoom factor and returns
        /// it as a BitmapSource ready to bind to a WPF Image control.
        /// </summary>
        public BitmapSource RenderCurrentPage()
        {
            EnsureOpen();

            // PDFium reports page size in points (1/72 inch). We convert to
            // pixels at 96 DPI (WPF's native unit) and then apply zoom.
            SizeF pointSize = _document!.PageSizes[CurrentPageIndex];

            int width = (int)Math.Max(1, pointSize.Width * (96.0 / 72.0) * ZoomFactor);
            int height = (int)Math.Max(1, pointSize.Height * (96.0 / 72.0) * ZoomFactor);

            using Image rendered = _document.Render(
                CurrentPageIndex,
                width,
                height,
                96 * ZoomFactor,
                96 * ZoomFactor,
                PdfRenderFlags.Annotations);

            return ConvertToBitmapSource((Bitmap)rendered);
        }

        public bool CanGoNext => IsDocumentOpen && CurrentPageIndex < PageCount - 1;
        public bool CanGoPrevious => IsDocumentOpen && CurrentPageIndex > 0;

        public void NextPage()
        {
            EnsureOpen();
            if (CanGoNext) CurrentPageIndex++;
        }

        public void PreviousPage()
        {
            EnsureOpen();
            if (CanGoPrevious) CurrentPageIndex--;
        }

        public void GoToPage(int pageIndex)
        {
            EnsureOpen();
            if (pageIndex < 0 || pageIndex >= PageCount)
                throw new ArgumentOutOfRangeException(nameof(pageIndex));

            CurrentPageIndex = pageIndex;
        }

        public void ZoomIn(double step) => ZoomFactor = Math.Min(ZoomFactor + step, 4.0);

        public void ZoomOut(double step) => ZoomFactor = Math.Max(ZoomFactor - step, 0.25);

        public void ResetZoom() => ZoomFactor = 1.0;

        private void EnsureOpen()
        {
            if (_document == null)
                throw new InvalidOperationException("No PDF document is currently open.");
        }

        private static BitmapSource ConvertToBitmapSource(Bitmap bitmap)
        {
            using MemoryStream memory = new();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;

            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // Makes it safe to use across threads/UI updates

            return bitmapImage;
        }

        private void CloseInternal()
        {
            _document?.Dispose();
            _document = null;
            FilePath = null;
            CurrentPageIndex = 0;
        }

        public void Dispose()
        {
            if (_disposed) return;
            CloseInternal();
            _disposed = true;
        }
    }
}
