using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetMaenad.FPS
{
    public class OffsetYRotation : MonoBehaviour
    {
        public float offsetRotation = 80f;
        public Transform RotateTarget;

        internal Vector3 RotateOffset;

        public void SetRotation(float SetRotation)
        {
            offsetRotation = SetRotation;
        }


        void LateUpdate()
        {
            //if (RotateTarget) transform.LookAt(RotateTarget.position + RotateOffset);

            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + offsetRotation, 0);
        }
    }
}
