using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


namespace PlanetMaenad.FPS
{
    public class FPSCameraController : MonoBehaviour
    {
        public Transform PlayerRoot;
        public Transform MiddleSpineBone;
        public Camera MainCam;
        public PostProcessVolume DamageVolume;
        public PostProcessVolume DeathVolume;
        [Space(5)]
        public float Sensitivity = 10f;
        public float minPitch = -30f;
        public float maxPitch = 60f;
        [Space(10)]


        public Vector3 LeftShoulderOffset = new Vector3(-90,-90,90);
        public Vector3 RightShoulderOffset = new Vector3(90, 90, -90);
        [Space(5)]
        public Vector3 LeftArmsOffset = new Vector3(0, 0, 90);
        public Vector3 RightArmsOffset = new Vector3(0, 0, -90);
        [Space(5)]
        public Vector3 LeftFingersOffset = new Vector3(90, 0, 90);
        public Vector3 RightFingersOffset = new Vector3(-90, 0, -90);
        [Space(5)]
        public float pitch = 0f;
        public float yaw = 0f;
        [Space(10)]



        public bool LockFullbodyArms;
        public FullBodyRig _fullBodyRig;
        public FPSBodyRig _FPSBodyRig;


        [System.Serializable]
        public class FullBodyRig
        {
            public Transform LeftShoulder;
            public Transform LeftArm;
            public Transform LeftForeArm;
            public Transform LeftHand;
            [Space(10)]
            public Transform LeftHandThumb1;
            public Transform LeftHandThumb2;
            public Transform LeftHandThumb3;
            [Space(5)]
            public Transform LeftHandIndex1;
            public Transform LeftHandIndex2;
            public Transform LeftHandIndex3;
            [Space(5)]
            public Transform LeftHandMiddle1;
            public Transform LeftHandMiddle2;
            public Transform LeftHandMiddle3;
            [Space(5)]
            public Transform LeftHandRing1;
            public Transform LeftHandRing2;
            public Transform LeftHandRing3;
            [Space(5)]
            public Transform LeftHandPinky1;
            public Transform LeftHandPinky2;
            public Transform LeftHandPinky3;
            [Space(20)]

            public Transform RightShoulder;
            public Transform RightArm;
            public Transform RightForeArm;
            public Transform RightHand;
            [Space(10)]
            public Transform RightHandThumb1;
            public Transform RightHandThumb2;
            public Transform RightHandThumb3;
            [Space(5)]
            public Transform RightHandIndex1;
            public Transform RightHandIndex2;
            public Transform RightHandIndex3;
            [Space(5)]
            public Transform RightHandMiddle1;
            public Transform RightHandMiddle2;
            public Transform RightHandMiddle3;
            [Space(5)]
            public Transform RightHandRing1;
            public Transform RightHandRing2;
            public Transform RightHandRing3;
            [Space(5)]
            public Transform RightHandPinky1;
            public Transform RightHandPinky2;
            public Transform RightHandPinky3;

        }

        [System.Serializable]
        public class FPSBodyRig
        {
            public Transform LeftShoulder;
            public Transform LeftArm;
            public Transform LeftForeArm;
            public Transform LeftHand;
            [Space(10)]
            public Transform LeftHandThumb1;
            public Transform LeftHandThumb2;
            public Transform LeftHandThumb3;
            [Space(5)]
            public Transform LeftHandIndex1;
            public Transform LeftHandIndex2;
            public Transform LeftHandIndex3;
            [Space(5)]
            public Transform LeftHandMiddle1;
            public Transform LeftHandMiddle2;
            public Transform LeftHandMiddle3;
            [Space(5)]
            public Transform LeftHandRing1;
            public Transform LeftHandRing2;
            public Transform LeftHandRing3;
            [Space(5)]
            public Transform LeftHandPinky1;
            public Transform LeftHandPinky2;
            public Transform LeftHandPinky3;
            [Space(20)]



            public Transform RightShoulder;
            public Transform RightArm;
            public Transform RightForeArm;
            public Transform RightHand;
            [Space(10)]
            public Transform RightHandThumb1;
            public Transform RightHandThumb2;
            public Transform RightHandThumb3;
            [Space(5)]
            public Transform RightHandIndex1;
            public Transform RightHandIndex2;
            public Transform RightHandIndex3;
            [Space(5)]
            public Transform RightHandMiddle1;
            public Transform RightHandMiddle2;
            public Transform RightHandMiddle3;
            [Space(5)]
            public Transform RightHandRing1;
            public Transform RightHandRing2;
            public Transform RightHandRing3;
            [Space(5)]
            public Transform RightHandPinky1;
            public Transform RightHandPinky2;
            public Transform RightHandPinky3;
        }

        internal float relativeYaw = 0f;


        void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void LateUpdate()
        {
            CameraRotate();
            LockFullBodyArmBones();

            //Lock Camera To Spine 
            if (MiddleSpineBone) transform.position = MiddleSpineBone.position;
        }

        public void ToggleDeathPostProfile(bool Bool)
        {
            DeathVolume.enabled = Bool;
        }
        public void ToggleDamagePostProfile(bool Bool)
        {
            DamageVolume.enabled = Bool;
        }

        public void ReceiveDamage()
        {
            StopCoroutine(ReceiveDamageDelay());
            StartCoroutine(ReceiveDamageDelay());
        }
        IEnumerator ReceiveDamageDelay()
        {
            ToggleDamagePostProfile(true);

            yield return new WaitForSeconds(1f);

            ToggleDamagePostProfile(false);
        }

        void CameraRotate()
        {
            //Get input to turn the cam view
            relativeYaw = Input.GetAxis("Mouse X") * Sensitivity;
            pitch -= Input.GetAxis("Mouse Y") * Sensitivity;

            yaw += Input.GetAxis("Mouse X") * Sensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);

            //Rotate Spine 
            if (MiddleSpineBone) MiddleSpineBone.rotation = transform.rotation;
        }
        void LockFullBodyArmBones()
        {
            if (LockFullbodyArms)
            {
                _fullBodyRig.LeftShoulder.position = _FPSBodyRig.LeftShoulder.position;
                _fullBodyRig.LeftShoulder.rotation = _FPSBodyRig.LeftShoulder.rotation * Quaternion.Euler(-90,-90,LeftShoulderOffset.z);

                _fullBodyRig.LeftArm.position = _FPSBodyRig.LeftArm.position;
                _fullBodyRig.LeftArm.rotation = _FPSBodyRig.LeftArm.rotation * Quaternion.Euler(LeftArmsOffset);

                _fullBodyRig.LeftForeArm.position = _FPSBodyRig.LeftForeArm.position;
                _fullBodyRig.LeftForeArm.rotation = _FPSBodyRig.LeftForeArm.rotation * Quaternion.Euler(LeftArmsOffset);

                _fullBodyRig.LeftHand.position = _FPSBodyRig.LeftHand.position;
                _fullBodyRig.LeftHand.rotation = _FPSBodyRig.LeftHand.rotation * Quaternion.Euler(LeftArmsOffset);



                _fullBodyRig.LeftHandThumb1.rotation = _FPSBodyRig.LeftHandThumb1.rotation * Quaternion.Euler(LeftFingersOffset);
                _fullBodyRig.LeftHandThumb2.rotation = _FPSBodyRig.LeftHandThumb2.rotation * Quaternion.Euler(LeftFingersOffset);
                _fullBodyRig.LeftHandThumb3.rotation = _FPSBodyRig.LeftHandThumb3.rotation * Quaternion.Euler(LeftFingersOffset);

                _fullBodyRig.LeftHandIndex1.rotation = _FPSBodyRig.LeftHandIndex1.rotation * Quaternion.Euler(LeftFingersOffset);
                _fullBodyRig.LeftHandIndex2.rotation = _FPSBodyRig.LeftHandIndex2.rotation * Quaternion.Euler(LeftFingersOffset);
                _fullBodyRig.LeftHandIndex3.rotation = _FPSBodyRig.LeftHandIndex3.rotation * Quaternion.Euler(LeftFingersOffset);

                _fullBodyRig.LeftHandMiddle1.rotation = _FPSBodyRig.LeftHandMiddle1.rotation * Quaternion.Euler(LeftFingersOffset);
                _fullBodyRig.LeftHandMiddle2.rotation = _FPSBodyRig.LeftHandMiddle2.rotation * Quaternion.Euler(LeftFingersOffset);
                _fullBodyRig.LeftHandMiddle3.rotation = _FPSBodyRig.LeftHandMiddle3.rotation * Quaternion.Euler(LeftFingersOffset);

                _fullBodyRig.LeftHandRing1.rotation = _FPSBodyRig.LeftHandRing1.rotation * Quaternion.Euler(LeftFingersOffset);
                _fullBodyRig.LeftHandRing2.rotation = _FPSBodyRig.LeftHandRing2.rotation * Quaternion.Euler(LeftFingersOffset);
                _fullBodyRig.LeftHandRing3.rotation = _FPSBodyRig.LeftHandRing3.rotation * Quaternion.Euler(LeftFingersOffset);

                _fullBodyRig.LeftHandPinky1.rotation = _FPSBodyRig.LeftHandPinky1.rotation * Quaternion.Euler(LeftFingersOffset);
                _fullBodyRig.LeftHandPinky2.rotation = _FPSBodyRig.LeftHandPinky2.rotation * Quaternion.Euler(LeftFingersOffset);
                _fullBodyRig.LeftHandPinky3.rotation = _FPSBodyRig.LeftHandPinky3.rotation * Quaternion.Euler(LeftFingersOffset);







                _fullBodyRig.RightShoulder.position = _FPSBodyRig.RightShoulder.position;
                _fullBodyRig.RightShoulder.rotation = _FPSBodyRig.RightShoulder.rotation * Quaternion.Euler(90,90,RightShoulderOffset.z);

                _fullBodyRig.RightArm.position = _FPSBodyRig.RightArm.position;
                _fullBodyRig.RightArm.rotation = _FPSBodyRig.RightArm.rotation * Quaternion.Euler(RightArmsOffset);

                _fullBodyRig.RightForeArm.position = _FPSBodyRig.RightForeArm.position;
                _fullBodyRig.RightForeArm.rotation = _FPSBodyRig.RightForeArm.rotation * Quaternion.Euler(RightArmsOffset);

                _fullBodyRig.RightHand.position = _FPSBodyRig.RightHand.position;
                _fullBodyRig.RightHand.rotation = _FPSBodyRig.RightHand.rotation * Quaternion.Euler(RightArmsOffset);




                _fullBodyRig.RightHandThumb1.rotation = _FPSBodyRig.RightHandThumb1.rotation * Quaternion.Euler(RightFingersOffset);
                _fullBodyRig.RightHandThumb2.rotation = _FPSBodyRig.RightHandThumb2.rotation * Quaternion.Euler(RightFingersOffset);
                _fullBodyRig.RightHandThumb3.rotation = _FPSBodyRig.RightHandThumb3.rotation * Quaternion.Euler(RightFingersOffset);

                _fullBodyRig.RightHandIndex1.rotation = _FPSBodyRig.RightHandIndex1.rotation * Quaternion.Euler(RightFingersOffset);
                _fullBodyRig.RightHandIndex2.rotation = _FPSBodyRig.RightHandIndex2.rotation * Quaternion.Euler(RightFingersOffset);
                _fullBodyRig.RightHandIndex3.rotation = _FPSBodyRig.RightHandIndex3.rotation * Quaternion.Euler(RightFingersOffset);

                _fullBodyRig.RightHandMiddle1.rotation = _FPSBodyRig.RightHandMiddle1.rotation * Quaternion.Euler(RightFingersOffset);
                _fullBodyRig.RightHandMiddle2.rotation = _FPSBodyRig.RightHandMiddle2.rotation * Quaternion.Euler(RightFingersOffset);
                _fullBodyRig.RightHandMiddle3.rotation = _FPSBodyRig.RightHandMiddle3.rotation * Quaternion.Euler(RightFingersOffset);

                _fullBodyRig.RightHandRing1.rotation = _FPSBodyRig.RightHandRing1.rotation * Quaternion.Euler(RightFingersOffset);
                _fullBodyRig.RightHandRing2.rotation = _FPSBodyRig.RightHandRing2.rotation * Quaternion.Euler(RightFingersOffset);
                _fullBodyRig.RightHandRing3.rotation = _FPSBodyRig.RightHandRing3.rotation * Quaternion.Euler(RightFingersOffset);

                _fullBodyRig.RightHandPinky1.rotation = _FPSBodyRig.RightHandPinky1.rotation * Quaternion.Euler(RightFingersOffset);
                _fullBodyRig.RightHandPinky2.rotation = _FPSBodyRig.RightHandPinky2.rotation * Quaternion.Euler(RightFingersOffset);
                _fullBodyRig.RightHandPinky3.rotation = _FPSBodyRig.RightHandPinky3.rotation * Quaternion.Euler(RightFingersOffset);
            }
        }
    }
}