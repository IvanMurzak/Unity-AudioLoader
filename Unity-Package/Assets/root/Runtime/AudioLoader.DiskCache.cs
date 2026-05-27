using System;
using System.IO;
using System.Threading.Tasks;

namespace Extensions.Unity.AudioLoader
{
    public static partial class AudioLoader
    {
        internal static readonly TaskFactory diskTaskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(1));
        private static string DiskCachePath(string url) => $"{settings.diskSaveLocation}/I_{url.GetHashCode()}";
        private static void SaveDisk(string url, byte[] data)
        {
            if (!settings.useDiskCache) return;
            Directory.CreateDirectory(settings.diskSaveLocation);
            Directory.CreateDirectory(Path.GetDirectoryName(DiskCachePath(url)));
            File.WriteAllBytes(DiskCachePath(url), data);
        }
        private static float[] LoadDisk(string url)
        {
            if (!settings.useDiskCache) return null;
            Directory.CreateDirectory(settings.diskSaveLocation);
            Directory.CreateDirectory(Path.GetDirectoryName(DiskCachePath(url)));
            if (!DiskCacheContains(url)) return null;
            return ConvertByteToFloat(File.ReadAllBytes(DiskCachePath(url)));
        }
        private static Task SaveDiskAsync(string url, byte[] data)
        {
            if (!settings.useDiskCache)
                return Task.CompletedTask;

            return diskTaskFactory.StartNew(() => SaveDisk(url, data));
        }
        private static Task<float[]> LoadDiskAsync(string url)
        {
            if (!settings.useDiskCache)
                return Task.FromResult<float[]>(null);

            return diskTaskFactory.StartNew(() => LoadDisk(url));
        }

        /// <summary>
        /// Check if the AudioClip is cached on disk
        /// </summary>
        /// <param name="url">URL to the audio file, web or local</param>
        /// <returns>Returns true if AudioClip is cached on disk</returns>
        public static bool DiskCacheContains(string url) => File.Exists(DiskCachePath(url));

        /// <summary>
        /// Check if the AudioClip is cached on disk
        /// </summary>
        /// <param name="url">URL to the audio file, web or local</param>
        /// <returns>Returns true if AudioClip is cached on disk</returns>
        public static Task<bool> DiskCacheExistsAsync(string url)
        {
            var path = DiskCachePath(url);
            return diskTaskFactory.StartNew(() => File.Exists(path));
        }

        /// <summary>
        /// Clear Disk cache for all urls
        /// </summary>
        public static Task ClearDiskCache()
        {
            return diskTaskFactory.StartNew(() =>
            {
                if (Directory.Exists(settings.diskSaveLocation))
                    Directory.Delete(settings.diskSaveLocation, true);
            });
        }

        /// <summary>
        /// Clear Disk cache for the given url
        /// </summary>
        /// <param name="url">URL to the audio file, web or local</param>
        public static Task ClearDiskCache(string url)
        {
            var diskPath = DiskCachePath(url);
            return diskTaskFactory.StartNew(() =>
            {
                if (!File.Exists(diskPath)) return;
                File.Delete(diskPath);
            });
        }

        private static float[] ConvertByteToFloat(byte[] array)
        {
            if ((array?.Length ?? 0) == 0) 
                return null;

            var floatArr = new float[array.Length / 4];
            for (var i = 0; i < floatArr.Length; i++)
            {
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(array, i * 4, 4);

                floatArr[i] = BitConverter.ToSingle(array, i * 4) / 0x80000000;
            }
            return floatArr;
        }
    }
}