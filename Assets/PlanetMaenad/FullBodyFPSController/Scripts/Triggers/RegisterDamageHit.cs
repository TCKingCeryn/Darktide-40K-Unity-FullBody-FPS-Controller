using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetMaenad.FPS
{
    public class RegisterDamageHit : MonoBehaviour
    {
        public FPSPlayerController PlayerController;
        public LayerMask HitLayers = -1;
        public string[] DamageTags;
        [Space(10)]

        public GameObject DamageCrosshair;
        [Space(5)]


        public int damage;
        public float hitVolume = .35f;
        public float hitForce = 5f;
        public AudioClip[] hitSounds;
        [Space(10)]


        public bool AutoDestroy;
        public GameObject impactParticle;
        public GameObject impactBloodParticle;
        public float impactDespawnTime = 3f;





        void OnCollisionEnter(Collision other)
        {
            //Check Layers
            if ((HitLayers.value & 1 << other.gameObject.layer) == 1 << other.gameObject.layer)
            {
                //Check Tags
                if (DamageTags.Length > 0)
                {
                    for (int i = 0; i < DamageTags.Length; i++)
                    {
                        if (other.transform.CompareTag(DamageTags[i]))
                        {
                            PassDamage(other.gameObject);
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
                if (AutoDestroy == true) Destroy(gameObject);
            }
        }



        public void PassDamage(GameObject other)
        {
            if (other.GetComponentInParent<HealthController>())
            {
                var parentHealth = other.GetComponentInParent<HealthController>();
                parentHealth.Damage(transform.forward * 360, hitForce, damage);

                if (DamageCrosshair && PlayerController) Instantiate(DamageCrosshair, PlayerController.healthControl.HUDController.DamageCrosshairParent.position, DamageCrosshair.transform.rotation, PlayerController.healthControl.HUDController.DamageCrosshairParent);

                //Spawn blood 
                GameObject tempImpact;
                tempImpact = Instantiate(impactBloodParticle, this.transform.position, this.transform.rotation) as GameObject;
                tempImpact.transform.Rotate(Vector3.left * 90);

                Destroy(tempImpact, impactDespawnTime);
            }

        }



    }

}
