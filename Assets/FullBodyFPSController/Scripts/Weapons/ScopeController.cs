//SlapChickenGames
//2021
//Sniper scope controller

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace scgFullBodyController
{
    public class ScopeController : MonoBehaviour
    {
        public FPSCameraController camControl;
        public float sniperAimSensitivty;
        public Animator blackLensAnim;


        internal float originalCamSensitivity;
        //internal  DepthOfField dofComponent;


        void Start()
        {
            originalCamSensitivity = camControl.Sensitivity;
        }


        // Update is called once per frame
        void Update()
        {
            if (gameObject.GetComponent<FPSShooterWeapon>().aiming)
            {
                camControl.Sensitivity = sniperAimSensitivty;

                //dofComponent.active = true;
                blackLensAnim.SetBool("aiming", true);
            }
            else
            {
                camControl.Sensitivity = originalCamSensitivity;

                //dofComponent.active = false;
                blackLensAnim.SetBool("aiming", false);
            }
        }
    }
}
