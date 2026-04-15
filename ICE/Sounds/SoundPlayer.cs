using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ICE.Sounds
{
    public static class SoundPlayer
    {
        private static byte[]? _soundData;
        private static WasapiOut? _waveOut;
        private static bool _initialized = false;

        // Call this once during plugin init, off the main thread
        public static async Task InitializeAsync()
        {
            if (_initialized) return;

            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ICE.Sounds.Task_Completed.mp3")!;
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms).ConfigureAwait(false);
            _soundData = ms.ToArray();

            using var probe = new WasapiOut(AudioClientShareMode.Shared, latency: 200);
            using var probeReader = new Mp3FileReader(new MemoryStream(_soundData));
            probe.Init(probeReader.ToSampleProvider());

            _initialized = true;
        }

        public static async Task PlaySoundAsync()
        {
            if (_soundData == null) return;
            try
            {
                var memoryStream = new MemoryStream(_soundData, writable: false);
                var reader = new Mp3FileReader(memoryStream);
                var volumeProvider = new VolumeSampleProvider(reader.ToSampleProvider())
                {
                    Volume = C.SoundVolume
                };

                var waveOut = new WasapiOut(AudioClientShareMode.Shared, latency: 200);
                var tcs = new TaskCompletionSource<bool>();

                waveOut.PlaybackStopped += (_, _) =>
                {
                    tcs.TrySetResult(true);
                    waveOut.Dispose();
                    reader.Dispose();
                    memoryStream.Dispose();
                };

                waveOut.Init(volumeProvider);
                waveOut.Play();

                await tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to play sound: {ex}");
            }
        }
    }
}