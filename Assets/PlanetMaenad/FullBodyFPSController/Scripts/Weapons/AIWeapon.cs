using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PlanetMaenad.FPS
{
    public class AIWeapon : MonoBehaviour
    {
        public Animator AIAnimator;
        public WeaponAdjuster WeaponAdjust;
        [Space(10)]


        public float MoveSetID = 1;
        public WeaponTypes Weapon;
        public int Damage;
        [Space(5)]
        public GameObject[] SecondaryObjects;
        public Collider[] Hitboxes;
        public bool attacking;


        public enum WeaponTypes { Rifle, Pistol, Melee };


        void OnEnable()
        {
            //Reset adjuster to sync up every time gun is loaded
            WeaponAdjust.enabled = false;
            WeaponAdjust.enabled = true;
        }

    }
}