using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using PlanetMaenad.FPS;

public class DemoGPU_AIEnemy : MonoBehaviour
{
    public NavMeshAgent agent;
    public DemoGPU_Animator GPUMeshAnimator;
    public string[] IdleNames;
    public string[] WalkNames;
    public float CurrentVelocity;
    [Space(10)]




    public bool UseWander;
    public float wanderSpeed = 1.5f;
    public float wanderRadius = 15;
    public float wanderTimer = 10;
    [Space(20)]



    public Transform DetectEyes;
    public LayerMask DetectionLayers;
    public string[] DetectionTags;
    public Transform CurrentTarget;
    [Space(5)]
    public float DetectionFrequencyTimer = 1.5f;
    public float DetectionRadius = 15f;
    [Space(10)]



    public bool UseAttacks;
    public float AttackFrequencyTimer = 2f;
    public float AttackDistance = 2f;
    public RandomAttack[] _randomAttacks;
    [Space(5)]
    public bool CanAttack = true;
    public bool IsAttacking;
    [Space(10)]



    public UnityEvent OnIdle;
    public UnityEvent OnWalk;
    public UnityEvent OnAttack;



    [System.Serializable]
    public class RandomAttack
    {
        public string AttackName;
        public float AttackDamage;
        public Transform AttackRayOrigin;
        public float AttackRayDelay = .25f;
        public float RaycastDistance = 0.5f;
    }

    internal Transform target;
    internal float timer;
    internal bool IsWalking;



    internal float AnimationOffset;
    internal string selectedIdle;
    internal string selectedWalk;
    internal int selectedAttack;

    internal WaitForEndOfFrame EndOfFrameDelay = new WaitForEndOfFrame();
    internal WaitForSeconds TinyDelay = new WaitForSeconds(0.1f);
    internal WaitForSeconds ShortDelay = new WaitForSeconds(0.2f);

    internal WaitForSeconds DetectionFrequency;
    internal WaitForSeconds AttackFrequency;

    internal Collider[] PotentialTargets;

    void OnEnable()
    {
        agent.speed = wanderSpeed;

        selectedIdle = IdleNames[Random.Range(0, IdleNames.Length)];
        selectedWalk = WalkNames[Random.Range(0, WalkNames.Length)];

        wanderTimer = Random.Range(wanderTimer - 3, wanderTimer + 4);
        timer = wanderTimer;

        AnimationOffset = GPUMeshAnimator.CurrentPlayingOffset;


        DetectionFrequencyTimer = UnityEngine.Random.Range(DetectionFrequencyTimer - 0.25f, _randomAttacks.Length + 0.25f);
        DetectionFrequency = new WaitForSeconds(DetectionFrequencyTimer);
        AttackFrequency = new WaitForSeconds(AttackFrequencyTimer);

        StartCoroutine(CheckForTargets());
    }


    void Update()
    {
        if (agent)
        {
            if (agent.enabled)
            {
                if (agent.isOnNavMesh)
                {
                    CurrentVelocity = agent.velocity.magnitude;
                    if (CurrentTarget) agent.SetDestination(CurrentTarget.position);

                    //Check Speed
                    if (CurrentVelocity <= .1f && IsWalking == true)
                    {
                        OnIdle.Invoke();
                        IsWalking = false;
                    }
                    if (CurrentVelocity > .1f && IsWalking == false)
                    {
                        OnWalk.Invoke();
                        IsWalking = true;
                    }

                    //Play Animation
                    if (GPUMeshAnimator && !IsAttacking)
                    {
                        //Idle
                        if (CurrentVelocity <= .1f)
                        {
                            GPUMeshAnimator.Play(selectedIdle, AnimationOffset); 
                        }
                        //Walking
                        if (CurrentVelocity > .1f)
                        {
                            GPUMeshAnimator.Play(selectedWalk, AnimationOffset); 
                        }
                    }

                    timer += Time.deltaTime;

                    if (timer >= wanderTimer)
                    {
                        if(!IsAttacking && !CurrentTarget)
                        {
                            selectedWalk = WalkNames[Random.Range(0, WalkNames.Length)];

                            //Find New Destination
                            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);

                            if (UseWander) agent.SetDestination(newPos);
                        }                 

                        timer = 0;
                    }
                }

            }
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }


    IEnumerator CheckForTargets()
    {
        while (enabled)
        {
            yield return DetectionFrequency;

            PotentialTargets = Physics.OverlapSphere(transform.position, DetectionRadius, DetectionLayers);

            //Found Colliders
            if (PotentialTargets.Length > 0)
            {
                if (CurrentTarget == null)
                {
                    //Check Each Target
                    for (int t = 0; t < PotentialTargets.Length; t++)
                    {
                        //Check Each Tags
                        for (int i = 0; i < DetectionTags.Length; i++)
                        {
                            //Tag Match
                            if (PotentialTargets[t].transform.CompareTag(DetectionTags[i]))
                            {
                                CurrentTarget = PotentialTargets[t].transform;

                                agent.SetDestination(CurrentTarget.position);

                                if (UseAttacks)
                                {
                                    StartCoroutine(CheckAttackDistance());
                                }
                            }
                        }
                    }
                }
            }

            //No Target Found
            if (PotentialTargets.Length == 0)
            {
                CurrentTarget = null;

                selectedIdle = IdleNames[Random.Range(0, IdleNames.Length)];
                selectedWalk = WalkNames[Random.Range(0, WalkNames.Length)];

                if (UseAttacks)
                {
                    StopCoroutine(CheckAttackDistance());

                    Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);

                    if (UseWander) agent.SetDestination(newPos);
                }
            }

            if (CurrentTarget != null)
            {
                agent.SetDestination(CurrentTarget.position);

                if (UseAttacks && !IsAttacking)
                {
                    StartCoroutine(CheckAttackDistance());
                }
            }
        }
    }
    IEnumerator CheckAttackDistance()
    {
        while (enabled)
        {
            yield return DetectionFrequency;

            //Has Target
            if (CurrentTarget)
            {
                float dist = Vector3.Distance(CurrentTarget.position, transform.position);

                if (dist < AttackDistance)
                {
                    if (!IsAttacking && CanAttack)
                    {
                        DoRandomAttack();
                    }
                }
                if (dist > AttackDistance)
                {
                    CanAttack = true;
                    IsAttacking = false;

                  
                }
            }
        }
    }

    public void DoRandomAttack()
    {
        if (_randomAttacks.Length > 0)
        {
            StartCoroutine(AttackResetDelay());
        }
    }


    IEnumerator AttackResetDelay()
    {
        while (enabled)
        {
            OnAttack.Invoke();

            transform.LookAt(CurrentTarget, Vector3.up);

            //Select Attack
            var selectedAttack = Random.Range(0, _randomAttacks.Length);
            GPUMeshAnimator.Play(_randomAttacks[selectedAttack].AttackName, 0);

            StartCoroutine(AttackRaycastDelay(_randomAttacks[selectedAttack].AttackRayDelay));

            //Disable Attack
            CanAttack = false;
            IsAttacking = true;

            yield return AttackFrequency;

            selectedAttack = Random.Range(0, _randomAttacks.Length);
            CanAttack = true;

            if (CurrentTarget)
            {
                float dist = Vector3.Distance(CurrentTarget.position, transform.position);

                if (dist > AttackDistance)
                {
                    IsAttacking = false;
                    yield break;
                }
            }
            else
            {
                IsAttacking = false;
                yield break;
            }
        }
    }

    IEnumerator AttackRaycastDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        Vector3 rayOrigin = _randomAttacks[selectedAttack].AttackRayOrigin.position;
        Vector3 rayForward = _randomAttacks[selectedAttack].AttackRayOrigin.forward;

        RaycastHit hit;

        //Hits an Object
        if (Physics.Raycast(rayOrigin, rayForward, out hit, _randomAttacks[selectedAttack].RaycastDistance, DetectionLayers))
        {
            var hitTransform = hit.collider.transform;

            if (hitTransform.GetComponent<HealthController>())
            {
                var rootHealth = hitTransform.GetComponent<HealthController>();

                if (rootHealth) rootHealth.Damage(rayForward * 360, 0, _randomAttacks[selectedAttack].AttackDamage);
                
            }
        }

        //
    }



}
