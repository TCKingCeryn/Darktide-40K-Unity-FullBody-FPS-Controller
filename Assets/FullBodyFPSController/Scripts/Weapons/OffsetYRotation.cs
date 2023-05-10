//SlapChickenGames
//2021
//Spine orientation control

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace scgFullBodyController
{
    public class OffsetYRotation : MonoBehaviour
    {
        public float offsetRotation = 80f;

        public void SetRotation(float SetRotation)
        {
            offsetRotation = SetRotation;
        }


        void LateUpdate()
        {
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + offsetRotation, 0);
        }
    }
}
