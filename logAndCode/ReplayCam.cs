/* 
*   NatCorder
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatCorderU.Examples
{

    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;
    using Core;
    using System.IO;

    public class ReplayCam : MonoBehaviour
    {

        public static ReplayCam Instance;

        private Camera Mcamera;

        private AudioListener audioListener;
        private bool recordMicrophoneAudio = false;
        private AudioSource audioSource;

        /**
        * ReplayCam Example
        * -----------------
        * This example records the screen using the high-level `Replay` API
        * We simply call `Replay.StartRecording` to start recording, and `Replay.StopRecording` to stop recording
        * When we want mic audio, we play the mic to an AudioSource and pass the audio source to `Replay.StartRecording`
        * -----------------
        * Note that UI canvases in Overlay mode cannot be recorded, so we use a different mode (this is a Unity issue)
        */
        private void Awake()
        {
            Instance = this;
            Mcamera = GetComponent<Camera>();
            audioListener = GetComponent<AudioListener>();
        }

        public void StartRecording()
        {
            if (audioSource == null)
                audioSource = GameObject.FindObjectOfType<MicrophoneManager>().GetComponent<AudioSource>();
            // Create a recording configuration
            const float DownscaleFactor = 1;
            var configuration = new Configuration((int)(Screen.width * DownscaleFactor), (int)(Screen.height * DownscaleFactor));
            // Start recording with microphone audio
            //if (recordMicrophoneAudio) {
            //    StartMicrophone();
            //    Replay.StartRecording(Camera.main, configuration, OnReplay, audioSource, true);
            //}
            //// Start recording without microphone audio
            //else

            if (audioSource != null)
                Replay.StartRecording(Mcamera, configuration, OnReplay, audioSource);
            else
                Replay.StartRecording(Mcamera, configuration, OnReplay);
          
        }

        private void StartMicrophone()
        {
#if !UNITY_WEBGL || UNITY_EDITOR // No `Microphone` API on WebGL :(
            // If the clip has not been set, set it now
            if (audioSource.clip == null)
            {
                audioSource.clip = Microphone.Start(null, true, 60, 48000);
                while (Microphone.GetPosition(null) <= 0) ;
            }
            // Play through audio source
            audioSource.timeSamples = Microphone.GetPosition(null);
            audioSource.loop = true;
            audioSource.Play();
#endif
        }

        public void StopRecording()
        {
            if (recordMicrophoneAudio) audioSource.Stop();
            Replay.StopRecording();
            Resources.UnloadUnusedAssets();
            foreach (var item in GameManager.Instance.ActorDic.Values)
            {
                item.SetActive(true);
            }
        }

        public void PauseRecording()
        {
            Replay.PauseRecording();
        }

        public void ResumeRecording()
        {
            Replay.ResumeRecording();
        }

        void OnReplay(string path)
        {
            // Debug.Log("save recording to: " + path);
            // Playback the video
            //#if UNITY_IOS
            //Handheld.PlayFullScreenMovie("file://" + path);
#if UNITY_ANDROID && !UNITY_EDITOR
            //Handheld.PlayFullScreenMovie(path);
            if(null!= UnityCallAndroidApi.Instance.getSaveDirectory())
            {
                string newPath = UnityCallAndroidApi.Instance.getSaveDirectory() + Path.GetFileName(path);
                File.Move(path, newPath);
                UnityCallAndroidApi.Instance.RefulshPhotos(newPath);
            }
#endif
        }

    }
}