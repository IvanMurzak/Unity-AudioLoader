using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Extensions.Unity.AudioLoader
{
    public static partial class AudioLoader
    {
        /// <summary>
        /// Load audio file from URL and set it to the AudioSource component
        /// </summary>
        /// <param name="url">URL to the audio file, web or local</param>
        /// <param name="audioSource">AudioSource component from Unity UI</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the audio file was not found by specified url</param>
        /// <returns>Returns async task</returns>
        public static async UniTask SetAudioSource(string url, AudioSource audioSource, bool ignoreImageNotFoundError = false)
        {
            try
            {
                if (audioSource == null || audioSource.gameObject == null)
                    return;

                var audioClip = await LoadAudioClip(url, ignoreImageNotFoundError);
                UniTask.Post(() =>
                {
                    if (audioSource == null || GameObject.Equals(audioSource.gameObject, null))
                        return;
                    try
                    {
                        audioSource.clip = audioClip;
                    }
                    catch (Exception e)
                    {
                        if (settings.debugLevel <= DebugLevel.Exception)
                            Debug.LogException(e); 
                    }
                });
            }
            catch (Exception e) 
            { 
                if (settings.debugLevel <= DebugLevel.Exception)
                    Debug.LogException(e); 
            }
        }

        /// <summary>
        /// Load audio file from URL and set it to the AudioSource components
        /// </summary>
        /// <param name="url">URL to the audio file, web or local</param>
        /// <param name="audioSources">Array of AudioSource components from Unity UI</param>
        /// <returns>Returns async task</returns>
        public static UniTask SetAudioSource(string url, params AudioSource[] audioSources)
            => SetAudioSource(url, false, audioSources);

        /// <summary>
        /// Load audio file from URL and set it to the AudioSource components
        /// </summary>
        /// <param name="url">URL to the audio file, web or local</param>
        /// <param name="ignoreAudioFileNotFoundError">Ignore error if the audio file was not found by specified url</param>
        /// <param name="audioSources">Array of AudioSource components from Unity UI</param>
        /// <returns>Returns async task</returns>
        public static async UniTask SetAudioSource(string url, bool ignoreAudioFileNotFoundError = false, params AudioSource[] audioSources)
        {
            if (audioSources == null)
                return;

            var audioClip = await LoadAudioClip(url, ignoreAudioFileNotFoundError);
            UniTask.Post(() =>
            {
                for (var i = 0; i < audioSources.Length; i++)
                {
                    try
                    {
                        if (audioSources[i] == null || GameObject.Equals((object)audioSources[i].gameObject, null))
                            continue;

                        audioSources[i].clip = audioClip;
                    }
                    catch (Exception e) 
                    {
                        if (settings.debugLevel <= DebugLevel.Exception)
                            Debug.LogException(e); 
                    }
                }
            });
        }
    }
}