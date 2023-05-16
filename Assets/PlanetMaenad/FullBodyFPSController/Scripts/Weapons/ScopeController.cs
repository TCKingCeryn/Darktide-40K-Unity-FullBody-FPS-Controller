using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PlanetMaenad.FPS
{
    public class ScopeController : MonoBehaviour
    {
        public FPSArmsController PlayerArmsController;
        public float sniperAimSensitivty;
        public Animator blackLensAnim;


        internal float originalCamSensitivity;


        void Start()
        {
            originalCamSensitivity = PlayerArmsController.Sensitivity;
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
