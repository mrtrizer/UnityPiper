using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Abuksigun.Piper
{
    public class PiperSpeaker
    {
        readonly AudioClip audioClip;
        readonly PiperVoice voice;

        readonly List<float[]> pcmBuffers = new List<float[]>();
        volatile int pcmBufferPointer = 0;
        volatile string queuedText = null;
        Task speachTask = null;

        public AudioClip AudioClip => audioClip;

        public unsafe PiperSpeaker(PiperVoice voice)
        {
            this.voice = voice;
            PiperLib.SynthesisConfig synthesisConfig = PiperLib.getSynthesisConfig(voice.Voice);
            audioClip = AudioClip.Create("MyPCMClip", 1024 * 24, synthesisConfig.channels, synthesisConfig.sampleRate, true, PCMRead);
        }

        ~PiperSpeaker()
        {
            if (audioClip)
                UnityEngine.Object.Destroy(audioClip);
        }

        // Use when you want to interrupt the current speech and say new replica
        public Task Speak(string text)
        {
            pcmBufferPointer = 0;
            return OverrideSpeech(text);
        }

        // Use when you are streaming generating text, so you can override audiostream seamlessly while it's playing
        // For example, LLM generates first 3 tokens "I'm going to", you can start playing them before generation ends
        // And then override with "I'm going to school" while it's playing.
        // This way you can minimize latency between generation and playback
        public Task OverrideSpeech(string text)
        {
            lock (pcmBuffers)
                pcmBuffers.Clear();
            return ContinueSpeach(text);
        }

        // Use when you want to add more text to the current speech
        public unsafe Task ContinueSpeach(string text)
        {
            if (speachTask == null || speachTask.IsCompleted)
            {
                speachTask = Task.Run(() =>
                {
                    do
                    {
                        voice.TextToAudioStream(text, (short* data, int length) => AddPCMData(data, length));
                        text = queuedText;
                        queuedText = null;
                    }
                    while (text != null);
                });
            }
            else
            {
                queuedText = text;
            }
            return speachTask;
        }

        void PCMRead(float[] data)
        {
            if (pcmBuffers.Count == 0)
            {
                Array.Fill(data, 0);
                return;
            }

            int dataLength = data.Length;
            int dataIndex = 0;

            while (dataIndex < dataLength)
            {
                int bufferIndex = 0;
                int bufferOffset = pcmBufferPointer;

                lock (pcmBuffers)
                {
                    while (bufferIndex < pcmBuffers.Count && bufferOffset >= pcmBuffers[bufferIndex].Length)
                    {
                        bufferOffset -= pcmBuffers[bufferIndex].Length;
                        bufferIndex++;
                    }

                    if (bufferIndex < pcmBuffers.Count)
                    {
                        float[] currentBuffer = pcmBuffers[bufferIndex];
                        int remainingInBuffer = currentBuffer.Length - bufferOffset;
                        int remainingInData = dataLength - dataIndex;
                        int copyLength = Mathf.Min(remainingInBuffer, remainingInData);

                        Array.Copy(currentBuffer, bufferOffset, data, dataIndex, copyLength);

                        dataIndex += copyLength;
                        pcmBufferPointer += copyLength;
                    }
                    else
                    {
                        Array.Fill(data, 0, dataIndex, data.Length - dataIndex);
                        break;
                    }
                }
            }
        }

        public unsafe void AddPCMData(short* pcmData, int length)
        {
            float[] floatData = new float[length];
            for (int i = 0; i < length; i++)
                floatData[i] = pcmData[i] / 32768.0f;
            lock (pcmBuffers)
                pcmBuffers.Add(floatData);
        }
    }

}