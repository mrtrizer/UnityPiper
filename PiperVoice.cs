using System;
using System.IO;
using System.Threading.Tasks;

namespace Abuksigun.Piper
{
    public sealed unsafe class PiperVoice : IDisposable
    {
        Piper piper;
        PiperLib.Voice* voice;

        public Piper Piper => piper;
        internal PiperLib.Voice* Voice => voice;

        PiperVoice(Piper piper, PiperLib.Voice* voice)
        {
            this.piper = piper;
            this.voice = voice;
        }

        ~PiperVoice()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (voice != null)
            {
                PiperLib.destroy_Voice(voice);
                voice = null;
            }
        }

        public static Task<PiperVoice> LoadPiperVoice(Piper piper, string fullModelPath)
        {
            if (!File.Exists(fullModelPath))
                throw new FileNotFoundException("Model file not found", fullModelPath);
            if (!File.Exists(fullModelPath + ".json"))
                throw new FileNotFoundException("Model descriptor not found (Make sure it has the same name as model + .json)", fullModelPath);

            return Task.Run(() =>
            {
                var newVoice = PiperLib.create_Voice();
                try
                {
                    PiperLib.loadVoice(piper.Config, fullModelPath, fullModelPath + ".json", newVoice, null);
                    return Task.FromResult(new PiperVoice(piper, newVoice));
                }
                catch
                {
                    PiperLib.destroy_Voice(newVoice);
                    throw;
                }
            });
        }

        public float[] TextToPCMAudio(string text)
        {
            float[] audioData = new float[0];
            TextToAudioStream(text, (short* data, int length) =>
            {
                int writeIndex = audioData.Length;
                Array.Resize(ref audioData, audioData.Length + length);
                for (int i = writeIndex; i < audioData.Length; i++)
                    audioData[i] = data[i] / 32768f;
            });

            return audioData;
        }

        public void TextToAudioStream(string text, PiperLib.AudioCallbackDelegate audioCallback)
        {
            PiperLib.SynthesisResult result = new PiperLib.SynthesisResult();
            PiperLib.textToAudio(piper.Config, voice, text, &result, audioCallback);
        }
    }
}
