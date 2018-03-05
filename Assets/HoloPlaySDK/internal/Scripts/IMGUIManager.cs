using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloPlaySDK
{
    public class IMGUIManager : MonoBehaviour
    {
        public static Action GUIAction;

        [SerializeField]
        int columns = 3;

        [SerializeField]
        int rows = 12;

        [Range(0.5f, 1.5f)]
        [SerializeField]
        float fontMagnification = 1;

        [SerializeField]
        Font font;

        [SerializeField]
        bool readjustLive;

        static Vector2 guiLabelSize;
        static Vector2 guiPosDefault;
        static Vector2 guiPos
        {
            get
            {
                return new Vector2(
                    guiPosDefault.x + (guiLabelSize.x + guiPosDefault.x) * guiX,
                    guiPosDefault.y + guiLabelSize.y * guiY
                );
            }
        }
        static int guiFontSize;

        static int guiX = 0;
        static int guiY = 0;
        static int guiIndex = 0;

        static bool isReady = false;

        static void Reset()
        {
            guiIndex = 0;
            guiX = 0;
            guiY = 0;
        }

        public static void AdvanceColumn()
        {
            GUIAction += () =>
            {
                guiX++;
                guiY = 0;
            };
        }

        public static void AdvanceIndex()
        {
            GUIAction += () =>
            {
                guiIndex++;
                guiY++;
            };
        }

        /// <summary>
        /// Adds a label to the set of GUIAction
        /// </summary>
        /// <param name="text"></param>
        public static void AddLabel(string text)
        {
            Label(text, Color.white);
        }

        public static void AddLabel(string text, Color color)
        {
            Label(text, color);
        }

        static void Label(string text, Color color)
        {
            GUIAction += () =>
            {
                GUI.color = color;
                Rect rect = new Rect(guiPos, guiLabelSize);
                GUI.Box(rect, text);
                guiY++;
                guiIndex++;
                GUI.color = Color.white;
            };
        }

        void Start()
        {
            ReadjustSizes();
            StartCoroutine(ClearGUIAction());
        }

        void ReadjustSizes()
        {
            guiPosDefault = new Vector2(Screen.width * 0.02f, Screen.width * 0.02f);
            guiLabelSize = new Vector2(
                (Screen.width - guiPosDefault.x) / columns - guiPosDefault.x,
                (Screen.height - guiPosDefault.x) / rows - guiPosDefault.x
            );
            guiFontSize = (int)(guiLabelSize.y * 0.4f * fontMagnification * (float)Screen.width / Screen.height);
        }

        IEnumerator ClearGUIAction()
        {
            while (true)
            {
                //clear the GUIManager action at frame end
                GUIAction = null;
                yield return new WaitForEndOfFrame();
                isReady = true;

                //readjust every x frames incase screensize changes
                if (readjustLive)
                {
                    ReadjustSizes();
                }
            }
        }

        void OnGUI()
        {
            //set gui properties
            if (font != null)
                GUI.skin.box.font = font;
            GUI.skin.box.fontSize = guiFontSize;
            GUI.skin.box.alignment = TextAnchor.MiddleCenter;

            if (isReady)
            {
                Reset();
                if (GUIAction != null)
                    GUIAction();
            }
        }
    }
}
