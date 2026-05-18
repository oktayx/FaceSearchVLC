# FaceSearchVLC

Real-time face recognition while watching video on VLC media player. The application monitors VLC's snapshot folder and sends each new screenshot to a face recognition API. When a face is identified, a Windows toast notification displays the person's name, category, and the captured image. Clicking the notification opens an IMDB search for the recognized person.

## How It Works

1. **Folder Watcher** (`nfwFolderCheck`) — monitors the VLC snapshot directory for new image files using `FileSystemWatcher`.
2. **Face Recognition API Call** — each captured screenshot is uploaded to a local face search service at `http://127.0.0.1:5555/api/face/SearchFace?treshold=45`.
3. **Toast Notification** (`ConsoleToast`) — displays a Windows 10 toast notification with the recognized person's name, list/category, and the captured photo.
4. **IMDB Lookup** — clicking the notification launches Chrome and searches IMDB for the recognized person.

## Prerequisites

- Windows 10 or later
- .NET Framework 4.5.2
- Visual Studio 2019+ (or MSBuild)
- VLC media player installed
- A face recognition API service running locally on port 5555

## Project Structure

```
FaceSearchVLC/
├── nfwFolderCheck/          # Main application (folder watcher + API client)
│   ├── Program.cs           # Entry point, FileSystemWatcher, file upload logic
│   └── nfwFolderCheck.csproj
├── ConsoleToast/            # Windows toast notification library
│   ├── Program.cs           # Toast display, shortcut creation, IMDB launch
│   ├── ShellHelper.cs       # Windows Shell API helpers
│   ├── toast1.xml           # Toast notification XML template
│   └── ConsoleToast.csproj
├── lib/                     # Shared libraries
│   └── Newtonsoft.Json.dll
└── nfwFolderCheckGithub.sln # Solution file
```

## Configuration

### VLC Snapshot Path

The application reads VLC's snapshot folder from the VLC config file located at:

```
%APPDATA%\vlc\vlcrc
```

It extracts the `snapshot-path` setting automatically. Make sure VLC is configured to save snapshots to a specific folder (VLC > Preferences > Video > Snapshot directory).

### Face Recognition API

The API endpoint is defined in `nfwFolderCheck/Program.cs`:

```csharp
static string apiAddress = "http://127.0.0.1:5555/api/face/SearchFace?treshold=45";
```

- **Address**: update if your face recognition service runs on a different host/port.
- **Threshold**: adjust the `treshold` query parameter to control recognition sensitivity (lower = more lenient, higher = stricter).

### Expected API Response

```json
{"AD":"Person Name","LISTE":"Category","SKOR":0.47}
```

| Field  | Description                        |
|--------|------------------------------------|
| AD     | Recognized person's name           |
| LISTE  | Category/list the person belongs to|
| SKOR   | Confidence score                   |

## Build

Open `nfwFolderCheckGithub.sln` in Visual Studio and build, or use the command line:

```
msbuild nfwFolderCheckGithub.sln /p:Configuration=Release
```

## Run

1. Ensure the face recognition API service is running on `http://127.0.0.1:5555`.
2. Start VLC and play a video.
3. Run `nfwFolderCheck.exe` from the build output directory.
4. Take a snapshot in VLC (default shortcut: `Shift+S`). The application will detect the new image, send it to the API, and display a toast notification with the result.

## Notes

- `toast1.xml` and `face.png` must be present in the same directory as the executable for toast notifications to work.
- The application creates a Start Menu shortcut on first run (required for Windows toast notifications).
- Clicking the toast notification opens an IMDB search in Chrome. This can be customized in `ConsoleToast/Program.cs`.
