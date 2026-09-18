# BelPet

BelPet is a small Windows desktop pet built with **Godot 4 and C#**, featuring integrated Spotify playback controls.

Bel sits on your desktop as a transparent, always-on-top companion and lets you control Spotify without opening the Spotify window.

## Features

- 🎵 Spotify playback integration
- ⏯️ Play and pause music
- ⏭️ Skip and return to tracks
- 🔊 Scroll on Bel to control Spotify volume
- 💿 Displays the currently playing album artwork
- ⭕ Seek through the current song using the album progress ring
- 🖱️ Drag Bel around your desktop
- 🌍 Falls and rests above the Windows taskbar
- 🪟 Transparent desktop window with click-through areas
- 🌊 Animated volume visualization on Bel

## Built With

- **Godot 4**
- **C# / .NET**
- **Spotify Web API**
- **Spotify Web Playback SDK**
- **godot_wry / WebView2**

## Download

A pre-built Windows version is available from the **Releases** section.

Download the latest `BelPet-Windows.zip`, extract the folder, and run:

`BelPet.exe`

> Spotify Premium is required for Spotify Web Playback SDK functionality.

## Controls

| Action | Control |
| --- | --- |
| Move Bel | Click and drag |
| Volume | Scroll over Bel |
| Play / Pause | Click album artwork |
| Seek | Drag around the progress ring |
| Previous Track | Left button |
| Next Track | Right button |

## How It Works

BelPet combines Spotify's Web API with the Web Playback SDK to act as a Spotify Connect playback device.

The application uses a transparent native Godot window with custom mouse passthrough regions, allowing clicks outside of Bel's interactive areas to continue through to the desktop.

## Running From Source

1. Clone this repository.
2. Open the project using the .NET version of Godot 4.
3. Build the C# project.
4. Run the project from Godot.

Spotify authentication must be configured before Spotify functionality can be used.

## License

This project is currently provided primarily as a personal/portfolio project.
