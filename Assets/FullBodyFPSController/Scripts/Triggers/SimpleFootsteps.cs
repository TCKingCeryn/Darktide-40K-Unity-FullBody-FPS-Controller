using UnityEngine;
using System.Collections;

namespace scgFullBodyController
{
    [Tooltip("IMPORTANT, this script needs to be on the root transform")]
    public class SimpleFootsteps : MonoBehaviour
    {
        public bool toggle;
        [Space(5)]
        public bool isAi;
        public bool moving;
        [Space(10)]


        public float footstepSensitivity;
        public float playbackSpeedDamping;
        public float speed = 0.0f;
        [Space(10)]


        public AudioSource audioSource;
        [Space(5)]
        public string grassTag = "Grass";
        public AudioClip[] soundGrass;
        public string waterTag = "Water";
        public AudioClip[] soundWater;
        public string metalTag = "Metal";
        public AudioClip[] soundMetal;
        public string concreteTag = "Concrete";
        public AudioClip[] soundConcrete;
        public string gravelTag = "Gravel";
        public AudioClip[] soundGravel;
        [Space(10)]


        internal string floortag;





        void Start()
        {
            StartCoroutine("senseSteps");
        }

        void OnCollisionEnter(Collision col)
        {
            if (col.transform.tag == "grass")
            {
                floortag = "grass";
            }
            else if (col.transform.tag == "metal")
            {
                floortag = "metal";
            }
            else if (col.transform.tag == "gravel")
            {
                floortag = "gravel";
            }
            else if (col.transform.tag == "water")
            {
                floortag = "water";
            }
            else if (col.transform.tag == "concrete")
            {
                floortag = "concrete";
            }
        }

        void OnTriggerEnter(Collider col)
        {
            if (col.transform.tag == "grass")
            {
                floortag = "grass";
            }
            else if (col.transform.tag == "metal")
            {
                floortag = "metal";
            }
            else if (col.transform.tag == "gravel")
            {
                floortag = "gravel";
            }
            else if (col.transform.tag == "water")
            {
                floortag = "water";
            }
            else if (col.transform.tag == "concrete")
            {
                floortag = "concrete";
            }
        }

        void Update()
        {
            if (!isAi)
            {
                //Sensing movement for players
                var velocity = gameObject.GetComponent<Rigidbody>().velocity;
                var localVel = transform.InverseTransformDirection(velocity);

                if (localVel.z > footstepSensitivity)
                {
                    moving = true;
                }
                else if (localVel.z < (footstepSensitivity * -1))
                {
                    moving = true;
                }
                else if (localVel.x > footstepSensitivity)
                {
                    moving = true;
                }
                else if (localVel.x < (footstepSensitivity * -1))
                {
                    moving = true;
                }
                else
                {
                    moving = false;
                }

                //Different playback speed calculations (for crouching, sprinting etc.)
                speed = 1 - (gameObject.GetComponent<Rigidbody>().velocity.magnitude * playbackSpeedDamping);
            }
            else
            {
                //Sensing movement for AI
                if (GetComponent<AIController>().moving)
                {
                    moving = true;
                }
                else
                {
                    moving = false;
                }
            }


        }

        IEnumerator senseSteps()
        {
            while (true)
            {
                if (!isAi)
                {
                    if (gameObject.GetComponent<FPSPlayerMover>().m_Grounded && moving && !gameObject.GetComponent<FPSPlayerMover>().m_Sliding)
                    {
                        if (floortag == "grass")
                        {
                            audioSource.clip = soundGrass[Random.Range(0, soundGrass.Length)];
                        }
                        else if (floortag == "gravel")
                        {
                            audioSource.clip = soundGravel[Random.Range(0, soundGravel.Length)];
                        }
                        else if (floortag == "water")
                        {
                            audioSource.clip = soundWater[Random.Range(0, soundWater.Length)];
                        }
                        else if (floortag == "metal")
                        {
                            audioSource.clip = soundMetal[Random.Range(0, soundMetal.Length)];
                        }
                        else if (floortag == "concrete")
                        {
                            audioSource.clip = soundConcrete[Random.Range(0, soundConcrete.Length)];
                        }
                        else
                        {
                            yield return 0;
                        }
                        if (audioSource.clip != null)
                            audioSource.PlayOneShot(audioSource.clip);
                        yield return new WaitForSeconds(speed);
                    }
                    else
                    {
                        yield return 0;
                    }
                }
                else
                {
                    if (moving)
                    {
                        if (floortag == "grass")
                        {
                            audioSource.clip = soundGrass[Random.Range(0, soundGrass.Length)];
                        }
                        else if (floortag == "gravel")
                        {
                            audioSource.clip = soundGravel[Random.Range(0, soundGravel.Length)];
                        }
                        else if (floortag == "water")
                        {
                            audioSource.clip = soundWater[Random.Range(0, soundWater.Length)];
                        }
                        else if (floortag == "metal")
                        {
                            audioSource.clip = soundMetal[Random.Range(0, soundMetal.Length)];
                        }
                        else if (floortag == "concrete")
                        {
                            audioSource.clip = soundConcrete[Random.Range(0, soundConcrete.Length)];
                        }
                        else
                        {
                            yield return 0;
                        }
                        if (audioSource.clip != null)
                            audioSource.PlayOneShot(audioSource.clip);
                        yield return new WaitForSeconds(speed);
                    }
                    else
                    {
                        yield return 0;
                    }
                }

            }
        }
    }
}