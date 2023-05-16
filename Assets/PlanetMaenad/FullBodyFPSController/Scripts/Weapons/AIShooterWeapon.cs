using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlanetMaenad.FPS
{
    [RequireComponent(typeof(AudioSource))]
    public class AIShooterWeapon : AIWeapon
    {
        public ShootTypes shootType;
        public bool reloading = false;
        public bool firing = false;
        public bool aiming = false;
        [Space(10)]


        public GameObject shootPoint;
        public LayerMask HitLayers = -1;
        public string[] DamageTags;
        [Space(5)]
        public bool UseRigidbodyBullet;
        public GameObject Bullet;
        public float bulletVelocity;
        public float bulletDespawnTime;
        [Space(10)]

        public float hitVolume = .35f;
        public float hitForce = 5f;
        public AudioClip[] hitSounds;
        [Space(10)]


        public int bulletsPerMag;
        public int bulletsInMag;
        public int totalBullets;
        [Space(5)]
        public float reloadTime;
        public float grenadeTime;
        public float fireRate;
        [Space(10)]



        public float ShootVolume = .35f;
        public AudioClip fireSound;
        public float ReloadVolume = 0.35f;
        public AudioClip reloadSound;
        [Space(10)]


        public ParticleSystem[] muzzleFlashes;
        public GameObject ejectionPoint;
        public GameObject magDropPoint;
        public GameObject Shell;
        public GameObject Mag;
        [Space(10)]

   
        public float shellVelocity;
        public float magVelocity;
        public float shellDespawnTime;
        public float magDespawnTime;
        public float cycleTimeBoltAction;
        public float cycleTimeSemiAuto;
        [Space(10)]



        public bool AutoDestroyImpacts;
        public GameObject[] impactParticles;
        public GameObject[] impactBloodParticles;
        public float impactDespawnTime = 3f;




        internal bool throwing = false;
        internal bool cycling = false;
        internal Vector3 currentShootPoint;
        internal Coroutine lastRoutine = null;
      
        public enum ShootTypes { SemiAuto, FullAuto, BoltAction };



        void OnEnable()
        {
            //Reset adjuster to sync up every time gun is loaded
            WeaponAdjust.enabled = false;
            WeaponAdjust.enabled = true;
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
                gameObject.GetComponent<AudioSource>().PlayOneShot(fireSound, ShootVolume);
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
            {
                if (lastRoutine != null) StopCoroutine(lastRoutine);
            }

        }
        public void Reload()
        {
            if (!reloading && !firing && bulletsInMag < bulletsPerMag && totalBullets > 0)
            {
                if (AIAnimator) AIAnimator.SetBool("Reload", true);
                reloading = true;
                gameObject.GetComponent<AudioSource>().PlayOneShot(reloadSound, ReloadVolume);
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
        void reloadFinished()
        {
            reloading = false;
            if (AIAnimator) AIAnimator.SetBool("Reload", false);
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

        IEnumerator shootBullet()
        {
            while (true)
            {
                yield return new WaitForSeconds(fireRate);

                if (bulletsInMag > 0)
                {
                    gameObject.GetComponent<AudioSource>().PlayOneShot(fireSound, ShootVolume);

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

        void spawnBullet()
        {
            if (!UseRigidbodyBullet)
            {
                Vector3 rayOrigin = shootPoint.transform.position;
                RaycastHit hit;
                var rayDirection = currentShootPoint - shootPoint.transform.position;

                //Hits an Object
                if (Physics.Raycast(rayOrigin, rayDirection, out hit, 1000 + 1, HitLayers))
                {
                    var hitTransform = hit.collider.transform;

                    for (int i = 0; i < DamageTags.Length; i++)
                    {       
                        //Can See Target
                        if (hitTransform.CompareTag(DamageTags[i]))
                        {
                            PassDamage(hitTransform.gameObject, hit.point);
                        }
                    }

                    if (hitSounds.Length > 0)
                    {
                        float random = Random.Range(0f, 1f);

                        if (random <= 0.3f)
                        {
                            int randomIndex = Random.Range(0, hitSounds.Length);
                            AudioSource.PlayClipAtPoint(hitSounds[randomIndex], transform.position, hitVolume);
                        }
                    }


                    for (int i = 0; i < impactParticles.Length; i++)
                    {
                        GameObject tempImpact;
                        tempImpact = Instantiate(impactParticles[i], hit.point, impactParticles[i].transform.rotation) as GameObject;
                        tempImpact.transform.Rotate(Vector3.left * 90);

                        Destroy(tempImpact, impactDespawnTime);

                        if (hitTransform.GetComponent<Rigidbody>())
                        {
                            hitTransform.GetComponent<Rigidbody>().AddForce(shootPoint.transform.forward * 1 * hitForce);
                        }
                    }
                   
                }
            }

            if (UseRigidbodyBullet && Bullet)
            {
                GameObject tempBullet;
                tempBullet = Instantiate(Bullet, shootPoint.transform.position, shootPoint.transform.rotation) as GameObject;
                tempBullet.GetComponent<RegisterDamageHit>().Damage = Damage;

                tempBullet.transform.Rotate(Vector3.left * 90);



                //Add forward force based on where camera is pointing
                Rigidbody tempRigidBody;
                tempRigidBody = tempBullet.GetComponent<Rigidbody>();

                //Always shoot towards where camera is facing
                tempRigidBody.AddForce(shootPoint.transform.forward * bulletVelocity);

                //Destroy after time
                Destroy(tempBullet, bulletDespawnTime);
            }
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


        public void PassDamage(GameObject other, Vector3 damagePoint)
        {
            if (other.GetComponentInParent<HealthController>())
            {
                var parentHealth = other.GetComponentInParent<HealthController>();
                parentHealth.Damage(transform.forward * 360, hitForce, Damage);

                for (int i = 0; i < impactBloodParticles.Length; i++)
                {
                    GameObject tempImpact;
                    tempImpact = Instantiate(impactBloodParticles[i], damagePoint, impactBloodParticles[i].transform.rotation) as GameObject;
                    tempImpact.transform.Rotate(Vector3.left * 90);

                    Destroy(tempImpact, impactDespawnTime);
                }
                
            }

        }

    }
}
