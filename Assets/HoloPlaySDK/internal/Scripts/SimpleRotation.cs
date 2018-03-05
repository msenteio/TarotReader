using UnityEngine;

namespace HoloPlaySDK_Tests
{
    public class SimpleRotation : MonoBehaviour
    {
        public Vector3 rotation;
        public Space space;

        void Update()
        {
            transform.Rotate(rotation * Time.deltaTime, space);
        }
    }
}

//hi