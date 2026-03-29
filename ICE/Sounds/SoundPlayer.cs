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
        public static async Task PlaySoundAsync()
        {
            try
            {
                string sound = "ICE.Sounds.Task_Completed.mp3";
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(sound);

                if (stream == null)
                {
                    PluginLog.Warning($"Could not find embedded resource: {sound}");
                    return;
                }

                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
                memoryStream.Position = 0;

                using var reader = new Mp3FileReader(memoryStream);

                var sampleProvider = reader.ToSampleProvider();
                var volumeProvider = new VolumeSampleProvider(sampleProvider)
                {
                    Volume = C.SoundVolume
                };

                var waveOut = new WasapiOut(
                    AudioClientShareMode.Shared,
                    latency: 200
                );

                var tcs = new TaskCompletionSource<bool>();

                waveOut.PlaybackStopped += (sender, args) =>
                {
                    tcs.TrySetResult(true);
                    waveOut.Dispose();
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