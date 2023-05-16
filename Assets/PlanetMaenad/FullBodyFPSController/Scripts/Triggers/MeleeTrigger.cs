using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetMaenad.FPS
{
    public class MeleeTrigger : MonoBehaviour
    {
        public bool IsPlayer;
        public DEMOExamplePlayer PlayerController;
        public GameObject PlayerDamageCrosshair;
        [Space(10)]


        public GameObject RootCharacter;
        public GameObject ForceForwardTransform;
        public Collider Trigger;
        [Space(10)]

        public string hitReaction = "Hit";
        public LayerMask HitLayers = -1;
        public string[] DamageTags;
        [Space(5)]
        public int Damage;
        public float hitforce;
        public float hitVolume = .35f;
        public AudioClip[] hitSounds;
        public AudioClip[] damageSounds;
        [Space(10)]

        public bool IsBigHit;
        public GameObject[] impactParticles;
        public GameObject[] impactBloodParticles;
        public float impactDespawnTime = 3f;



        internal WaitForSeconds DamageFrequency;
        internal bool CanApplyDamage = true;
        internal GameObject CurrentObject;


        void Start()
        {

        }

        void OnTriggerEnter(Collider other)
        {
            if ((HitLayers.value & 1 << other.gameObject.layer) == 1 << other.gameObject.layer)
            {
                CurrentObject = other.gameObject;
                //Debug.Log("Hit Object: " + CurrentObject.gameObject.name);


                //Check Tags
                if (DamageTags.Length > 0)
                {
                    for (int i = 0; i < DamageTags.Length; i++)
                    {
                        if (other.transform.CompareTag(DamageTags[i]))
                        {
                            PassDamage(CurrentObject, transform.position);
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


                for (int i = 0; i < impactParticles.Length; i++)
                {
                    GameObject tempImpact;
                    tempImpact = Instantiate(impactParticles[i], this.transform.position, this.transform.rotation) as GameObject;
                    tempImpact.transform.Rotate(Vector3.left * 90);

                    Destroy(tempImpact, impactDespawnTime);

                    if (other.GetComponent<Rigidbody>() && IsPlayer && ForceForwardTransform)
                    {
                        other.GetComponent<Rigidbody>().AddForce(ForceForwardTransform.transform.forward * 360 * hitforce);
                    }
                }
                
            }
        }
       
        


        public void PassDamage(GameObject other, Vector3 damagePoint)
        {
            if (other != null)
            {
                if (other.GetComponentInParent<HealthController>())
                {
                    var parentHealth = other.GetComponentInParent<HealthController>();
                    if (parentHealth.gameObject != RootCharacter.gameObject)
                    {
                        if (IsPlayer)
                        {
                            if (PlayerDamageCrosshair && PlayerController)
                            {
                                Instantiate(PlayerDamageCrosshair, PlayerController.DamageCrosshairHolder.position, PlayerDamageCrosshair.transform.rotation, PlayerController.DamageCrosshairHolder);
                            }
                        }

                        if (parentHealth.CharacterAnimator && hitReaction != null && !parentHealth.CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName(hitReaction)) parentHealth.CharacterAnimator.Play(hitReaction);

                        if (!IsBigHit && ForceForwardTransform) parentHealth.Damage(ForceForwardTransform.transform.forward * 360, hitforce, Damage);
                        if (IsBigHit && ForceForwardTransform) parentHealth.DamageBig(ForceForwardTransform.transform.forward * 360, hitforce, Damage);


                        for (int i = 0; i < impactBloodParticles.Length; i++)
                        {
                            GameObject tempblood;
                            tempblood = Instantiate(impactBloodParticles[i], damagePoint, impactBloodParticles[i].transform.rotation) as GameObject;
                            tempblood.transform.Rotate(Vector3.left * 90);

                            Destroy(tempblood, impactDespawnTime);
                        }


                        if (damageSounds.Length > 0)
                        {
                            float random = Random.Range(0f, 1f);

                            if (random <= 0.3f)
                            {
                                int randomIndex = Random.Range(0, damageSounds.Length);
                                AudioSource.PlayClipAtPoint(damageSounds[randomIndex], transform.position, hitVolume);
                            }
                        }

                    }                    
                }              
            }       
        }
    }
}
