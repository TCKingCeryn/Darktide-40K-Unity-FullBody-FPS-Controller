using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetMaenad.FPS
{
    public class MeleeTrigger : MonoBehaviour
    {
        public FPSPlayerController PlayerController;
        public GameObject RootCharacter;
        public GameObject ForceForwardTransform;
        public Collider Trigger;
        [Space(10)]

        public GameObject DamageCrosshair;
        [Space(5)]
        public LayerMask HitLayers = -1;
        public string[] DamageTags;
        [Space(10)]



        public bool IsBigHit;
        public int Damage;
        public float DamageFrequencyTimer = 2f;
        [Space(5)]
        public float hitVolume = .35f;
        public float hitforce;
        public AudioClip[] hitSounds;
        [Space(10)]



        public GameObject impactParticle;
        public GameObject impactBloodParticle;
        public float impactDespawnTime = 3f;

        internal WaitForSeconds DamageFrequency;
        internal bool CanApplyDamage;
        internal GameObject CurrentObject;


        void Start()
        {
            DamageFrequency = new WaitForSeconds(DamageFrequencyTimer);
        }

        void OnTriggerEnter(Collider other)
        {
            if(RootCharacter && other.gameObject != RootCharacter)
            {
                if ((HitLayers.value & 1 << other.gameObject.layer) == 1 << other.gameObject.layer)
                {
                    //Check Tags
                    if (DamageTags.Length > 0)
                    {
                        for (int i = 0; i < DamageTags.Length; i++)
                        {
                            if (other.transform.CompareTag(DamageTags[i]))
                            {
                                PassDamage(CurrentObject);
                            }
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


                    GameObject tempImpact;
                    tempImpact = Instantiate(impactParticle, this.transform.position, this.transform.rotation) as GameObject;
                    tempImpact.transform.Rotate(Vector3.left * 90);

                    Destroy(tempImpact, impactDespawnTime);

                    if (other.GetComponent<Rigidbody>() && ForceForwardTransform)
                    {
                        other.GetComponent<Rigidbody>().AddForce(ForceForwardTransform.transform.forward * 360 * hitforce);
                    }
                }
            }        
        }


        void OnTriggerStay(Collider other)
        {
            if (RootCharacter && other.gameObject != RootCharacter)
            {
                if ((HitLayers.value & 1 << other.gameObject.layer) == 1 << other.gameObject.layer)
                {
                    //Check Tags
                    if (DamageTags.Length > 0)
                    {
                        for (int i = 0; i < DamageTags.Length; i++)
                        {
                            if (other.transform.CompareTag(DamageTags[i]))
                            {
                                CurrentObject = other.gameObject;

                                if (CanApplyDamage)
                                {
                                  
                                    CanApplyDamage = false;
                                    ResetDamageDelay();                 
                                }
                            }
                        }
                    }

                }
            }
        }


        void OnTriggerExit(Collider other)
        {
            if (RootCharacter && other.gameObject != RootCharacter)
            {
                if ((HitLayers.value & 1 << other.gameObject.layer) == 1 << other.gameObject.layer)
                {
                    //Check Tags
                    if (DamageTags.Length > 0)
                    {
                        for (int i = 0; i < DamageTags.Length; i++)
                        {
                            if (other.transform.CompareTag(DamageTags[i]))
                            {
                                CanApplyDamage = true;
                                StopAllCoroutines();
                            }
                        }
                    }
                }
            }
        }



        IEnumerator ResetDamageDelay()
        {
            while (enabled)
            {
                yield return DamageFrequency;

                CanApplyDamage = true;
            }
        }

        public void PassDamage(GameObject other)
        {

            if (other != null)
            {
                if (other.GetComponent<HealthController>())
                {
                    var rootHealth = other.GetComponent<HealthController>();
                    if (DamageCrosshair && PlayerController) Instantiate(DamageCrosshair, PlayerController.healthControl.HUDController.DamageCrosshairParent.position, DamageCrosshair.transform.rotation, PlayerController.healthControl.HUDController.DamageCrosshairParent);

                    if (!IsBigHit && ForceForwardTransform) rootHealth.Damage(ForceForwardTransform.transform.forward * 360, hitforce, Damage);
                    if (IsBigHit && ForceForwardTransform) rootHealth.DamageBig(ForceForwardTransform.transform.forward * 360, hitforce, Damage);

                    GameObject tempblood;
                    tempblood = Instantiate(impactBloodParticle, this.transform.position, this.transform.rotation) as GameObject;
                    tempblood.transform.Rotate(Vector3.left * 90);

                    Destroy(tempblood, impactDespawnTime);
                }
                if (other.GetComponentInParent<HealthController>())
                {
                    var parentHealth = other.GetComponentInParent<HealthController>();
                    if (DamageCrosshair && PlayerController) Instantiate(DamageCrosshair, PlayerController.healthControl.HUDController.DamageCrosshairParent.position, DamageCrosshair.transform.rotation, PlayerController.healthControl.HUDController.DamageCrosshairParent);

                    if (!IsBigHit && ForceForwardTransform) parentHealth.Damage(ForceForwardTransform.transform.forward * 360, hitforce, Damage);
                    if (IsBigHit && ForceForwardTransform) parentHealth.DamageBig(ForceForwardTransform.transform.forward * 360, hitforce, Damage);

                    GameObject tempblood;
                    tempblood = Instantiate(impactBloodParticle, this.transform.position, this.transform.rotation) as GameObject;
                    tempblood.transform.Rotate(Vector3.left * 90);

                    Destroy(tempblood, impactDespawnTime);
                }


            }

            
        }




    }
}
