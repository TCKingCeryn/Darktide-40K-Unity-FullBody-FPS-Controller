using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using PlanetMaenad.FPS;

public class DEMOGPU_AIEnemy : MonoBehaviour
{
    public Rigidbody _rigidbody;
    public NavMeshAgent agent;
    public DemoGPU_Animator GPUMeshAnimator;
    public HealthController HealthControl;
    [Space(5)]
    public float CurrentVelocity;
    [Space(10)]


    public bool UseWander;
    [Space(5)]
    public float wanderSpeed = 1.5f;
    public float wanderRadius = 15;
    public float wanderTimer = 10;
    [Space(5)]
    public string[] IdleNames;
    public string[] WalkNames;
    [Space(10)]



    public Transform CurrentTarget;
    [Space(5)]
    public Transform DetectEyes;
    public LayerMask DetectionLayers;
    public string[] DetectionTags;
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
    internal GameObject HitObject;

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
                //Check Each Tags
                for (int i = 0; i < DetectionTags.Length; i++)
                {
                    //Tag Match
                    if (PotentialTargets[0].transform.CompareTag(DetectionTags[i]))
                    {
                        CurrentTarget = PotentialTargets[0].transform;

                        Vector3 rayOrigin = DetectEyes.position;
                        RaycastHit hit;
                        var rayDirection = CurrentTarget.position - DetectEyes.position;

                        //Hits an Object
                        if (Physics.Raycast(rayOrigin, rayDirection, out hit, DetectionRadius + 1, DetectionLayers))
                        {
                            var hitTransform = hit.collider.transform;

                            //Can See Target
                            if (hitTransform == CurrentTarget)
                            {
                                agent.SetDestination(CurrentTarget.position);

                                if (UseAttacks)
                                {
                                    StartCoroutine(CheckAttackDistance());
                                }
                            }
                        }
                        else
                        {
                            //No Target Found
                            CurrentTarget = null;

                            selectedIdle = IdleNames[Random.Range(0, IdleNames.Length)];
                            selectedWalk = WalkNames[Random.Range(0, WalkNames.Length)];

                            if (UseAttacks)
                            {
                                StopCoroutine(CheckAttackDistance());
                                StopCoroutine(AttackResetDelay());

                                CanAttack = true;
                                IsAttacking = false;

                                Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);

                                if (UseWander) agent.SetDestination(newPos);
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
                    StopCoroutine(AttackResetDelay());

                    CanAttack = true;
                    IsAttacking = false;

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
            yield return AttackFrequencyTimer;

            //Has Target
            if (CurrentTarget)
            {
                float dist = Vector3.Distance(CurrentTarget.position, transform.position);

                if (dist < AttackDistance)
                {
                    if (CanAttack && !IsAttacking)
                    {
                        CanAttack = false;
                        IsAttacking = true;

                        DoRandomAttack();

                        StartCoroutine(AttackResetDelay());
                    }
                }
            }
        }
    }


    public void DoRandomAttack()
    {
        if (_randomAttacks.Length > 0)
        {
            OnAttack.Invoke();

            var selectedAttack = Random.Range(0, _randomAttacks.Length);
            GPUMeshAnimator.Play(_randomAttacks[selectedAttack].AttackName, 0);
            
            if (!HitObject) StartCoroutine(AttackRaycastDelay(_randomAttacks[selectedAttack].AttackRayDelay));
        }
    }
    IEnumerator AttackResetDelay()
    {
        while (enabled)
        {
            yield return AttackFrequency;

            CanAttack = true;
            IsAttacking = false;
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
            HitObject = hitTransform.gameObject;

            for (int i = 0; i < DetectionTags.Length; i++)
            {
                //Can See Target
                if (hitTransform.CompareTag(DetectionTags[i]))
                {
                    PassDamage(hitTransform.gameObject, hit.point);

                    HitObject = null;
                }
            }
        }  
    }

    public void PassDamage(GameObject other, Vector3 damagePoint)
    {
        if (other.GetComponentInParent<HealthController>())
        {
            var parentHealth = other.GetComponentInParent<HealthController>();
            parentHealth.Damage(transform.forward * 360, 0, _randomAttacks[selectedAttack].AttackDamage);          
        }
    }



}
