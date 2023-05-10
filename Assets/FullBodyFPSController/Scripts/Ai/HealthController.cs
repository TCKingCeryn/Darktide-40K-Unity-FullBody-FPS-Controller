using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace scgFullBodyController
{
    [Tooltip("IMPORTANT, this script needs to be on the root transform")]
    public class HealthController : MonoBehaviour
    {
        public bool isAI;
        [Space(10)]

        public Animator CharacterAnimator;
        public HealthUIController HUDController;
        public float health;
        public bool regen;
        public float timeBeforeRegen;
        public float regenSpeed;
        public UnityEvent OnReceiveDamage;
        [Space(10)]

        public bool playNoiseOnHurt;
        public float percentageToPlay;
        public AudioSource hurtSource;
        public AudioClip hurtNoise;
        [Space(10)]

        public bool DestroyOnDead = true;
        public float deadTime;
        public bool dontSpawnRagdoll;
        public GameObject ragdoll;
        public UnityEvent OnDeath;
        [Space(10)]


        public bool RespawnOnDead;
        public float respawnTime = 5f;
        public UnityEvent OnRespawn;

        GameObject tempdoll;
        bool meleeDeath;
        bool alreadyRegenning;

        float origTimeBeforeRegen;
        float maxHealth;
        Vector2 startingPos;

        void Start()
        {
            startingPos = transform.position;

            if(!HUDController && !isAI) HUDController = GameObject.FindGameObjectWithTag("HUD").GetComponent<HealthUIController>();

            //Get a reference to the original reset time
            origTimeBeforeRegen = timeBeforeRegen;

            //Set maxHealth to what our max is at start of the scene
            maxHealth = health;
        }
        void Update()
        {
            //If health is low enough and we are not kicked to death, then die normally
            if (health <= 0)
            {
                if (!meleeDeath)
                {
                    Die();
                }
            }

            //Only update HUD text if exists
            if (health > 0)
            {
                if (HUDController) HUDController.uiHealth.text = health.ToString();
                if (HUDController) HUDController.uiHealthSlider.value = health;
            }
            else
            {
                if (HUDController) HUDController.uiHealth.text = "0";
                if (HUDController) HUDController.uiHealthSlider.value = 0;
            }


            //Check if we are done regenning and stop
            if (health == maxHealth && regen && alreadyRegenning)
            {
                alreadyRegenning = false;
                StopCoroutine("regenHealth");
            }

        }


        public void Damage(float damage)
        {
            OnReceiveDamage.Invoke();

            //If we are a player, take damage, otherwise (AI), apply the hit animation and attack the player
            if (!isAI)
            {
                health -= damage;

                if (playNoiseOnHurt)
                {
                    if (Random.value < percentageToPlay)
                    {
                       if (hurtSource) hurtSource.PlayOneShot(hurtNoise);
                    }
                }
            }
            else
            {
                health -= damage;

                if (CharacterAnimator) CharacterAnimator.Play("Hit");

                if (gameObject.GetComponent<AIController>())
                {
                    gameObject.GetComponent<AIController>().overrideAttack = true;
                }

                if (playNoiseOnHurt)
                {
                    if (Random.value < percentageToPlay)
                    {
                        GetComponent<AudioSource>().PlayOneShot(hurtNoise);
                    }
                }

            }

            //If we are allowed to regen, start gaining health
            if (regen)
            {
                timeBeforeRegen = origTimeBeforeRegen;
                StopCoroutine("regenHealth");
                CancelInvoke();

                if (timeBeforeRegen == origTimeBeforeRegen)
                {
                    alreadyRegenning = true;
                    Invoke(nameof(regenEnumeratorStart), timeBeforeRegen);
                }
            }
        }
        public void Damage(Vector3 pos, float Force, float damage)
        {
            OnReceiveDamage.Invoke();

            //Subtract the damage from values passed in by kickSensing
            health -= damage;

            //If kicked enough, then die
            if (health <= 0)
            {
                meleeDeath = true;

                if (!tempdoll)
                {
                    tempdoll = Instantiate(ragdoll, this.transform.position, this.transform.rotation) as GameObject;
                    //tempdoll.GetComponent<OverrideCameraParent>().isAi = isAI;

                }

                if (DestroyOnDead) Destroy(gameObject);

                foreach (Rigidbody rb in tempdoll.GetComponentsInChildren<Rigidbody>())
                {
                    rb.AddForce(pos * Force);
                }
            }
            else
            {
                //Dont die just play hit anim
                if (CharacterAnimator) CharacterAnimator.Play("Hit");
            }
        }
        public void DamageByKick(Vector3 pos, float kickForce, int kickDamage)
        {
            OnReceiveDamage.Invoke();

            //Subtract the damage from values passed in by kickSensing
            health -= kickDamage;

            //If kicked enough, then die
            if (health <= 0)
            {
                meleeDeath = true;

                if(!tempdoll)
                {
                    tempdoll = Instantiate(ragdoll, this.transform.position, this.transform.rotation) as GameObject;
                    //tempdoll.GetComponent<OverrideCameraParent>().isAi = isAI;

                }

                if (DestroyOnDead) Destroy(gameObject);

                foreach (Rigidbody rb in tempdoll.GetComponentsInChildren<Rigidbody>())
                {
                    rb.AddForce(pos * kickForce);
                }
            }
            else
            {
                //Dont die just play hit anim
                if (CharacterAnimator) CharacterAnimator.Play("KickHit");
            }
        }

        public void Die()
        {
            OnDeath.Invoke();
            if (RespawnOnDead) StartCoroutine(RespawnDelay());

            //Only spawn ragdoll if option is selected
            if (!dontSpawnRagdoll)
            {
                if (!tempdoll)
                {
                    //Spawn ragdoll and destroy us
                    tempdoll = Instantiate(ragdoll, this.transform.position, this.transform.rotation) as GameObject;

                    //Tell the ragdoll if we are a player or not so it knows to move our camera or not to the ragdoll
                    //tempdoll.GetComponent<OverrideCameraParent>().isAi = isAI;
                }

                if (DestroyOnDead) Destroy(gameObject);
            }
            if (isAI)
            {
                //If we aren't spawning a ragdoll, then disable all important scripts on us and destroy after deadtime seconds
                //This feature is for AI with the ragdoll built in for a more realistic death
                if (CharacterAnimator)
                    CharacterAnimator.enabled = false;

                if (gameObject.GetComponent<AIController>())
                    gameObject.GetComponent<AIController>().enabled = false;

                if (gameObject.GetComponent<HealthController>())
                    gameObject.GetComponent<HealthController>().enabled = false;

                if (gameObject.GetComponent<SimpleFootsteps>())
                    gameObject.GetComponent<SimpleFootsteps>().enabled = false;

                if (gameObject.GetComponent<NavMeshAgent>())
                    gameObject.GetComponent<NavMeshAgent>().enabled = false;

                if (gameObject.GetComponentInChildren<OffsetYRotation>())
                    gameObject.GetComponentInChildren<OffsetYRotation>().enabled = false;

                if (gameObject.GetComponentInChildren<AIShooterWeapon>())
                    gameObject.GetComponentInChildren<AIShooterWeapon>().enabled = false;

                if (gameObject.GetComponentInChildren<WeaponAdjuster>())
                    gameObject.GetComponentInChildren<WeaponAdjuster>().enabled = false;


                if (DestroyOnDead) Destroy(gameObject, 30f);
            }
        }
        IEnumerator RespawnDelay()
        {
            yield return new WaitForSeconds(respawnTime);

            health = maxHealth;
            OnRespawn.Invoke();

            if (tempdoll) 
            {
                Destroy(tempdoll);
                tempdoll = null;
            }

            transform.position = startingPos;
        }


        void regenEnumeratorStart()
        {
            StartCoroutine("regenHealth");
        }
        IEnumerator regenHealth()
        {
            //Only regen while under max health and gain 1 health every regenSpeed seconds
            while (health < maxHealth)
            {
                health++;
                yield return new WaitForSeconds(regenSpeed);
            }
        }

    }
}