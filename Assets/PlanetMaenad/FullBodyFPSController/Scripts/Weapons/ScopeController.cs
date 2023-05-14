using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PlanetMaenad.FPS
{
    public class ScopeController : MonoBehaviour
    {
        public FPSCameraController camControl;
        public float sniperAimSensitivty;
        public Animator blackLensAnim;


        internal float originalCamSensitivity;


        void Start()
        {
            originalCamSensitivity = camControl.Sensitivity;
        }


        // Update is called once per frame
        void Update()
        {
            if (gameObject.GetComponent<FPSShooterWeapon>().aiming)
            {
                blackLensAnim.SetBool("aiming", true);
            }
            else
            {
                blackLensAnim.SetBool("aiming", false);
            }
        }
    }
}
