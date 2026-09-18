# BelPet

Bel is a small desktop pet that just chills and plays spotify songs for you :D 

## Features

- Spotify playback integration
- Play and pause music
- Skip and return to tracks
- Scroll on Bel to control Spotify volume
- Skip to different parts of the song
- Drag Bel around your desktop

## Built With

- **Godot 4 (C#)**
- **Spotify Web API**
- **Spotify Web Playback SDK**
- **godot_wry**

## Spotify Limitations

Due to Spotify's API development restrictions, Spotify integration is currently limited to authorized users added to the application's Spotify Developer account.

Since public users are not authorized on my Spotify Developer application, the Spotify features in the pre-built version of BelPet will probably not work for other users.

The current workaround is to create your own Spotify application through the Spotify Developer Dashboard and use your own Client ID.

## Using Your Own Client ID

1. Create your own application through the Spotify Developer Dashboard at https://developer.spotify.com.
2. Copy the Client ID provided by Spotify.
3. Open `SpotifyAuth.cs`.
4. Replace the existing `clientId` with your own Client ID.
5. Build and run the project.

BelPet should then authenticate through your own Spotify application instead.

## Download

Pre-built versions of BelPet are available under **Releases**.

Due to Spotify's development user restrictions, the pre-built version's Spotify functionality may only work for accounts that have been authorized on my Spotify Developer application.

## Getting Started

To start using BelPet with Spotify, you will need another Spotify device, such as the Spotify desktop app, mobile app, or Web Player.

Start playing a song on any of these devices, then either:

- Leave the song playing and BelPet will display and control the currently active Spotify device.
- Transfer playback to **PixePet** from Spotify's device selection menu to use BelPet itself as the active playback device.

BelPet cannot start playback on its own if there is no active Spotify session, so make sure Spotify is already active on another device first.

## Controls

| Action | Control |
| --- | --- |
| Move Bel | Click and drag Bel |
| Volume | Scroll over Bel |
| Play / Pause | Click album artwork |
| Seek | Click around the progress ring |
| Previous Track | Left side button |
| Next Track | Right side button |

## How It Works

BelPet combines Spotify's Web API with the Web Playback SDK to act as a Spotify Connect playback device.

When BelPet is the active Spotify player, playback state and controls are handled directly through the Web Playback SDK.

When another Spotify device is active instead, BelPet switches to a polling system that periodically calls the Spotify Web API to retrieve the current track, playback progress, volume, and playback state. This allows BelPet's UI to stay mostly synchronized even when music is playing from another device.

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.
