using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace scgFullBodyController
{
    [RequireComponent(typeof(AudioSource))]
    public class AIShooterWeapon : MonoBehaviour
    {
        public Animator AIAnimator;
        [Space(10)]


        public float MoveSetID = 1;
        public WeaponTypes Weapon;
        public ShootTypes shootType;
        public bool reloading = false;
        public bool firing = false;
        public bool aiming = false;
        [Space(10)]




        public GameObject shootPoint;
        [Space(5)]
        public int Damage;
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





        internal bool throwing = false;
        internal bool cycling = false;

        Coroutine lastRoutine = null;

        public enum WeaponTypes { Rifle, Pistol };
        public enum ShootTypes { SemiAuto, FullAuto, BoltAction };




        void OnEnable()
        {
            if (Weapon == WeaponTypes.Rifle)
            {
                AIAnimator.SetLayerWeight(1, 1);
                AIAnimator.SetLayerWeight(2, 0);
                AIAnimator.SetLayerWeight(4, 0.4f);
                AIAnimator.SetLayerWeight(5, 0);
            }
            else if (Weapon == WeaponTypes.Pistol)
            {
                AIAnimator.SetLayerWeight(1, 0);
                AIAnimator.SetLayerWeight(2, 1);
                AIAnimator.SetLayerWeight(4, 0);
                AIAnimator.SetLayerWeight(5, 0.4f);
            }

            //Reset adjuster to sync up every time gun is loaded
            gameObject.GetComponent<WeaponAdjuster>().enabled = false;
            gameObject.GetComponent<WeaponAdjuster>().enabled = true;
        }
        void Start()
        {
            //Set the ammo count
            bulletsInMag = bulletsPerMag;
        }

        void Update()
        {
            if (bulletsInMag == 0 && !reloading)
            {
                firing = false;
                Reload();
            }

            //AIAnimators
            AIAnimator.SetBool("Fire", firing);

            //AIAnimators
            AIAnimator.SetBool("Fire", firing);
        }

        public void Fire()
        {
            //Input and actions for shooting
            if (!firing && reloading == false && bulletsInMag > 0 && !cycling)
            {
                firing = true;
                foreach (ParticleSystem ps in muzzleFlashes)
                {
                    ps.Play();
                }
                gameObject.GetComponent<AudioSource>().PlayOneShot(fireSound);
                spawnBullet();
                bulletsInMag--;

                if (shootType == ShootTypes.FullAuto)
                {
                    spawnShell();
                    lastRoutine = StartCoroutine(shootBullet());
                }
                else if (shootType == ShootTypes.SemiAuto)
                {
                    spawnShell();

                    if (Weapon == WeaponTypes.Rifle)
                    {
                        Invoke("fireCancel", .25f);
                    }
                    Invoke("cycleFire", cycleTimeSemiAuto);
                    cycling = true;
                }
                else if (shootType == ShootTypes.BoltAction)
                {

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
        }
        public void FireCancel()
        {
            firing = false;
            foreach (ParticleSystem ps in muzzleFlashes)
            {
                ps.Stop();
            }
            if (shootType == ShootTypes.FullAuto)
                StopCoroutine(lastRoutine);
        }
        public void Reload()
        {
            if (!reloading && !firing && bulletsInMag < bulletsPerMag && totalBullets > 0)
            {
                AIAnimator.SetBool("Reload", true);
                reloading = true;
                gameObject.GetComponent<AudioSource>().PlayOneShot(reloadSound);
                Invoke("reloadFinished", reloadTime);
                spawnMag();
                if (Weapon == WeaponTypes.Rifle)
                {
                    //gameObject.GetComponent<Animator>().SetBool("reloading", true);
                }
            }

        }

        void cycleFire()
        {
            cycling = false;

            //if (shootType == ShootTypes.BoltAction)
                //gameObject.GetComponent<Animator>().SetBool("cycle", false);
        }
        void ejectShellBoltAction()
        {
            spawnShell();
        }
        void fireCancel()
        {
            firing = false;
        }


        IEnumerator shootBullet()
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
                    spawnBullet();
                    spawnShell();
                    bulletsInMag--;
                }
            }
        }

        void reloadFinished()
        {
            reloading = false;
            AIAnimator.SetBool("Reload", false);
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

            if (Weapon == WeaponTypes.Rifle)
            {
                //gameObject.GetComponent<Animator>().SetBool("reloading", false);
            }
        }

        void spawnBullet()
        {
            GameObject tempBullet;
            //Spawn bullet from the shoot point position, the true tip of the gun
            tempBullet = Instantiate(Bullet, shootPoint.transform.position, shootPoint.transform.rotation) as GameObject;
            tempBullet.GetComponent<RegisterDamageHit>().damage = Damage;

            //Orient it
            tempBullet.transform.Rotate(Vector3.left * 90);

            //Add forward force based on where camera is pointing
            Rigidbody tempRigidBody;
            tempRigidBody = tempBullet.GetComponent<Rigidbody>();

            //Always shoot towards where camera is facing
            tempRigidBody.AddForce(shootPoint.transform.forward * bulletVelocity);

            //Destroy after time
            Destroy(tempBullet, bulletDespawnTime);
        }
        void spawnShell()
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
        void spawnMag()
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
    }
}
