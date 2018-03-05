using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloPlaySDK
{
    public enum HP_Button
    {
        ONE,
        TWO,
        FOUR,
        THREE,
        HOME
    }

    public class HoloPlayButton
    {
        public static int holoPlayerJoystickNum = -1;
        static readonly float joystickCheckInterval = 5f;
        static float joystickLastCheckTime = 0f;

        /// <summary>
        /// This happens automatically every x seconds as called from HoloPlay.
        /// No need for manually calling this function typically
        /// </summary>
        public static void ScanForHoloPlayerJoystick()
        {
            var joyNames = Input.GetJoystickNames();
            for (int i = 0; i < joyNames.Length; i++)
            {
                if (joyNames[i].ToLower().Contains("holoplayer one"))
                {
                    holoPlayerJoystickNum = i + 1; //for whatever reason unity starts their joystick list at 1 and not 0
                    return;
                }
            }

            //if it didn't find a joystick
            holoPlayerJoystickNum = -1;
        }

        public static bool GetButton(HP_Button button)
        {
            return CheckButton((x) => Input.GetKey(x), button);
        }

        public static bool GetButtonDown(HP_Button button)
        {
            return CheckButton((x) => Input.GetKeyDown(x), button);
        }

        public static bool GetButtonUp(HP_Button button)
        {
            return CheckButton((x) => Input.GetKeyUp(x), button);
        }

        /// <summary>
        /// Get any button down. By default, includeHome is false and it will only return on buttons 1-4
        /// </summary>
        public static bool GetAnyButtonDown(bool includeHome = false)
        {
            for (int i = 0; i < Enum.GetNames(typeof(HP_Button)).Length; i++)
            {
                var button = (HP_Button)i;
                if (includeHome && button == HP_Button.HOME)
                    continue;

                if (GetButtonDown(button)) return true;
            }
            return false;
        }

        static bool CheckButton(Func<KeyCode, bool> buttonFunc, HP_Button button)
        {
            bool buttonPress = false;
            if (holoPlayerJoystickNum >= 0)
            {
                buttonPress = buttonPress || buttonFunc(ButtonToJoystickKeycode(button));
            }
            if (HoloPlay.Config.keyboardAsButtons == 1 || holoPlayerJoystickNum < 0)
            {
                buttonPress = buttonPress || buttonFunc(ButtonToNumberOnKeyboard(button));
            }

            return buttonPress;
        }

        static KeyCode ButtonToJoystickKeycode(HP_Button button)
        {
            return
                (KeyCode)Enum.Parse(
                    typeof(KeyCode),
                    "Joystick" + holoPlayerJoystickNum + "Button" + (int)button
                );
        }

        static KeyCode ButtonToNumberOnKeyboard(HP_Button button)
        {
            switch (button)
            {
                case HP_Button.ONE: return KeyCode.Alpha1;
                case HP_Button.TWO: return KeyCode.Alpha2;
                case HP_Button.THREE: return KeyCode.Alpha3;
                case HP_Button.FOUR: return KeyCode.Alpha4;
                case HP_Button.HOME: return KeyCode.Escape;
                default: return KeyCode.Escape;
            }
        }
    }
}