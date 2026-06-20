# MyPdfReader

A minimal WPF PDF viewer for Windows: open a PDF, view it, zoom in/out, and
move between pages. Rendering is powered by **PDFium** (the same engine
Chrome uses) via the **PdfiumViewer.Core** NuGet package.

## Project layout

```
MyPdfReader/
├── MyPdfReader.sln
├── src/
│   ├── MyPdfReader.csproj
│   ├── App.xaml / App.xaml.cs / App.config
│   ├── Core/
│   │   └── PdfEngine.cs        # Opens/renders/navigates PDFs via PDFium
│   ├── UI/
│   │   ├── MainWindow.xaml     # Toolbar + page viewer layout
│   │   └── MainWindow.xaml.cs  # Button click handlers, UI state
│   └── Assets/
│       ├── App.ico
│       ├── icon-open.png
│       ├── icon-zoom-in.png
│       └── icon-next.png
└── bin/Release/                # NOT checked in — produced by building
    ├── MyPdfReader.exe
    ├── MyPdfReader.config
    └── pdfium.dll
```

`bin/` and `.vs/` are build/IDE output, not source — they're listed in
`.gitignore` and will appear automatically the first time you build.

## Requirements

- Windows 10/11
- Visual Studio 2022 (17.8+) with the **.NET desktop development** workload
- .NET 8 SDK (the `net8.0-windows` target in the `.csproj`)

## Building

1. Open `MyPdfReader.sln` in Visual Studio.
2. Set the solution platform to **x64** (the toolbar dropdown next to
   Debug/Release) — PDFium's native `pdfium.dll` is 64-bit only, and the
   `.csproj` already pins `PlatformTarget` to `x64` to match.
3. Build → Restore NuGet packages will pull in `PdfiumViewer.Core`, which
   also drops the native `pdfium.dll` into the output folder for you.
4. Press **F5** to run, or build in **Release** mode to produce
   `bin/Release/net8.0-windows/MyPdfReader.exe`.

If you'd rather supply `pdfium.dll` yourself (e.g. a specific build), drop
it directly into `bin/Release/` alongside `MyPdfReader.exe` — PDFium is
loaded at runtime, not link time, so no rebuild is needed.

## Using the app

| Action          | How                                      |
|------------------|------------------------------------------|
| Open a PDF       | Toolbar "Open" button, or `Ctrl+O`       |
| Next / previous page | Chevron buttons in the toolbar      |
| Jump to a page   | Type a page number in the box, press Enter |
| Zoom in / out    | `+` / `−` buttons in the toolbar         |

## Where to extend next

- `Core/PdfEngine.cs` is intentionally self-contained — it has no knowledge
  of WPF controls, so it's a good place to add features like text search,
  password-protected file handling, or page thumbnails without touching the
  UI code.
- `UI/MainWindow.xaml(.cs)` only handles presentation. Add a thumbnail
  sidebar, a recent-files menu, or printing support here.
- The `Assets/` icons included here are simple generated placeholders —
  swap them for your own artwork; they're already wired up via the
  `<Resource>` items in the `.csproj`, so no project file changes needed.

## Troubleshooting

- **"BadImageFormatException" or pdfium fails to load**: make sure the
  solution platform is set to **x64**, not "Any CPU" or "x86". PDFium's
  native binary is 64-bit only.
- **NuGet restore fails for `PdfiumViewer.Core`**: check
  Tools → NuGet Package Manager → Package Sources includes nuget.org.
