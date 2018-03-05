using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HoloPlaySDK
{
    public class ControlsTest : MonoBehaviour
    {
        Transform canvasTransform;
        GameObject panelPrefab;
        Dictionary<int, float> buttonDownDict = new Dictionary<int, float>();
        readonly float fadeSpeed = 2;

        void Update()
        {
            var buttonNames = Enum.GetNames(typeof(HP_Button));

            //get button
            for (int i = 0; i < buttonNames.Length; i++)
            {
                bool button = HoloPlayButton.GetButton((HP_Button)i);
                string btnName = buttonNames[i];
                IMGUIManager.AddLabel(
                    btnName + " = " + button,
                    button ? Color.green : Color.white
                );
            }

            IMGUIManager.AdvanceColumn();

            //handling both button down and button up here like an idiot
            for (int i = 0; i < buttonNames.Length * 2; i++)
            {
                int j = i;
                string action = " Down";
                Func<int, bool> buttonAction = (x) => HoloPlayButton.GetButtonDown((HP_Button)x);
                Color downColor = Color.red;
                if (i == buttonNames.Length)
                {
                    IMGUIManager.AdvanceColumn();
                }
                if (i >= buttonNames.Length)
                {
                    j = i - buttonNames.Length;
                    action = " Up";
                    buttonAction = (x) => HoloPlayButton.GetButtonUp((HP_Button)x);
                    downColor = Color.blue;
                }

                bool button = buttonAction(j);
                Color textColor = Color.white;
                if (buttonDownDict.ContainsKey(i))
                {
                    if (button)
                        buttonDownDict[i] = 1f;
                    textColor = Color.Lerp(Color.white, downColor, buttonDownDict[i]);
                }
                else
                {
                    if (button)
                    {
                        buttonDownDict.Add(i, 1f);
                        textColor = downColor;
                    }
                }
                string btnName = buttonNames[j];
                IMGUIManager.AddLabel(
                    btnName + action,
                    textColor
                );
            }

            IMGUIManager.AdvanceIndex();

            bool k = HoloPlay.Config.keyboardAsButtons.Value == 1;
            IMGUIManager.AddLabel(
                "Keyboard = " + k,
                k ? Color.green : Color.white
            );

            IMGUIManager.AddLabel(
                "Holo Joy num = " + HoloPlayButton.holoPlayerJoystickNum
            );

            var keys = buttonDownDict.Keys.ToArray();
            var toRemove = new List<int>();
            for (int i = 0; i < keys.Length; i++)
            {
                buttonDownDict[keys[i]] -= Time.deltaTime * fadeSpeed;
                if (buttonDownDict[keys[i]] < 0) toRemove.Add(keys[i]);
            }
            foreach (var remove in toRemove)
            {
                buttonDownDict.Remove(remove);
            }
        }
    }
}