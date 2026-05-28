using System;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Extensions.Unity.AudioLoader
{
    public static partial class AudioLoader
    {
        private static HashSet<string> loadingInProcess = new HashSet<string>();
        private static void AddLoading(string url) => loadingInProcess.Add(url);
        private static void RemoveLoading(string url) => loadingInProcess.Remove(url);

        /// <summary>
        /// Initialization of static variables, should be called from main thread at project start
        /// </summary>
        public static void Init()
        {
            // need get SaveLocation variable in runtime from thread to setup the default static value into it
            var temp = settings.diskSaveLocation + settings.diskSaveLocation;
        }

        /// <summary>
        /// Check if the url is loading right now
        /// </summary>
        /// <returns>Returns true if the url is loading right now</returns>
        public static bool IsLoading(string url) => loadingInProcess.Contains(url);

        /// <summary>
        /// Clear cache from Memory and Disk layers for all urls
        /// </summary>
        public static Task ClearCache()
        {
            ClearMemoryCache();
            return ClearDiskCache();
        }

        /// <summary>
        /// Clear cache from Memory and Disk layers for all urls
        /// </summary>
        /// <param name="url">URL to the audio file, web or local</param>
        /// <returns>Returns AudioClip</returns>
        public static bool CacheContains(string url) => MemoryCacheContains(url) || DiskCacheContains(url);

        /// <summary>
        /// Load audio file from web or local path and return it as AudioClip
        /// </summary>
        /// <param name="url">URL to the audio file, web or local</param>
        /// <param name="ignoreAudioFileNotFoundError">Ignore error if the audio file was not found by specified url</param>
        /// <returns>Returns AudioClip asynchronously </returns>
        public static async UniTask<AudioClip> LoadAudioClip(string url, bool ignoreAudioFileNotFoundError = false)
        {
            if (string.IsNullOrEmpty(url))
            {
                if (settings.debugLevel <= DebugLevel.Error)
                    Debug.LogError($"[AudioLoader] Empty url. Audio file could not be loaded!");
                return null;
            }

            if (MemoryCacheContains(url))
            {
                var audioClip = LoadFromMemoryCache(url);
                if (audioClip != null)
                    return audioClip;
            }

            if (IsLoading(url))
            {
                if (settings.debugLevel <= DebugLevel.Log)
                    Debug.Log($"[AudioLoader] Waiting while another task is loading the AudioClip url={url}");
                await UniTask.WaitWhile(() => IsLoading(url));
                return await LoadAudioClip(url, ignoreAudioFileNotFoundError);
            }

            AddLoading(url);

            if (settings.debugLevel <= DebugLevel.Log)
                Debug.Log($"[AudioLoader] Loading new AudioClip into memory url={url}");
            try
            {
                var cachedAudioFile = await LoadDiskAsync(url);
                if (cachedAudioFile != null && cachedAudioFile.Length > 0)
                {
                    await UniTask.SwitchToMainThread();
                    var audioClip = AudioClip.Create("CachedAudioClip", cachedAudioFile.Length * 4, 1, 48000, false);
                    audioClip.SetData(cachedAudioFile, 0);
                    SaveToMemoryCache(url, audioClip, replace: true);
                    RemoveLoading(url);
                    return audioClip;
                }
            }
            catch (Exception e)
            {
                if (settings.debugLevel <= DebugLevel.Exception)
                    Debug.LogException(e);
            }

            UnityWebRequest request = null;
            var finished = false;
            UniTask.Post(async () =>
            {
                try
                {
                    request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
                    await request.SendWebRequest();
                }
                catch (Exception e) 
                {
                    if (!ignoreAudioFileNotFoundError)
                        if (settings.debugLevel <= DebugLevel.Exception)
                            Debug.LogException(e);
                }
                finally
                {
                    finished = true;
                }
            });
            await UniTask.WaitUntil(() => finished);

            RemoveLoading(url);

#if UNITY_2020_1_OR_NEWER
            var isError = request.result != UnityWebRequest.Result.Success;
#else
            var isError = request.isNetworkError || request.isHttpError;
#endif

            if (isError)
            {
                if (settings.debugLevel <= DebugLevel.Error)
#if UNITY_2020_1_OR_NEWER
                    Debug.LogError($"[AudioLoader] {request.result} {request.error}: url={url}");
#else
                    Debug.LogError($"[AudioLoader] {request.error}: url={url}");
#endif
                return null;
            }
            else
            {
                await SaveDiskAsync(url, request.downloadHandler.data);
                var audioClip = ((DownloadHandlerAudioClip)request.downloadHandler).audioClip;
                SaveToMemoryCache(url, audioClip, replace: true);
                return audioClip;
            }
        }
    }
}