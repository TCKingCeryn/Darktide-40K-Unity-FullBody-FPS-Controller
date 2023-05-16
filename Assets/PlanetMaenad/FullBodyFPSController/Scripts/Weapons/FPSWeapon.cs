using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetMaenad.FPS
{
    public class FPSWeapon : MonoBehaviour
    {
        public DEMOExamplePlayer PlayerController;
        [Space(10)]

        public FPSArmsController PlayerArmsController;
        public WeaponAdjuster WeaponAdjust;
        [Space(5)]
        public GameObject mainCam;
        [Space(10)]



        public KeyCode AttackButton = KeyCode.Mouse0;
        public KeyCode AimButton = KeyCode.Mouse1;

        public bool UseFPSArmsConstant = true;
        public bool UseFPSArmsOnAttack = true;
        public bool UseFPSArmsOnAim = true;
        [Space(10)]


        public float MoveSetID = 1;
        public WeaponTypes Weapon;
        public int Damage;
        [Space(5)]
        public GameObject[] SecondaryObjects;
        public MeleeTrigger[] Hitboxes;
        [Space(5)]
        public bool UseRandomAttack;
        [Range(-5, 5)] public int MinRandom;
        [Range(-5, 5)] public int MaxRandom;
        [Space(10)]


        public Vector3 aimCamPosition;
        public Vector3 aimCamOffset = new Vector3(-15, 0, 0);
        public float originalCamFov = 60;
        public float zoomInAmount = 50;
        [Space(5)]
        public float aimTime = 5;
        public float aimInOutDuration = 0.1f;
        [Space(10)]



        public bool attacking;
        public bool swapping;
        public bool sprinting;
        public bool aiming;
        [Space(10)]




        internal Vector3 originalCamPos;
        internal Vector3 originalAimOffsetCamPos;

        internal bool aimFinished = true;
        internal float aimTimeElapsed;

        public enum WeaponTypes { Rifle, Pistol, Melee };




        void OnEnable()
        {
            //Reset adjuster to sync up every time gun is loaded
            if (WeaponAdjust) WeaponAdjust.enabled = false;
            if (WeaponAdjust) WeaponAdjust.enabled = true;

            if (UseFPSArmsConstant)
            {
                PlayerArmsController.LockFullbodyArms = true;
            }
            if (!UseFPSArmsConstant)
            {
                PlayerArmsController.LockFullbodyArms = false;
            }

            if (Hitboxes.Length > 0)
            {
                for (int i = 0; i < Hitboxes.Length; i++)
                {
                    Hitboxes[i].PlayerController = PlayerController;
                }
            }
        }
        void Start()
        {
            originalCamPos = new Vector3(0, .35f, 0.15f);
        }

        void Update()
        {
            //Attack
            if (Input.GetKeyDown(AttackButton) && Weapon == WeaponTypes.Melee)
            {
               
            }

            if (Input.GetKey(AttackButton) && Weapon == WeaponTypes.Melee)
            {            
                if (UseRandomAttack && !PlayerController.ArmsAnimator.GetCurrentAnimatorStateInfo(0).IsName("RandomAttack"))
                {
                    float randomAttackFloat = Random.Range(MinRandom, MaxRandom);
                    PlayerController.ArmsAnimator.SetFloat("RandomAttackID", randomAttackFloat);
                    PlayerController.ArmsAnimator.Play("RandomAttack");
                }

                if (UseFPSArmsOnAttack && !UseFPSArmsConstant)
                {
                    PlayerArmsController.LockFullbodyArms = true;
                }

                if (Hitboxes.Length > 0)
                {
                    for (int i = 0; i < Hitboxes.Length; i++)
                    {
                        Hitboxes[i].Trigger.enabled = true;
                    }
                }
              
                attacking = true;
            }
            if (Input.GetKeyUp(AttackButton) && Weapon == WeaponTypes.Melee)
            {
                if (UseFPSArmsOnAttack && !UseFPSArmsConstant)
                {
                    PlayerArmsController.LockFullbodyArms = false;
                }

                if (Hitboxes.Length > 0)
                {
                    for (int i = 0; i < Hitboxes.Length; i++)
                    {
                        Hitboxes[i].Trigger.enabled = false;
                    }
                }

                attacking = false;
            }

            //Sprint
            if (Input.GetKey(KeyCode.LeftShift) && !aiming)
            {
                sprinting = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                sprinting = false;
            }

            //PlayerAnimators
            if (!UseRandomAttack) PlayerController.ArmsAnimator.SetBool("Attack", attacking);
            PlayerController.ArmsAnimator.SetBool("Sprint", sprinting);
        }
        void LateUpdate()
        {

            //Aiming
            if (Input.GetKeyDown(AimButton) && aimFinished && !swapping)
            {
                if(UseFPSArmsOnAim && !UseFPSArmsConstant)
                {
                    PlayerArmsController.LockFullbodyArms = true;
                }

                mainCam.transform.localEulerAngles = aimCamOffset;

                originalAimOffsetCamPos = aimCamPosition;
                aimCamPosition += originalCamPos;

                PlayerController.ArmsAnimator.SetBool("Aiming", true);
                PlayerController.BodyAnimator.SetFloat("idleAnimSpeed", 0);

                aimTimeElapsed = 0;
                aiming = true;
                aimFinished = false;
            }
            else if (Input.GetKeyUp(AimButton) && aiming && !swapping)
            {
                if (UseFPSArmsOnAim && !UseFPSArmsConstant)
                {
                    PlayerArmsController.LockFullbodyArms = false;
                }

                mainCam.transform.localEulerAngles = Vector3.zero;

                aiming = false;
                aimTimeElapsed = 0;
                Invoke("aimingOutFinished", aimInOutDuration);

                PlayerController.ArmsAnimator.SetBool("Aiming", false);
                PlayerController.BodyAnimator.SetFloat("idleAnimSpeed", 1);

            }

            if (aiming && !aimFinished)
            {
                LerpAimIn();
            }
            else if (!aiming && !aimFinished)
            {
                LerpAimOut();
            }
        }



        void aimingOutFinished()
        {
            mainCam.GetComponent<Camera>().fieldOfView = originalCamFov;
            mainCam.transform.localPosition = originalCamPos;

            aimCamPosition = originalAimOffsetCamPos;
            aimFinished = true;
        }
        void LerpAimIn()
        {
            if (aimTimeElapsed < aimInOutDuration)
            {
                mainCam.GetComponent<Camera>().fieldOfView = Mathf.Lerp(originalCamFov, zoomInAmount, aimTimeElapsed / aimInOutDuration);
                mainCam.transform.localPosition = Vector3.Lerp(originalCamPos, aimCamPosition, aimTimeElapsed / aimInOutDuration);

                aimTimeElapsed += Time.deltaTime;
            }
            else
            {
                mainCam.GetComponent<Camera>().nearClipPlane = 0.01f;
                mainCam.GetComponent<Camera>().fieldOfView = zoomInAmount;
                mainCam.transform.localPosition = new Vector3(aimCamPosition.x, aimCamPosition.y, aimCamPosition.z);
            }
        }
        void LerpAimOut()
        {
            if (aimTimeElapsed < aimInOutDuration)
            {
                mainCam.GetComponent<Camera>().fieldOfView = Mathf.Lerp(zoomInAmount, originalCamFov, aimTimeElapsed / aimInOutDuration);
                mainCam.transform.localPosition = Vector3.Lerp(aimCamPosition, originalCamPos, aimTimeElapsed / aimInOutDuration);

                aimTimeElapsed += Time.deltaTime;
            }
            else
            {
                mainCam.GetComponent<Camera>().fieldOfView = originalCamFov;
                mainCam.transform.localPosition = originalCamPos;
            }
        }


    }
}