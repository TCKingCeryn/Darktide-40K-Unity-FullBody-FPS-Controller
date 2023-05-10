using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace scgFullBodyController
{
    [RequireComponent(typeof(AudioSource))]
    public class FPSShooterWeapon : FPSWeapon
    {
        public ShootTypes shootType;
        public bool firing;
        public bool reloading;
        [Space(10)]


        public GameObject shootPoint;
        public bool shootFromCamera;
        public GameObject camShootPoint;
        [Space(5)]
        public GameObject Bullet;
        public AudioClip fireSound;
        public AudioClip reloadSound;
        [Space(5)]
        public int bulletsPerMag;
        public int bulletsInMag;
        public int totalBullets;
        [Space(10)]


        public ParticleSystem[] muzzleFlashes;
        public GameObject ejectionPoint;
        public GameObject magDropPoint;
        public GameObject Shell;
        public GameObject Mag;
        [Space(10)]

        public float bulletVelocity;
        public float bulletDespawnTime;
        public float shellVelocity;
        public float magVelocity;
        public float shellDespawnTime;
        public float magDespawnTime;
        public float cycleTimeBoltAction;
        public float cycleTimeSemiAuto;


        [Header("Timing")]
        public float reloadTime;
        public float grenadeTime;
        public float fireRate;



        internal bool recoilAuto = false;
        internal bool recoilSemi = false;
        internal bool throwing = false;
        internal bool cycling = false;



        Coroutine lastRoutine = null;

        public enum ShootTypes { SemiAuto, FullAuto, BoltAction };

        void OnEnable()
        {
            //Reset adjuster to sync up every time gun is loaded
            if (WeaponAdjust) WeaponAdjust.enabled = false;
            if (WeaponAdjust) WeaponAdjust.enabled = true;
        }
        void Start()
        {
            if (!HUDController) HUDController = GameObject.FindGameObjectWithTag("HUD").GetComponent<HealthUIController>();

            originalCamPos = mainCam.transform.localPosition;

            //Set the ammo count
            bulletsInMag = bulletsPerMag;
        }

        void Update()
        {
            //Shoot
            if (Input.GetKeyDown(KeyCode.Mouse0) && !firing && reloading == false && bulletsInMag > 0 && !cycling && !swapping)
            {
                firing = true;

                foreach (ParticleSystem ps in muzzleFlashes)
                {
                    ps.Play();
                }

                //Shoot Bullet
                ShootBullet();
                gameObject.GetComponent<AudioSource>().PlayOneShot(fireSound);

                bulletsInMag--;

                if (shootType == ShootTypes.FullAuto)
                {
                    SpawnShell();

                    recoilAuto = true;
                    recoilSemi = false;
                    lastRoutine = StartCoroutine(ShootBulletDelay());
                }
                else if (shootType == ShootTypes.SemiAuto)
                {
                    SpawnShell();

                    recoilAuto = false;
                    recoilSemi = true;

                    if (Weapon == WeaponTypes.Rifle)
                    {
                        Invoke("fireCancel", .25f);
                    }

                    Invoke("cycleFire", cycleTimeSemiAuto);

                    cycling = true;
                }
                else if (shootType == ShootTypes.BoltAction)
                {
                    recoilAuto = false;
                    recoilSemi = true;

                    if (Weapon == WeaponTypes.Rifle)
                    {
                        Invoke("fireCancel", .25f);
                        Invoke("cycleFire", cycleTimeBoltAction);
                        Invoke("ejectShellBoltAction", cycleTimeBoltAction / 2);
                        cycling = true;

                        //gameObject.GetComponent<Animator>().SetBool("cycle", true);
                    }
                }
            }
            else if (firing && (Input.GetKeyUp(KeyCode.Mouse0) || bulletsInMag == 0))
            {
                firing = false;
                recoilSemi = false;
                recoilAuto = false;

                if (shootType == ShootTypes.FullAuto)
                {
                    StopCoroutine(lastRoutine);
                }
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

            //Reload
            if (Input.GetKeyDown(KeyCode.R) && !reloading && !firing && bulletsInMag < bulletsPerMag && totalBullets > 0)
            {
                ArmsAnimator.CrossFade("Reload", 0.1f);
                BodyAnimator.SetBool("Reload", true);

                reloading = true;
                gameObject.GetComponent<AudioSource>().PlayOneShot(reloadSound);
                Invoke("reloadFinished", reloadTime);

                SpawnMag();
            }

            //UI
            if (HUDController) HUDController.uiBullets.text = bulletsInMag.ToString() + "/" + totalBullets;

            //Animators
            ArmsAnimator.SetBool("Shoot", firing);
            ArmsAnimator.SetBool("Sprint", sprinting);
        }



        IEnumerator ShootBulletDelay()
        {
            while (true)
            {
                yield return new WaitForSeconds(fireRate);
                if (bulletsInMag > 0)
                {
                    gameObject.GetComponent<AudioSource>().PlayOneShot(fireSound);
                    foreach (ParticleSystem ps in muzzleFlashes)
                    {
                        ps.Play();
                    }

                    ShootBullet();
                    SpawnShell();
                    bulletsInMag--;
                }
            }
        }
        void ShootBullet()
        {
            GameObject tempBullet;
            if (shootFromCamera)
            {
                //Spawn bullet from the camera shoot point position, not from the true tip of the gun
                tempBullet = Instantiate(Bullet, camShootPoint.transform.position, camShootPoint.transform.rotation) as GameObject;
                tempBullet.GetComponent<RegisterDamageHit>().damage = Damage;
            }
            else
            {
                //Spawn bullet from the shoot point position, the true tip of the gun
                tempBullet = Instantiate(Bullet, shootPoint.transform.position, shootPoint.transform.rotation) as GameObject;
                tempBullet.GetComponent<RegisterDamageHit>().damage = Damage;
            }

            //Orient it
            tempBullet.transform.Rotate(Vector3.left * 90);

            //Add forward force based on where camera is pointing
            Rigidbody tempRigidBody;
            tempRigidBody = tempBullet.GetComponent<Rigidbody>();

            //Always shoot towards where camera is facing
            tempRigidBody.AddForce(mainCam.transform.forward * bulletVelocity);

            //Destroy after time
            Destroy(tempBullet, bulletDespawnTime);
        }
        void SpawnShell()
        {
            //Spawn bullet
            GameObject tempShell;
            tempShell = Instantiate(Shell, ejectionPoint.transform.position, ejectionPoint.transform.rotation) as GameObject;

            //Orient it
            tempShell.transform.Rotate(Vector3.left * 90);

            //Add forward force based on where ejection point is pointing (blue axis)
            Rigidbody tempRigidBody;
            tempRigidBody = tempShell.GetComponent<Rigidbody>();
            tempRigidBody.AddForce(ejectionPoint.transform.forward * shellVelocity);

            //Destroy after time
            Destroy(tempShell, shellDespawnTime);
        }
        void SpawnMag()
        {
            //Spawn bullet
            GameObject tempMag;
            tempMag = Instantiate(Mag, magDropPoint.transform.position, magDropPoint.transform.rotation) as GameObject;

            //Orient it
            tempMag.transform.Rotate(Vector3.left * 90);

            //Add forward force based on where ejection point is pointing (blue axis)
            Rigidbody tempRigidBody;
            tempRigidBody = tempMag.GetComponent<Rigidbody>();
            tempRigidBody.AddForce(magDropPoint.transform.forward * magVelocity);

            //Destroy after time
            Destroy(tempMag, magDespawnTime);
        }


        void cycleFire()
        {
            cycling = false;

            if (shootType == ShootTypes.BoltAction)
            {
                //gameObject.GetComponent<Animator>().SetBool("cycle", false);
            }
        }
        void ejectShellBoltAction()
        {
            SpawnShell();
        }
        void fireCancel()
        {
            firing = false;
        }
        void reloadFinished()
        {
            reloading = false;

            BodyAnimator.SetBool("Reload", false);

            int bulletsToRemove = (bulletsPerMag - bulletsInMag);
            if (totalBullets >= bulletsPerMag)
            {
                bulletsInMag = bulletsPerMag;
                totalBullets -= bulletsToRemove;
            }
            else if (bulletsToRemove <= totalBullets)
            {
                bulletsInMag += bulletsToRemove;
                totalBullets -= bulletsToRemove;
            }
            else
            {
                bulletsInMag += totalBullets;
                totalBullets -= totalBullets;
            }
        }

      
    }
}
