using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;

namespace Extensions.Unity.AudioLoader.Tests
{
    public class TestLoading
    {
        static readonly string[] AudioURLs =
        {
            "https://github.com/IvanMurzak/Unity-AudioLoader/raw/master/Test%20Audio%20Files/sample.aiff",
            "https://github.com/IvanMurzak/Unity-AudioLoader/raw/master/Test%20Audio%20Files/sample.mp3",
            "https://github.com/IvanMurzak/Unity-AudioLoader/raw/master/Test%20Audio%20Files/sample.wav"
        };

        public async UniTask LoadAudioClip(string url)
        {
            var audioClip = await AudioLoader.LoadAudioClip(url);
            Assert.IsNotNull(audioClip);
        }

        [UnityTest] public IEnumerator LoadAudioClipCacheMemoryDisk()
        {
            yield return AudioLoader.ClearCache().AsUniTask().ToCoroutine();
            AudioLoader.settings.useDiskCache = true;
            AudioLoader.settings.useMemoryCache = true;

            foreach (var imageURL in AudioURLs) 
                yield return LoadAudioClip(imageURL).ToCoroutine();
        }
        [UnityTest] public IEnumerator LoadAudioClipsCacheMemory()
        {
            yield return AudioLoader.ClearCache().AsUniTask().ToCoroutine();
            AudioLoader.settings.useDiskCache = false;
            AudioLoader.settings.useMemoryCache = true;

            foreach (var imageURL in AudioURLs) 
                yield return LoadAudioClip(imageURL).ToCoroutine();
        }
        [UnityTest] public IEnumerator LoadAudioClipsCacheDisk()
        {
            yield return AudioLoader.ClearCache().AsUniTask().ToCoroutine();
            AudioLoader.settings.useDiskCache = true;
            AudioLoader.settings.useMemoryCache = false;

            foreach (var imageURL in AudioURLs) 
                yield return LoadAudioClip(imageURL).ToCoroutine();
        }
        [UnityTest] public IEnumerator LoadAudioClipsNoCache()
        {
            yield return AudioLoader.ClearCache().AsUniTask().ToCoroutine();
            AudioLoader.settings.useDiskCache = false;
            AudioLoader.settings.useMemoryCache = false;

            foreach (var imageURL in AudioURLs) 
                yield return LoadAudioClip(imageURL).ToCoroutine();
        }
    }
}