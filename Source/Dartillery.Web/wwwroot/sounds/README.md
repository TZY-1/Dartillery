# Audio Files for Dartillery

This directory contains sound effects for the dart simulation.

## Required Files

The application expects the following audio files:

1. **throw.mp3** - Sound played when a dart is thrown
   - Duration: ~200ms
   - Suggested sound: Dart swish/whoosh sound
   - Volume: Will be played at 30% volume

2. **impact.mp3** - Sound played when a dart hits the dartboard
   - Duration: ~150ms
   - Suggested sound: Dart hitting board (thud/thump sound)
   - Volume: Will be played at 30% volume

## Where to Get Sound Effects

You can find free dart sound effects from:

- **Freesound.org** - https://freesound.org (search for "dart throw" and "dart impact")
- **Zapsplat.com** - https://www.zapsplat.com (search for "dart" in the game sounds section)
- **Mixkit.co** - https://mixkit.co/free-sound-effects (search for "whoosh" and "impact")

## Adding the Files

1. Download the sound effects as MP3 files
2. Rename them to `throw.mp3` and `impact.mp3`
3. Place them in this directory (`wwwroot/sounds/`)
4. The application will automatically use them when manual targeting is enabled

## Fallback Behavior

If the audio files are not present, the application will:
- Attempt to play the sounds
- Log a warning to the browser console if playback fails
- Continue to function normally without sound

This allows the application to work even without audio files, but the user experience is enhanced when they are present.

## Technical Details

- Audio playback is handled via JavaScript (`wwwroot/js/dartboard.js`)
- Volume is set to 30% to avoid being too loud
- There's a 300ms delay between throw and impact sounds to simulate dart flight time
- Sounds are triggered from `DartboardVisualizer.razor.cs` when manual targeting is used
