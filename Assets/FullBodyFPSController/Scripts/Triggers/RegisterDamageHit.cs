//SlapChickenGames
//2021
//Bullet hit registration

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace scgFullBodyController
{
    public class RegisterDamageHit : MonoBehaviour
    {
        public LayerMask HitLayers = -1;
        public string[] DamageTags;
        [Space(10)]


        public bool AutoDestroy;
        public GameObject impactParticle;
        public GameObject impactBloodParticle;
        public float impactDespawnTime = 3f;

        public int damage;



        public void PassDamage(GameObject other)//, bool autoDestroy)
        {
            //If the root object we hit has a healthcontroller then apply damage
            if (other.transform.root.gameObject.GetComponent<HealthController>())
            {
                other.transform.root.gameObject.GetComponent<HealthController>().Damage(damage);

                //Spawn blood 
                GameObject tempImpact;
                tempImpact = Instantiate(impactBloodParticle, this.transform.position, this.transform.rotation) as GameObject;
                tempImpact.transform.Rotate(Vector3.left * 90);

                Destroy(tempImpact, impactDespawnTime);
            }

            //Finally, destroy us (the bullet)
            //if (autoDestroy == true) Destroy(gameObject);
        }

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
                            PassDamage(other.gameObject);//, AutoDestroy ? true : false);
                        }
                    }
                }


                //Spawn Basic Impact 
                GameObject tempImpact;
                tempImpact = Instantiate(impactParticle, this.transform.position, this.transform.rotation) as GameObject;
                tempImpact.transform.Rotate(Vector3.left * 90);

                Destroy(tempImpact, impactDespawnTime);

                //Finally, destroy us (the bullet)
                if (AutoDestroy == true) Destroy(gameObject);
            }
        }

    }

}
