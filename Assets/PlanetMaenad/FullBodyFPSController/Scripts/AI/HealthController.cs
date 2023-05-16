using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace PlanetMaenad.FPS
{
    [Tooltip("IMPORTANT, this script needs to be on the root transform")]
    public class HealthController : MonoBehaviour
    {
        public bool isAI;
        [Space(10)]


        public Animator CharacterAnimator;
        public HealthUIController HUDController;
        public float health;
        public float fillAmount;
        [Space(10)]


        public bool regen;
        public float timeBeforeRegen;
        public float regenSpeed;
        public UnityEvent OnReceiveDamage;
        [Space(10)]

        public bool IsBlocking;
        [Space(5)]
        public bool playNoiseOnHurt;
        public float percentageToPlay;
        public AudioSource hurtSource;
        public AudioClip[] hurtNoises;
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
        bool isDead;
        bool alreadyRegenning;

        float origTimeBeforeRegen;
        float maxHealth;
        Vector2 startingPos;


        MaterialPropertyBlock matBlock;
        MeshRenderer meshRenderer;


        void Start()
        {
            if (!HUDController && !isAI) HUDController = GameObject.FindGameObjectWithTag("HUD").GetComponent<HealthUIController>();
            if (HUDController && HUDController.HealthMeshFill) meshRenderer = HUDController.HealthMeshFill;

            matBlock = new MaterialPropertyBlock();

            startingPos = transform.position;

            origTimeBeforeRegen = timeBeforeRegen;
            maxHealth = health;
        }
        void Update()
        {
            var healthPercentage = (health < 0 ? 0 : ((health / maxHealth) * 100f) / 100f);        
            fillAmount = healthPercentage;
            if (HUDController && HUDController.HealthMeshFill) UpdateHealthFillMesh(fillAmount < 0 ? 0 : fillAmount);


            //If health is low enough and we are not kicked to death, then die normally
            if (health <= 0)
            {
                if(!isDead)
                {
                    Die();
                }
            }

            //Only update HUD text if exists
            if (health > 0)
            {
                if (HUDController && HUDController.uiHealth) HUDController.uiHealth.text = health.ToString();
                if (HUDController && HUDController.uiHealthSlider) HUDController.uiHealthSlider.value = health;
            }
            else
            {
                if (HUDController && HUDController.uiHealth) HUDController.uiHealth.text = "0";
                if (HUDController && HUDController.uiHealthSlider) HUDController.uiHealthSlider.value = 0;
            }


            //Check if we are done regenning and stop
            if (health == maxHealth && regen && alreadyRegenning)
            {
                alreadyRegenning = false;
                StopCoroutine("regenHealth");
            }

        }

        public void UpdateHealthFillMesh(float value)
        {
            //renderer.sharedMaterial.SetFloat("_HealthFill", value);

            meshRenderer.GetPropertyBlock(matBlock);

            matBlock.SetFloat("_Fill", value);

            meshRenderer.SetPropertyBlock(matBlock);

            if (HUDController && HUDController.HealthTextMesh) HUDController.HealthTextMesh.text = health.ToString();
        }


        public void Damage(Vector3 pos, float Force, float damage)
        {
            if(!IsBlocking)
            {
                OnReceiveDamage.Invoke();

                if (playNoiseOnHurt)
                {
                    if (hurtNoises.Length > 0)
                    {
                        float random = Random.Range(0f, 1f);

                        if (random <= 0.3f)
                        {
                            int randomIndex = Random.Range(0, hurtNoises.Length);

                            if (hurtSource) hurtSource.clip = hurtNoises[randomIndex]; hurtSource.PlayOneShot(hurtNoises[randomIndex]);
                        }
                    }
                }
                if (isAI)
                {
                    if (isAI && GetComponent<AIController>())
                    {
                        gameObject.GetComponent<AIController>().overrideAttack = true;
                    }
                }

                health -= damage;

                //Death
                if (health <= 0)
                {
                    OnDeath.Invoke();

                    if (!tempdoll)
                    {
                        tempdoll = Instantiate(ragdoll, this.transform.position, this.transform.rotation) as GameObject;
                    }

                    if (DestroyOnDead) Destroy(gameObject);

                    foreach (Rigidbody rb in tempdoll.GetComponentsInChildren<Rigidbody>())
                    {
                        rb.AddForce(pos * 5f);
                    }
                }

                //Damage
                if (health > 0)
                {
                    //if (CharacterAnimator && !IgnoreReaction) CharacterAnimator.Play("Hit");

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
            }          
        }
        public void DamageBig(Vector3 pos, float Force, float damage)
        {
            if (!IsBlocking)
            {
                OnReceiveDamage.Invoke();

                if (playNoiseOnHurt)
                {
                    if (Random.value < percentageToPlay)
                    {
                        if (hurtNoises.Length > 0)
                        {
                            float random = Random.Range(0f, 1f);

                            if (random <= 0.3f)
                            {
                                int randomIndex = Random.Range(0, hurtNoises.Length);
                                if (hurtSource) hurtSource.PlayOneShot(hurtNoises[randomIndex]);
                            }
                        }
                    }
                }
                if (isAI)
                {
                    if (isAI && GetComponent<AIController>())
                    {
                        gameObject.GetComponent<AIController>().overrideAttack = true;
                    }
                }

                health -= damage;

                //Death
                if (health <= 0)
                {
                    OnDeath.Invoke();

                    if (!tempdoll)
                    {
                        tempdoll = Instantiate(ragdoll, this.transform.position, this.transform.rotation) as GameObject;
                    }

                    if (DestroyOnDead) Destroy(gameObject);

                    foreach (Rigidbody rb in tempdoll.GetComponentsInChildren<Rigidbody>())
                    {
                        rb.AddForce(pos * 5f);
                    }
                }

                //Damage
                if (health > 0)
                {
                    //if (CharacterAnimator && !IgnoreReaction) CharacterAnimator.Play("HitBig");

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
            }           
        }


        public void Die()
        {
            isDead = true;

            OnDeath.Invoke();

            if (RespawnOnDead) StartCoroutine(RespawnDelay());

            //Only spawn ragdoll if option is selected
            if (!dontSpawnRagdoll)
            {
                if (!tempdoll)
                {
                    //Spawn ragdoll and destroy us
                    tempdoll = Instantiate(ragdoll, this.transform.position, this.transform.rotation) as GameObject;
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

            transform.position = startingPos;

            health = maxHealth;
            OnRespawn.Invoke();

            if (tempdoll) 
            {
                Destroy(tempdoll);
                tempdoll = null;
            }

            isDead = false;
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