using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloPlaySDK
{
    public class Screenshot3DRenderer : MonoBehaviour
    {
        [HideInInspector]
        public bool useCustomTilecount;
        public Renderer r;

        void OnEnable() { HoloPlay.onViewRender += ShowScene; }
        void OnDisable() { HoloPlay.onViewRender -= ShowScene; }

        void Awake()
        {
            r = GetComponent<Renderer>();
        }

        void Update()
        {
            r.material.SetFloat("_TilesX", HoloPlay.tilesX);
            r.material.SetFloat("_TilesY", HoloPlay.tilesY);

			//abandoned attempt at doing it the right way
            // if (HoloPlay.Main == null)
            // {
            //     return;
            // }

            // Vector3 screenshotDir = transform.forward;
            // Vector3 screenshotPos = transform.position;
            // Vector3 holoplayPos = HoloPlay.Main.transform.position;

            // Vector3 finalDir = screenshotPos - holoplayPos;
			// print(finalDir);
            // Vector3 anotherPoint = HoloPlay.Main.transform.InverseTransformPoint(finalDir);
			// print(anotherPoint);
            // anotherPoint.z = 0;

            // //that triangle
            // var theta = Mathf.Asin(anotherPoint.x / anotherPoint.magnitude);
            // print(theta * Mathf.Rad2Deg);
        }

        void ShowScene(int i)
        {
            r.material.SetFloat("_View", i);
        }
    }
}