using System;
using System.IO;
using UnityEngine;

namespace Abuksigun.Piper
{
    public class PiperExample : MonoBehaviour
    {
        [SerializeField] AudioSource audioSource;

        [SerializeField] string text = "One more lib and we are done";
        [SerializeField] string modelPath;
        [SerializeField] string espeakDataPath;

        Piper piper;
        PiperVoice voice;
        PiperSpeaker piperSpeaker;

        [ContextMenu("Run Stream")]
        async void Run()
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

            piper ??= await Piper.LoadPiper(fullEspeakDataPath);
            try
            {
                voice ??= await PiperVoice.LoadPiperVoice(piper, fullModelPath);
                piperSpeaker ??= new PiperSpeaker(voice);
                piperSpeaker.ContinueSpeach(text);
                audioSource.clip = piperSpeaker.AudioClip;
                audioSource.loop = true;
                audioSource.Play();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}