using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlanetMaenad.FPS
{
    [RequireComponent(typeof(AudioSource))]
    public class FPSShooterWeapon : FPSWeapon
    {
        public ShootTypes shootType;
        public bool firing;
        public bool reloading;
        [Space(10)]


        public GameObject DamageCrosshair;
        [Space(10)]



        public bool shootFromCamera;
        [Space(5)]
        public GameObject shootPoint;
        public string hitReaction = "Hit";
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
        public TextMeshPro CurrentAmmoTextMesh;
        public TextMeshPro MaxAmmoTextMesh;
        public int totalBullets;
        [Space(5)]
        public float reloadTime = 0.2f;
        public float grenadeTime;
        public float fireRate = 0.1f;
        [Space(10)]


        public bool UseRecoil;
        public float recoilAmount = .2f;
        public float recoilDuration = .5f;
        public float returnSpeed = .5f;
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
        [Space(10)]




        internal Vector3 originalPosition;
        internal Quaternion originalRotation;
        internal bool isRecoiling;
        internal float recoilTimer;
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

            if (UseFPSArmsConstant)
            {
                PlayerArmsController.LockFullbodyArms = true;
            }
            if (!UseFPSArmsConstant)
            {
                PlayerArmsController.LockFullbodyArms = false;
            }

            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
        }
        void Start()
        {
            originalCamPos = new Vector3(0, .35f, 0.15f);

            //Set the ammo count
            bulletsInMag = bulletsPerMag;
        }

        void Update()
        {
            //Shoot
            if (Input.GetKeyDown(AttackButton) && !PlayerController.IsBlocking && !firing && reloading == false && bulletsInMag > 0 && !cycling && !swapping)
            {
                if (UseFPSArmsOnAttack && !UseFPSArmsConstant)
                {
                    PlayerArmsController.LockFullbodyArms = true;
                }

                firing = true;

                foreach (ParticleSystem ps in muzzleFlashes)
                {
                    ps.Play();
                }

                //Shoot Bullet
                ShootBullet();
                gameObject.GetComponent<AudioSource>().PlayOneShot(fireSound, ShootVolume);

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
            else if (firing && (Input.GetKeyUp(AttackButton) || bulletsInMag == 0))
            {
                if (UseFPSArmsOnAttack && !UseFPSArmsConstant)
                {
                    PlayerArmsController.LockFullbodyArms = false;
                }

                firing = false;
                recoilSemi = false;
                recoilAuto = false;

                if (shootType == ShootTypes.FullAuto)
                {
                    StopCoroutine(lastRoutine);
                }
            }

            //Sprint
            if (Input.GetKey(KeyCode.LeftShift) && !PlayerController.IsBlocking && !aiming)
            {
                sprinting = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                sprinting = false;
            }

            //Reload
            if (Input.GetKeyDown(KeyCode.R) && !PlayerController.IsBlocking && !reloading && !firing && bulletsInMag < bulletsPerMag && totalBullets > 0)
            {
                PlayerController.ArmsAnimator.CrossFade("Reload", 0.1f);
                PlayerController.BodyAnimator.SetBool("Reload", true);

                reloading = true;
                gameObject.GetComponent<AudioSource>().PlayOneShot(reloadSound, ReloadVolume);
                Invoke("reloadFinished", reloadTime);

                SpawnMag();
            }

            //UI
            if (CurrentAmmoTextMesh) CurrentAmmoTextMesh.text = bulletsInMag.ToString();
            if (MaxAmmoTextMesh) MaxAmmoTextMesh.text = totalBullets.ToString();


            //Animators
            PlayerController.ArmsAnimator.SetBool("Shoot", firing);
            PlayerController.ArmsAnimator.SetBool("Sprint", sprinting);
        }

        IEnumerator ShootBulletDelay()
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

                    ShootBullet();
                    SpawnShell();
                    bulletsInMag--;
                }
            }
        }
        void ShootBullet()
        {
            if (!UseRigidbodyBullet)
            {
                RaycastHit hit;
                var rayDirection = shootFromCamera ? mainCam.transform.forward : shootPoint.transform.forward;

                //Hits an Object
                if (Physics.Raycast(shootFromCamera ? mainCam.transform.position : shootPoint.transform.position, rayDirection, out hit, 1000 + 1, HitLayers))
                {
                    var hitTransform = hit.collider.transform;
                    //Debug.Log("Hit Object: " + hitTransform.gameObject.name);

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
                            hitTransform.GetComponent<Rigidbody>().AddForce(mainCam.transform.forward * 1 * hitForce);
                        }
                    }
                   
                }
            }

            if (UseRigidbodyBullet && Bullet)
            {
                GameObject tempBullet;
                if (shootFromCamera)
                {

                    tempBullet = Instantiate(Bullet, mainCam.transform.position, mainCam.transform.rotation) as GameObject;
                    tempBullet.GetComponent<RegisterDamageHit>().Damage = Damage;
                }
                else
                {
                    //Spawn bullet from the shoot point position, the true tip of the gun
                    tempBullet = Instantiate(Bullet, shootPoint.transform.position, shootPoint.transform.rotation) as GameObject;
                    tempBullet.GetComponent<RegisterDamageHit>().Damage = Damage;
                }

                if (PlayerController) tempBullet.GetComponent<RegisterDamageHit>().PlayerController = PlayerController;

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


        public void PassDamage(GameObject other, Vector3 damagePoint)
        {
            if (other.GetComponentInParent<HealthController>())
            {
                var parentHealth = other.GetComponentInParent<HealthController>();
                parentHealth.Damage(transform.forward * 360, hitForce, Damage);

                if (DamageCrosshair && PlayerController)
                {
                    Instantiate(DamageCrosshair, PlayerController.DamageCrosshairHolder.position, DamageCrosshair.transform.rotation, PlayerController.DamageCrosshairHolder);
                }

                if (parentHealth.CharacterAnimator && hitReaction != null && !parentHealth.CharacterAnimator.GetCurrentAnimatorStateInfo(1).IsName(hitReaction) && !parentHealth.CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName(hitReaction)) parentHealth.CharacterAnimator.Play(hitReaction);

                for (int i = 0; i < impactBloodParticles.Length; i++)
                {
                    GameObject tempImpact;
                    tempImpact = Instantiate(impactBloodParticles[i], damagePoint, impactBloodParticles[i].transform.rotation) as GameObject;
                    tempImpact.transform.Rotate(Vector3.left * 90);

                    Destroy(tempImpact, impactDespawnTime);
                }
               
            }

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

            PlayerController.BodyAnimator.SetBool("Reload", false);

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
