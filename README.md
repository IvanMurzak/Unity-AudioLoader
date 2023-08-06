# Unity Audio Loader

![npm](https://img.shields.io/npm/v/extensions.unity.audioloader) [![openupm](https://img.shields.io/npm/v/extensions.unity.audioloader?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/extensions.unity.audioloader/) ![License](https://img.shields.io/github/license/IvanMurzak/Unity-AudioLoader) [![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/badges/StandWithUkraine.svg)](https://stand-with-ukraine.pp.ua)

Async audio loader with two caching layers for Unity.

## Features

- ✔️ Async loading from **Web** or **Local** `AudioLoader.LoadAudioClip(audioURL);`
- ✔️ **Memory** and **Disk** caching - tries to load from memory first, then from disk
- ✔️ Dedicated thread for disk operations
- ✔️ Avoids loading same audio multiple times simultaneously, task waits for completion the first and just returns loaded audio if at least one cache layer activated
- ✔️ Auto set to AudioSource `AudioLoader.SetAudioSource(audioURL, audioSource);`
- ✔️ Debug level for logging `AudioLoader.settings.debugLevel = DebugLevel.Error;`

# Usage

In main thread somewhere at start of the project need to call `AudioLoader.Init();` once to initialize static properties in right thread. It is required to make in main thread. Then you can use `AudioLoader` from any thread and any time.

## Sample - Loading audio file, set to AudioSource

``` C#
using Extensions.Unity.AudioLoader;
using Cysharp.Threading.Tasks;

public class AudioLoaderSample : MonoBehaviour
{
    [SerializeField] string audioURL;
    [SerializeField] AudioSource audioSource;

    async void Start()
    {
        // Loading audio file from web, cached for quick load next time
        audioSource.clip = await AudioLoader.LoadAudioClip(audioURL);

        // Same loading with auto set to audio
        await AudioLoader.SetAudioSource(audioURL, audioSource);
    }
}
```

## Sample - Loading audio into multiple Audio components

``` C#
using Extensions.Unity.AudioLoader;
using Cysharp.Threading.Tasks;

public class AudioLoaderSample : MonoBehaviour
{
    [SerializeField] string audioURL;
    [SerializeField] AudioSource audioSource1;
    [SerializeField] AudioSource audioSource2;

    void Start()
    {
        // Loading with auto set to audio
        AudioLoader.SetAudioSource(audioURL, audioSource1, audioSource2).Forget();
    }
}
```

# Cache

Cache system based on the two layers. First layer is **memory cache**, second is **disk cache**. Each layer could be enabled or disabled. Could be used without caching at all. By default both layers are enabled.

## Setup Cache

- `AudioLoader.settings.useMemoryCache = true;` default value is `true`
- `AudioLoader.settings.useDiskCache = true;` default value is `true`
  
Change disk cache folder:

``` C#
AudioLoader.settings.diskSaveLocation = Application.persistentDataPath + "/myCustomFolder";
```

## Override Cache

``` C#
// Override Memory cache for specific audio
AudioLoader.SaveToMemoryCache(url, audioClip);

// Take from Memory cache for specific audio file if exists
AudioLoader.LoadFromMemoryCache(url);
```

## Does Cache contain audio

``` C#
// Check if any cache contains specific audio file
AudioLoader.CacheContains(url);

// Check if Memory cache contains specific audio file
AudioLoader.MemoryCacheContains(url);

// Check if Memory cache contains specific audio file
AudioLoader.DiskCacheContains(url);
```

## Clear Cache

``` C#
// Clear memory Memory and Disk cache
AudioLoader.ClearCache();

// Clear only Memory cache for all audio files
AudioLoader.ClearMemoryCache();

// Clear only Memory cache for specific audio file
AudioLoader.ClearMemoryCache(url);

// Clear only Disk cache for all audio files
AudioLoader.ClearDiskCache();

// Clear only Disk cache for specific audio file
AudioLoader.ClearDiskCache(url);
```

# Installation

- [Install OpenUPM-CLI](https://github.com/openupm/openupm-cli#installation)
- Open command line in Unity project folder
- Run the command

``` CLI
openupm add extensions.unity.audioloader
```
