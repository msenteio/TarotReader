//Copyright 2017 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using HoloPlaySDK;

using HoloPlaySDK_UI;

using UnityEngine;

namespace HoloPlaySDK
{
    public class RealsenseCalibrator : HoloPlaySDK.depthTouchTarget
    {
        [Serializable]
        public class CalibrationValues
        {
            //initialize to approx/expected values
            public float _xMin = 5.6f;
            public float _xMax = -9.5f;
            public float _yMin = -10.6f;
            public float _yMax = -0.1f;
            public float _zMin = 7.5f;
            public float _zMax = 14.6f;
            public float xAngle = -19.1f;
            public Vector3 offset = new Vector3(-1.5f, -1.32f, 0);
            public readonly Vector3 defaultOffset;
            public CalibrationValues()
            {
                defaultOffset = offset;
            }
        }

        static RealsenseCalibrator instance;
        public static RealsenseCalibrator Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<RealsenseCalibrator>();
                return instance;
            }
            private set { instance = value; }
        }

        private GameObject calibrationMarker;

        private static int calibrationState = -1;

        private float offsetTweakSpeed = 1.5f;

        private float zDistortionCorrection = .4f;

        private float frontZCutoff = -.35f;
        private float backZcutoff = .428f;

        private List<Vector3> rawPositions = new List<Vector3>();

        private Vector3[] corners;

        private Vector3 v1;
        private Vector3 v2;

        private float initZDist;
        private float markerInitScale = 1f;
        public float markerScale = 1f;

        bool saveTimerIsRunning;
        float saveTimerFloat;

        public static Action<int> onAdvanceCalibration;

        bool ready = false;

        [HideInInspector]
        public bool activelyCalibrating = false;

        private Vector3 lastGoodPos;

        //INPUT MANAGER

        //Allowing certain touches only
        List<int> validTouchIndexes = new List<int>();

        /// <summary>
        /// The current number of touches.
        /// </summary>
        [HideInInspector] public int touchCount;

        [SerializeField] Vector3 detectionRange = new Vector3(5f, 5f, 5f);
        Vector3 startDetectionRange;

        float hpStartSize;

        int lastFrameTouchCount = 0;

        void Awake()
        {
            instance = this;

            hpStartSize = HoloPlay.Main.size;
            startDetectionRange = detectionRange;

            StartCoroutine(WaitToInit());
        }

        IEnumerator WaitToInit()
        {
            while (HoloPlay.Main == null || !HoloPlay.Main.isReady)
                yield return null;

            //cycle through a few frames before proceeding
            int i = 0;
            while (i++ < 3)
                yield return null;

            DelayedInit();
        }

        void DelayedInit()
        {
            var calib = HoloPlay.Config.realsense;

            if (activelyCalibrating)
                InitMarker();
            initZDist = zDistortionCorrection;
            transform.localRotation = Quaternion.Euler(Vector3.right * calib.xAngle);

            zDistortionCorrection = initZDist / HoloPlay.Main.size;
            corners = HoloPlay.Main.GetFrustumCorners(HoloPlay.Main.Cam, HoloPlay.Main.GetAdjustedSize(), true);

            ready = true;
        }

        void Update()
        {
            if (activelyCalibrating)
            {
                var calib = HoloPlay.Config.realsense;

                if (Input.GetKeyDown(KeyCode.Space) && calibrationState != -1)
                    AdvanceCalibration();

                else if (Input.GetKey(KeyCode.RightArrow))
                    calib.offset.x -= offsetTweakSpeed * Time.deltaTime;
                else if (Input.GetKey(KeyCode.LeftArrow))
                    calib.offset.x += offsetTweakSpeed * Time.deltaTime;
                else if (Input.GetKey(KeyCode.UpArrow))
                    calib.offset.y += offsetTweakSpeed * Time.deltaTime;
                else if (Input.GetKey(KeyCode.DownArrow))
                    calib.offset.y -= offsetTweakSpeed * Time.deltaTime;
                else if (Input.GetKey(KeyCode.A))
                    calib.offset.z += offsetTweakSpeed * Time.deltaTime;
                else if (Input.GetKey(KeyCode.Z))
                    calib.offset.z -= offsetTweakSpeed * Time.deltaTime;

                if (Input.GetKeyDown(KeyCode.Y))
                    calib.offset = calib.defaultOffset;

                if (Input.anyKeyDown)
                {
                    saveTimerFloat = 0.5f;
                    if (!saveTimerIsRunning)
                        StartCoroutine(SaveTimer());
                }
            }

            UpdateDetectionRange();
        }

        IEnumerator SaveTimer()
        {
            saveTimerIsRunning = true;
            while (saveTimerFloat > 0)
            {
                saveTimerFloat -= Time.deltaTime;
                yield return null;
            }
            HoloPlay.SaveConfigToFile();
            saveTimerIsRunning = false;
        }

        void InitMarker()
        {
            if (calibrationMarker == null)
            {
                calibrationMarker = Instantiate((GameObject)Resources.Load("Marker"), HoloPlay.Main.transform.position,
                    Quaternion.identity);
                markerInitScale = calibrationMarker.transform.localScale.x;
            }
            calibrationMarker.GetComponentInChildren<RealsenseTargetCircle>().SetWidth(.02f * HoloPlay.Main.size);
            calibrationMarker.transform.localScale = Vector3.one * markerInitScale * HoloPlay.Main.size * markerScale;
        }

        ///Gets the position in world coordinates of any touch in range of the RealSense.
        public Vector3 GetWorldPos(int touchIndex)
        {
            if (!ready)
                return Vector3.zero;

            var calib = HoloPlay.Config.realsense;

            if (touchIndex >= rawPositions.Count)
            {
                return HoloPlay.Main.transform.rotation * transform.localRotation *
                    lastGoodPos + HoloPlay.Main.transform.position;
            }
            else
            {
                Vector3 remappedPos = RemapVector3(rawPositions[touchIndex] + calib.offset);
                lastGoodPos = remappedPos;
                return HoloPlay.Main.transform.rotation * transform.localRotation *
                    lastGoodPos + HoloPlay.Main.transform.position;
            }
        }

        ///Gets the position as a coordinate in the HoloPlay capture between 0 and 1.
        public Vector3 GetLocalizedPos(int touchIndex)
        {
            if (!ready)
                return Vector3.zero;

            var calib = HoloPlay.Config.realsense;
            if (touchIndex >= rawPositions.Count)
            {
                return -Vector3.up * 100 * HoloPlay.Main.size;
            }
            return RemapVector3Normalized(rawPositions[touchIndex] + calib.offset);
        }

        public void AdvanceCalibration()
        {
            var calib = HoloPlay.Config.realsense;

            if (onAdvanceCalibration != null)
                onAdvanceCalibration(calibrationState);

            switch (calibrationState)
            {
                case -1:
                    InitMarker();
                    calibrationMarker.SetActive(true);
                    calibrationMarker.transform.localPosition = HoloPlay.Main.transform.forward * frontZCutoff * HoloPlay.Main.size;
                    break;
                case 0:
                    if (rawPositions.Count > 0)
                    {
                        v2 = rawPositions[0];
                        calib._zMax = rawPositions[0].z;
                    }
                    calibrationMarker.transform.localPosition = HoloPlay.Main.transform.forward * backZcutoff * HoloPlay.Main.size;
                    break;
                case 1:
                    if (rawPositions.Count > 0)
                    {
                        v1 = rawPositions[0];
                        calib._zMin = rawPositions[0].z;
                    }
                    SetCalibrationAngle();
                    calibrationMarker.transform.localPosition = corners[0] * HoloPlay.Main.size;
                    break;
                case 2:
                    if (rawPositions.Count > 0)
                    {
                        calib._xMin = rawPositions[0].x;
                        calib._yMin = rawPositions[0].y;
                    }
                    calibrationMarker.transform.localPosition = corners[2] * HoloPlay.Main.size;
                    break;
                case 3:
                    if (rawPositions.Count > 0)
                    {
                        calib._xMax = rawPositions[0].x;
                        calib._yMax = rawPositions[0].y;
                    }
                    calibrationMarker.SetActive(false);
                    calibrationState = -2;
                    SaveConfig();
                    break;
            }
            calibrationState++;
        }

        void SetCalibrationAngle()
        {
            var calib = HoloPlay.Config.realsense;
            Vector3 dir = (v2 - v1).normalized;
            float angle = Vector3.Angle(dir, Vector3.forward);
            calib.xAngle = -angle;
            transform.localRotation = Quaternion.Euler(Vector3.right * calib.xAngle);
        }

        public override void onDepthTouch(List<depthTouch> touches)
        {
            rawPositions.Clear();

            for (int i = 0; i < touches.Count; i++)
            {
                rawPositions.Add(touches[i].getLocalPos());
            }

            SetValidTouches(touches);
        }

        Vector3 RemapVector3(Vector3 vec)
        {
            var calib = HoloPlay.Config.realsense;

            Vector3 newVec = new Vector3(
                vec.x.Remap(calib._xMin, calib._xMax, corners[0].x * HoloPlay.Main.size, corners[2].x * HoloPlay.Main.size),
                vec.y.Remap(calib._yMin, calib._yMax, corners[0].y * HoloPlay.Main.size, corners[2].y * HoloPlay.Main.size),
                vec.z.Remap(calib._zMin, calib._zMax, backZcutoff * HoloPlay.Main.size, frontZCutoff * HoloPlay.Main.size)
            );

            newVec.z += Mathf.Abs(newVec.y * newVec.y * zDistortionCorrection);

            return newVec;

        }

        Vector3 RemapVector3Normalized(Vector3 vec)
        {
            var calib = HoloPlay.Config.realsense;

            Vector3 newVec = new Vector3(
                vec.x.Remap(calib._xMin, calib._xMax, 0, 1),
                vec.y.Remap(calib._yMin, calib._yMax, 0, 1),
                vec.z.Remap(calib._zMin, calib._zMax, 0, 1)
            );
            newVec.z += Mathf.Abs(newVec.y * newVec.y * zDistortionCorrection);

            return newVec;
        }

        #region INPUT MANAGER STUFF
        /// <summary>
        /// Checks if a valid input began this frame at a specified index (default 0).
        /// </summary>
        /// <returns><c>true</c>, if input initially occured in this frame, <c>false</c> otherwise.</returns>
        /// <param name="touchIndex">Touch index to check has begun.</param>
        public bool InputBegan(int touchIndex = 0)
        {

            bool newTouch = false;

            if (lastFrameTouchCount == touchIndex && touchCount > touchIndex)
                newTouch = true;

            return newTouch;
        }

        /// <summary>
        /// Checks if there is input. This is true when there are more valid touches than the specified index (default 0).
        /// </summary>
        /// <returns><c>true</c>, if is there is input, <c>false</c> otherwise.</returns>
        public bool ThereIsInput(int index = 0)
        {
            if (validTouchIndexes.Count > index)
                return true;
            else
                return false;
        }

        /// <summary>
        /// There is no longer valid input at specified index (default 0).
        /// </summary>
        /// <returns><c>true</c>, if there are no longer any valid touches this frame, <c>false</c> otherwise.</returns>
        /// <param name="touchIndex">Touch index to check has ended.</param>
        public bool InputEnded(int touchIndex = 0)
        {
            if (lastFrameTouchCount > touchIndex && validTouchIndexes.Count <= touchIndex)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the position of the touch specified by the index within the interaction range.
        /// </summary>
        /// <returns>The position.</returns>
        /// <param name="index">Specifies the index of the touch to check.</param>
        /// <param name="worldSpace">If set to <c>Space.World</c>, returns world space coordinates.</param>
        public Vector3 GetValidTouchPos(int index, Space space = Space.World)
        {
            if (ThereIsInput())
            {
                if (space == Space.World)
                    return RealsenseCalibrator.Instance.GetWorldPos(validTouchIndexes[0]);
                else
                    return RealsenseCalibrator.Instance.GetLocalizedPos(validTouchIndexes[0]);
            }

            Debug.LogError("No valid touches! Make sure you're only requesting when ThereIsInput() is true.");
            return Vector3.zero;

        }

        /// <summary>
        /// Gets the average position of the touches within the interaction range.
        /// </summary>
        /// <returns>The average position.</returns>
        /// <param name="worldSpace">If set to <c>Space.World</c>, returns world space coordinates.</param>
        public Vector3 GetValidAveragePos(Space space = Space.World)
        {
            Vector3 pos = Vector3.zero;

            for (int i = 0; i < validTouchIndexes.Count; i++)
            {
                if (space == Space.World)
                    pos += RealsenseCalibrator.Instance.GetWorldPos(validTouchIndexes[i]);
                else
                    pos += RealsenseCalibrator.Instance.GetLocalizedPos(validTouchIndexes[i]);
            }

            pos /= validTouchIndexes.Count;

            return pos;
        }



        //Clear list and counts when there are no touches
        public override void onNoDepthTouches()
        {
            lastFrameTouchCount = 0;
            validTouchIndexes.Clear();
            touchCount = 0;
        }

        //Set the list of valid touches and the end count by checking touches against the detection range
        void SetValidTouches(List<depthTouch> touchList)
        {
            lastFrameTouchCount = touchCount;
            validTouchIndexes.Clear();
            for (int i = 0; i < touchList.Count; i++)
            {
                if (InRange(RealsenseCalibrator.Instance.GetWorldPos(i)))
                    validTouchIndexes.Add(i);
            }

            touchCount = validTouchIndexes.Count;
        }

        /// <summary>
        /// Updates the detection range when the HoloPlay size is changed. Right now, we're calling it every frame, which is fine but less than ideal
        /// </summary>
        void UpdateDetectionRange()
        {
            detectionRange = startDetectionRange * HoloPlay.Main.size / hpStartSize;
        }

        /// <summary>
        /// Checks if a position is in range.
        /// </summary>
        /// <returns><c>true</c>, if position is in the detection range, <c>false</c> otherwise.</returns>
        /// <param name="pos">Position.</param>
        bool InRange(Vector3 pos)
        {
            pos = HoloPlay.Main.transform.InverseTransformPoint(pos);

            if (Mathf.Abs(pos.x) > detectionRange.x
                || Mathf.Abs(pos.y) > detectionRange.y
                || Mathf.Abs(pos.z) > detectionRange.z)
                return false;

            return true;
        }

        /// <summary>
        /// Checks if a position is in range.
        /// </summary>
        /// <returns><c>true</c>, if position is in the provided detection range around the provided center, <c>false</c> otherwise.</returns>
        /// <param name="pos">Position.</param>
        /// <param name="yourCenter">Your center.</param>
        /// <param name="yourDetectionRange">Your detection range.</param>
        bool InRange(Vector3 pos, Vector3 yourCenter, Vector3 yourDetectionRange)
        {
            pos -= yourCenter;

            if (Mathf.Abs(pos.x) > yourDetectionRange.x
                || Mathf.Abs(pos.y) > yourDetectionRange.y
                || Mathf.Abs(pos.z) > yourDetectionRange.z)
                return false;

            return true;
        }
        #endregion

        public void SaveConfig()
        {
            HoloPlay.SaveConfigToFile();
        }

        public void LoadConfig()
        {
            HoloPlay.LoadConfig();
        }
    }
}