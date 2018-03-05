using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloPlaySDK
{
    /// <summary>
    /// This is for using your regular screen as a 2D UI while using the HoloPlayer for 3D content
    /// </summary>
    public class SwitchToMirrored : MonoBehaviour
    {
        Camera plain2DCam;
        Camera holoPlayCam;

        void OnEnble()
        {
            plain2DCam = GetComponent<Camera>();
            if (plain2DCam == null)
            {
                Debug.LogWarning("SwitchToMirrored must be placed on a regular Unity camera!");
                enabled = false;
                return;
            }

            if (HoloPlay.Main != null)
                holoPlayCam = HoloPlay.Main.GetComponent<Camera>();

            if (holoPlayCam == null)
            {
                Debug.LogWarning("SwitchToMirrored needs a HoloPlay Capture to operate");
                enabled = false;
                return;
            }

            if (holoPlayCam == plain2DCam)
            {
                Debug.LogWarning("SwitchToMirrored must be placed on a regular Unity camera, not your HoloPlay Capture!");
                enabled = false;
                return;
            }

            if (Display.displays.Length == 1 || Application.platform == RuntimePlatform.OSXPlayer)
            {
                Mirror();
            }
            else if (Display.displays.Length > 1)
            {
                Display.displays[1].Activate();
                Extend();
            }
        }

        void Mirror()
        {
            holoPlayCam.targetDisplay = 0;
            plain2DCam.gameObject.SetActive(false);
        }

        void Extend()
        {
            if (Display.displays.Length < 2)
                return;
            plain2DCam.gameObject.SetActive(true);
            plain2DCam.cullingMask = (1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("UI"));
            holoPlayCam.targetDisplay = 1;
        }
    }
}