using System;
using System.IO;
using UnityEngine;

namespace Abuksigun.Piper
{
    public unsafe class PiperExample : MonoBehaviour
    {
        [SerializeField] AudioSource audioSource;

        [SerializeField] string text = "One more lib and we are done";
        [SerializeField] string modelPath;
        [SerializeField] string espeakDataPath;

        PiperSpeaker piperSpeaker;

        [ContextMenu("Run")]
        void Run()
        {
            if (gameObject.scene.name == null)
                throw new InvalidOperationException("This script must be attached to a game object in a scene, otherwise AudioSource can't play");

            string fullModelPath = Path.Join(Application.streamingAssetsPath, modelPath).Replace("\\", "/");
            string fullEspeakDataPath = Path.Join(Application.streamingAssetsPath, espeakDataPath).Replace("\\", "/");

            if (!File.Exists(fullModelPath))
                throw new FileNotFoundException("Model file not found", fullModelPath);

            if (!File.Exists(fullModelPath + ".json"))
                throw new FileNotFoundException("Model descriptor not found (Make sure it has the same name as model + .json)", fullModelPath);

            if (!Directory.Exists(fullEspeakDataPath))
                throw new DirectoryNotFoundException("Espeak data directory not found");

            var piperConfig = PiperLib.create_PiperConfig(fullEspeakDataPath);
            PiperLib.initializePiper(piperConfig);
            try
            {
                var voice = PiperLib.create_Voice();
                try
                {
                    PiperLib.loadVoice(piperConfig, fullModelPath, fullModelPath + ".json", voice, null);
                    piperSpeaker ??= new PiperSpeaker();
                    var audioClip = piperSpeaker.ContinueSpeach(piperConfig, voice, text);
                    audioSource.clip = audioClip;
                    audioSource.loop = true;
                    audioSource.Play();
                    
                }
                finally
                {
                    PiperLib.destroy_Voice(voice);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                PiperLib.terminatePiper(piperConfig);
                PiperLib.destroy_PiperConfig(piperConfig);
                Debug.Log("Done!");
            }
        }
    }
}