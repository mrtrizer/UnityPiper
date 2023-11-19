using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abuksigun.Piper
{
    public class PiperSpeaker
    {
        AudioClip audioClip;

        readonly List<float[]> pcmBuffers = new List<float[]>();
        int pcmBufferPointer = 0;

        public AudioClip AudioClip => audioClip;

        ~PiperSpeaker()
        {
            if (audioClip)
                UnityEngine.Object.Destroy(audioClip);
        }

        public unsafe AudioClip Speak(PiperLib.PiperConfig* piperConfig, PiperLib.Voice* voice, string text)
        {
            pcmBuffers.Clear();
            pcmBufferPointer = 0;
            return ContinueSpeach(piperConfig, voice, text);
        }

        public unsafe AudioClip ContinueSpeach(PiperLib.PiperConfig* piperConfig, PiperLib.Voice* voice, string text)
        {
            PiperLib.SynthesisResult result = new PiperLib.SynthesisResult();
            PiperLib.SynthesisConfig synthesisConfig = PiperLib.getSynthesisConfig(voice);

            if (!audioClip)
                audioClip = AudioClip.Create("MyPCMClip", 1024 * 24, synthesisConfig.channels, synthesisConfig.sampleRate, true, PCMRead);

            PiperLib.textToAudio(piperConfig, voice, text, &result, (short* data, int length) => AddPCMData(data, length));
            return audioClip;
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

                Debug.Log($"dataIndex: {dataIndex}, pcmBufferPointer: {pcmBufferPointer}");
            }
        }

        public unsafe void AddPCMData(short* pcmData, int length)
        {
            float[] floatData = new float[length];
            for (int i = 0; i < length; i++)
            {
                floatData[i] = pcmData[i] / 32768.0f; // Convert int16 to float
            }
            pcmBuffers.Add(floatData);
        }
    }

}