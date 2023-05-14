using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetMaenad.FPS
{
    public class WeaponAdjuster : MonoBehaviour
    {
        public bool isAi;
        public AIWeapon AIWeapon;
        [Space(10)]


        public FPSShooterWeapon Weapon;
        [Space(5)]
        public bool useHead;
        public Transform headBone;
        public Vector3 headOffsetRot = new Vector3(0, 0, 0);
        [Space(10)]

  
        public bool useLeftHand;
        public Transform leftHandBone;
        public Transform leftIKTarget;
        [Space(10)]


        public bool useIndexFinger;
        public Transform indexFinger;
        public Vector3 indexFingerOffsetRot = new Vector3(0, 0, 0);



        internal Vector3 startingHeadOffset;


        void Start()
        {
            startingHeadOffset = headOffsetRot;
        }

        void LateUpdate()
        {
            if (useHead) headBone.rotation *= Quaternion.Euler(headOffsetRot.x, headOffsetRot.y, headOffsetRot.z);

            if (!isAi)
            {
                if (useLeftHand && Weapon && !Weapon.reloading)
                {
                    leftHandBone.transform.position = leftIKTarget.transform.position;
                    leftHandBone.transform.rotation = leftIKTarget.transform.rotation;
                }
                if (useLeftHand && !Weapon)
                {
                    leftHandBone.transform.position = leftIKTarget.transform.position;
                    leftHandBone.transform.rotation = leftIKTarget.transform.rotation;
                }
            }

            if (isAi)
            {
                if (useLeftHand && AIWeapon)
                {
                    leftHandBone.transform.position = leftIKTarget.transform.position;
                    leftHandBone.transform.rotation = leftIKTarget.transform.rotation;
                }               
            }

            if (useIndexFinger) indexFinger.localEulerAngles = new Vector3(indexFinger.localEulerAngles.x + indexFingerOffsetRot.x, indexFinger.localEulerAngles.y + indexFingerOffsetRot.y, indexFinger.localEulerAngles.z + indexFingerOffsetRot.z);
            
        }
    }
}
