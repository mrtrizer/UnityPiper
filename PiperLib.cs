using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Abuksigun.Piper
{
    public unsafe static class PiperLib
    {
        private const string DllName = "piperlib"; // Replace with the actual name of your DLL

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct PiperConfig
        {
            //public string eSpeakDataPath;
            //public bool useESpeak;
            //public bool useTashkeel;
            //public OptionalString tashkeelModelPath;
            //public IntPtr tashkeelState; // Assuming tashkeel::State is represented as a pointer/handle
        }

        public struct OptionalString
        {
            //public bool hasValue;
            //public string value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SynthesisConfig
        {
            public struct OptionalLong
            {
                public bool hasValue;
                public long value;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct OptionalPhonemeSilenceSecondsMap
            {
                public bool hasValue;
                public PhonemeSilenceSecondsMap* value;
            }

            public float noiseScale;
            public float lengthScale;
            public float noiseW;

            public int sampleRate;
            public int sampleWidth;
            public int channels;

            public OptionalLong speakerId;

            public float sentenceSilenceSeconds;
            public OptionalPhonemeSilenceSecondsMap phonemeSilenceSeconds;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct PhonemeSilenceSecondsMap
        {
            public int phoneme;
            public float silenceSeconds;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SynthesisResult
        {
            public double inferSeconds;
            public double audioSeconds;
            public double realTimeFactor;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Voice
        {
            //public IntPtr configRoot; // Handle for json
            //public PhonemizeConfig phonemizeConfig;
            //public SynthesisConfig synthesisConfig;
            //public ModelConfig modelConfig;
            //public ModelSession session;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AudioCallback();


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern PiperConfig* create_PiperConfig(string eSpeakDataPath);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void destroy_PiperConfig(PiperConfig* config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Voice* create_Voice();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void destroy_Voice(Voice* voice);



        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SynthesisConfig getSynthesisConfig(Voice* voice);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool isSingleCodepoint(string s);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern char getCodepoint(string s);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getVersion();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void initializePiper(PiperConfig* config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void terminatePiper(PiperConfig* config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void loadVoice(PiperConfig* config, string modelPath, string modelConfigPath, Voice* voice, Int64* speakerId);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AudioCallbackDelegate(short* audioBuffer, int length);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void textToAudio(PiperConfig* config, Voice* voice, string text, SynthesisResult* result, AudioCallbackDelegate audioCallback);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void textToWavFile(PiperConfig* config, Voice* voice, string text, string audioFile, SynthesisResult* result);
    }
}