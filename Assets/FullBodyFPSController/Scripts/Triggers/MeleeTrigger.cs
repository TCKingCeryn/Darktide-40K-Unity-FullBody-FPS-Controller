using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace scgFullBodyController
{
    public class MeleeTrigger : MonoBehaviour
    {
        public Animator RootAnimator;
        public GameObject cameraObj;
        [Space(5)]

        public bool IsKick;
        public int Damage;
        public LayerMask HitLayers = -1;
        public string[] DamageTags;
        [Space(10)]


        public float hitforce;
        public AudioClip hitSound;
        [Space(10)]

        public GameObject impactParticle;
        public GameObject impactBloodParticle;
        public float impactDespawnTime = 3f;


        void OnTriggerEnter(Collider other)
        {
            if(other.gameObject != RootAnimator.gameObject)
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
                                if (other.transform.root.GetComponent<HealthController>())
                                {
                                    if (!IsKick) other.transform.root.GetComponent<HealthController>().Damage(cameraObj.transform.forward * 360, hitforce, Damage);
                                    if (IsKick) other.transform.root.GetComponent<HealthController>().DamageByKick(cameraObj.transform.forward * 360, hitforce, Damage);

                                    //Spawn blood 
                                    var tempblood = Instantiate(impactBloodParticle, this.transform.position, this.transform.rotation) as GameObject;
                                    tempblood.transform.Rotate(Vector3.left * 90);

                                    Destroy(tempblood, impactDespawnTime);
                                }
                            }
                        }
                    }


                    //Spawn Basic Impact 
                    var tempImpact = Instantiate(impactParticle, this.transform.position, this.transform.rotation) as GameObject;
                    tempImpact.transform.Rotate(Vector3.left * 90);

                    Destroy(tempImpact, impactDespawnTime);

                    if (other.GetComponent<Rigidbody>())
                    {
                        other.GetComponent<Rigidbody>().AddForce(cameraObj.transform.forward * 360 * hitforce);
                    }
                }
            }        
        }
    }
}
