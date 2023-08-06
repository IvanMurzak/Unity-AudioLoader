using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;

namespace Extensions.Unity.AudioLoader.Tests
{
    public class TestCache
    {
        static readonly string[] AudioURLs =
        {
            "https://github.com/IvanMurzak/Unity-AudioLoader/raw/master/Test%20Audio%20Files/sample.aiff",
            "https://github.com/IvanMurzak/Unity-AudioLoader/raw/master/Test%20Audio%20Files/sample.mp3",
            "https://github.com/IvanMurzak/Unity-AudioLoader/raw/master/Test%20Audio%20Files/sample.wav"
        };

        public async UniTask LoadAudioClip(string url)
        {
            var AudioClip = await AudioLoader.LoadAudioClip(url);
            Assert.IsNotNull(AudioClip);
        }

        [UnityTest] public IEnumerator LoadingFromMemoryCache()
        {
            yield return AudioLoader.ClearCache().AsUniTask().ToCoroutine();
            AudioLoader.settings.useMemoryCache = true;
            AudioLoader.settings.useDiskCache = false;

            foreach (var imageURL in AudioURLs)
            {
                yield return LoadAudioClip(imageURL).ToCoroutine();
                Assert.IsTrue(AudioLoader.MemoryCacheContains(imageURL));
                yield return LoadAudioClip(imageURL).ToCoroutine();
                Assert.IsTrue(AudioLoader.MemoryCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator LoadingFromDiskCache()
        {
            yield return AudioLoader.ClearCache().AsUniTask().ToCoroutine();
            AudioLoader.settings.useMemoryCache = false;
            AudioLoader.settings.useDiskCache = true;

            foreach (var imageURL in AudioURLs)
            {
                yield return LoadAudioClip(imageURL).ToCoroutine();
                Assert.IsTrue(AudioLoader.DiskCacheContains(imageURL));
                yield return LoadAudioClip(imageURL).ToCoroutine();
                Assert.IsTrue(AudioLoader.DiskCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator DiskCacheEnable()
        {
            yield return AudioLoader.ClearCache().AsUniTask().ToCoroutine();
            AudioLoader.settings.useDiskCache = true;

            foreach (var imageURL in AudioURLs)
            {
                yield return LoadAudioClip(imageURL).ToCoroutine();
                Assert.IsTrue(AudioLoader.DiskCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator DiskCacheDisable()
        {
            yield return AudioLoader.ClearCache().AsUniTask().ToCoroutine();
            AudioLoader.settings.useDiskCache = false;

            foreach (var imageURL in AudioURLs)
            {
                yield return LoadAudioClip(imageURL).ToCoroutine();
                Assert.IsFalse(AudioLoader.DiskCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator MemoryCacheEnabled()
        {
            yield return AudioLoader.ClearCache().AsUniTask().ToCoroutine();
            AudioLoader.settings.useMemoryCache = true;

            foreach (var imageURL in AudioURLs)
            {
                yield return LoadAudioClip(imageURL).ToCoroutine();
                Assert.IsTrue(AudioLoader.MemoryCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator MemoryCacheDisabled()
        {
            yield return AudioLoader.ClearCache().AsUniTask().ToCoroutine();
            AudioLoader.settings.useMemoryCache = false;

            foreach (var imageURL in AudioURLs)
            {
                yield return LoadAudioClip(imageURL).ToCoroutine();
                Assert.IsFalse(AudioLoader.MemoryCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator ClearDiskCache()
        {
            yield return AudioLoader.ClearCache().AsUniTask().ToCoroutine();
            AudioLoader.settings.useDiskCache = true;

            foreach (var imageURL in AudioURLs)
            {
                yield return LoadAudioClip(imageURL).ToCoroutine();
                Assert.IsTrue(AudioLoader.DiskCacheContains(imageURL));
                yield return AudioLoader.ClearDiskCache().AsUniTask().ToCoroutine();
                Assert.IsFalse(AudioLoader.DiskCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator ClearMemoryCache()
        {
            yield return AudioLoader.ClearCache().AsUniTask().ToCoroutine();
            AudioLoader.settings.useMemoryCache = true;

            foreach (var imageURL in AudioURLs)
            {
                yield return LoadAudioClip(imageURL).ToCoroutine();
                Assert.IsTrue(AudioLoader.MemoryCacheContains(imageURL));
                AudioLoader.ClearMemoryCache(imageURL);
                Assert.IsFalse(AudioLoader.MemoryCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator ClearDiskCacheAll()
        {
            yield return AudioLoader.ClearCache().AsUniTask().ToCoroutine();
            AudioLoader.settings.useDiskCache = true;

            foreach (var imageURL in AudioURLs)
            {
                yield return LoadAudioClip(imageURL).ToCoroutine();
                Assert.IsTrue(AudioLoader.DiskCacheContains(imageURL));
            }
            yield return AudioLoader.ClearDiskCache().AsUniTask().ToCoroutine();
            foreach (var imageURL in AudioURLs)
                Assert.IsFalse(AudioLoader.DiskCacheContains(imageURL));
        }
        [UnityTest] public IEnumerator ClearMemoryCacheAll()
        {
            yield return AudioLoader.ClearCache().AsUniTask().ToCoroutine();
            AudioLoader.settings.useMemoryCache = true;

            foreach (var imageURL in AudioURLs)
            {
                yield return LoadAudioClip(imageURL).ToCoroutine();
                Assert.IsTrue(AudioLoader.MemoryCacheContains(imageURL));
            }
            AudioLoader.ClearMemoryCache();
            foreach (var imageURL in AudioURLs)
                Assert.IsFalse(AudioLoader.MemoryCacheContains(imageURL));
        }
    }
}