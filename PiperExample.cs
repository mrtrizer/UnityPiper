using System;
using System.IO;
using System.Threading.Tasks;
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
                throw new InvalidOperationException("This script must be attached to a game object in a scene, otherwise AudioSource can't play :(");

            string fullModelPath = Path.Join(Application.streamingAssetsPath, modelPath);
            string fullEspeakDataPath = Path.Join(Application.streamingAssetsPath, espeakDataPath);

            piper ??= await Piper.LoadPiper(fullEspeakDataPath);
            voice ??= await PiperVoice.LoadPiperVoice(piper, fullModelPath);
            piperSpeaker ??= new PiperSpeaker(voice);
            _ = piperSpeaker.ContinueSpeach(text).ContinueWith(x => Debug.Log($"Generation finished with status: {x.Status}"));
            audioSource.clip = piperSpeaker.AudioClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}