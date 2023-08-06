using System.Collections.Generic;
using UnityEngine;

namespace Extensions.Unity.AudioLoader
{
    public static partial class AudioLoader
    {
        internal static Dictionary<string, AudioClip> memoryAudioClipCache = new Dictionary<string, AudioClip>();

        /// <summary>
        /// Check if the Memory cache contains AudioClip for the given url
        /// </summary>
        /// <param name="url">URL to the audio file, web or local</param>
        /// <returns>Returns true if AudioClip exists in Memory cache</returns>
        public static bool MemoryCacheContains(string url)
        {
            return memoryAudioClipCache.ContainsKey(url);
        }
        /// <summary>
        /// Save AudioClip to Memory cache directly. Should be used for overloading cache system
        /// </summary>
        /// <param name="url">URL to the audio file, web or local</param>
        /// <param name="audioClip">AudioClip which should be saved</param>
        /// <param name="replace">replace existed cached AudioClip if any</param>
        public static void SaveToMemoryCache(string url, AudioClip audioClip, bool replace = false)
        {
            if (!settings.useMemoryCache) return;
            if (!replace && memoryAudioClipCache.ContainsKey(url))
            {
                if (settings.debugLevel <= DebugLevel.Warning)
                    Debug.LogError($"[AudioLoader] Memory cache already contains key: {url}");
                return;
            }
            memoryAudioClipCache[url] = audioClip;
        }
        /// <summary>
        /// Loads directly from Memory cache if exists and allowed
        /// </summary>
        /// <param name="url">URL to the audio file, web or local</param>
        /// <returns>Returns null if not allowed to use Memory cache or if there is no cached AudioClip</returns>
        public static AudioClip LoadFromMemoryCache(string url)
        {
            if (!settings.useMemoryCache) return null;
            return memoryAudioClipCache.GetValueOrDefault(url);
        }
        /// <summary>
        /// Clear Memory cache for the given url
        /// </summary>
        /// <param name="url">URL to the audio file, web or local</param>
        public static void ClearMemoryCache(string url)
        {
            var cache = memoryAudioClipCache.GetValueOrDefault(url);
            UnityEngine.Object.DestroyImmediate(cache);

            memoryAudioClipCache.Remove(url);
        }
        /// <summary>
        /// Clear Memory cache for all urls
        /// </summary>
        /// <param name="url">URL to the audio file, web or local</param>
        public static void ClearMemoryCache()
        {
            foreach (var cache in memoryAudioClipCache.Values)
                UnityEngine.Object.DestroyImmediate(cache);

            memoryAudioClipCache.Clear();
        }
    }
}