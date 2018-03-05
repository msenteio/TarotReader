using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;


namespace HoloPlaySDK
{
    public class HoloPlayLauncher : MonoBehaviour
    {
        static HoloPlayLauncher instance;
        public static HoloPlayLauncher Instance
        {
            get
            {
                //lazy singleton
                if (instance == null)
                {
                    instance = FindObjectOfType<HoloPlayLauncher>();
                    if (instance == null)
                    {
                        GameObject launcherGO;
                        if (HoloPlay.Main != null)
                            launcherGO = HoloPlay.Main.gameObject;
                        else
                            launcherGO = new GameObject("HoloPlay Launcher");
                        instance = launcherGO.AddComponent<HoloPlayLauncher>();
                    }
                }
                return instance;
            }
        }
        Action guiAction;
        public bool currentlyQuittingToLauncher { get; private set; }
        float countdown;
        public static readonly string quitToLauncherArg = "holoplayQuitToLauncher";

        void OnEnable()
        {
            if (instance != null)
            {
                Destroy(this);
                Debug.Log("can't have multiple Launcher, deleting one");
            }
        }

        void Update()
        {
            // clear guiaction
            guiAction = null;

            if (currentlyQuittingToLauncher && HoloPlayButton.GetButtonDown(HP_Button.HOME))
            {
                currentlyQuittingToLauncher = false;
            }
        }

        public void StartLoadingApp(string appFileName)
        {
            StopAllCoroutines();
            StartCoroutine(StartCountdown(appFileName));
        }

        public void CancelLoadingApp()
        {
            currentlyQuittingToLauncher = false;
        }

        void LoadApp(string appFileName)
        {
            string extension = Application.platform == RuntimePlatform.WindowsPlayer ? ".exe" : ".app";
            string fullPath = Path.Combine(Path.GetFullPath("."), appFileName + extension);

            if (SceneManager.GetActiveScene().name == "HoloPlayLauncher")
            {

                if (Application.platform == RuntimePlatform.WindowsPlayer)
                    System.Diagnostics.Process.Start(fullPath, quitToLauncherArg);
                else
                    System.Diagnostics.Process.Start(fullPath, "--args " + quitToLauncherArg);
            }
            else
            {
                System.Diagnostics.Process.Start(fullPath);
            }
        }

        IEnumerator StartCountdown(string appFileName)
        {
            currentlyQuittingToLauncher = true;

            //disable depth plugin
            var depthPlugin = FindObjectOfType<depthPlugin>();

            //if depth plugin exists, save it's state incase the user cancels quitting
            bool previousDepthStatus = true;
            if (depthPlugin != null)
            {
                previousDepthStatus = depthPlugin.enabled;
                depthPlugin.enabled = false; //hopefully the app totally lets go of the plugin
            }

            countdown = 3;
            while (currentlyQuittingToLauncher)
            {
                countdown -= Time.deltaTime;
                if (countdown <= 0)
                {
                    //quit
                    LoadApp(appFileName);
                    Application.Quit();
                }
                yield return null;
            }

            //shouldn't reach this point if the quitting completes
            //otherwise set the depthplugin back to where it was
            if (depthPlugin != null) depthPlugin.enabled = previousDepthStatus;
        }

        void OnGUI()
        {
            if (currentlyQuittingToLauncher)
            {
                GUI.skin.box.fontSize = HoloPlay.imguiFontSize;
                string str = "returning in ";
                if (SceneManager.GetActiveScene().name == "HoloPlayLauncher")
                    str = "launching in ";
                GUI.Box(
                    HoloPlay.imguiRect,
                    str + countdown.ToString("0.0")
                    );
            }
        }
    }
}