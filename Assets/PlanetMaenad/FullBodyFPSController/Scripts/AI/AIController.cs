using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace PlanetMaenad.FPS
{
    public class AIController : MonoBehaviour
    {
        public Rigidbody _rigidbody;
        public NavMeshAgent agent;
        public Animator m_Animator;
        public HealthController HealthControl;
        [Space(5)]
        public float CurrentVelocity;
        public float health = 100;
        [Space(10)]


        public LayerMask groundLayers = -1;
        [Space(10)]



        public bool UseWander = true;
        public Vector3 wanderPoint;
        public float wanderRadius = 5;
        public bool moving;
        public bool isAttacking;
        [Space(10)]


        public float m_ForwardAmount;
        public float m_TurnAmount;
        public float moveDamping = 0.4f;
        [Space(10)]



        
        public bool targetInSightRange;
        public bool targetInAttackRange;
        public bool overrideAttack;
        [Space(10)]


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
        public bool IsAttacking;
        public bool CanAttack = true;
        public bool MoveWhileAttacking = true;
        [Space(5)]
        public UnityEvent OnAttack;
        [Space(10)]



        internal WaitForSeconds DetectionFrequency;
        internal WaitForSeconds AttackFrequency;

        internal Collider[] PotentialTargets;

        internal bool wanderPointSet;
        internal Vector3 lastPos;



        [System.Serializable]
        public class RandomAttack
        {
            public string AttackName;
            public float AttackDamage;
            public GameObject AttackHitBox;
            public float HitboxResetDelay = .5f;
        }


        void OnEnable()
        {
            if (!UseWander && m_Animator) m_Animator.applyRootMotion = false;

            if (HealthControl) HealthControl.OnReceiveDamage.AddListener(AttackCancel);

            DetectionFrequencyTimer = Random.Range(DetectionFrequencyTimer - 0.25f, _randomAttacks.Length + 0.25f);
            DetectionFrequency = new WaitForSeconds(DetectionFrequencyTimer);
            AttackFrequency = new WaitForSeconds(AttackFrequencyTimer);

            StartCoroutine(CheckForTargets());
        }

        void Update()
        {
            //targetInSightRange = Physics.CheckSphere(transform.position, DetectionRadius, enemyLayers);
            //targetInAttackRange = Physics.CheckSphere(transform.position, AttackDistance, enemyLayers);

            if (CurrentTarget)
            {
                if (!targetInSightRange && !targetInAttackRange) Patroling();
                if (!targetInSightRange && !targetInAttackRange && isAttacking) Patroling();

                if (targetInSightRange && !targetInAttackRange) ChaseTarget(); 
                if (targetInSightRange && !targetInAttackRange && isAttacking) ChaseTarget(); 

                //if (targetInAttackRange && targetInSightRange) AttackTargetFixed();
            }
            //else
            //{
            //    //Immeditaley attack target if attacked
            //    if (!targetInAttackRange && target != null) ChaseTarget();
            //    if (targetInAttackRange) overrideAttack = false;
            //


            if (UseWander || MoveWhileAttacking)
            {
                if (agent && agent.enabled && agent.isOnNavMesh) CurrentVelocity = agent.velocity.magnitude;

                //Update the turn animation to our y axis rotation
                m_TurnAmount = transform.rotation.y;

                //Check for movement and play the walking animation
                if (transform.position != lastPos)
                {
                    m_ForwardAmount = 1 * moveDamping;
                    moving = true;
                }
                else
                {
                    m_ForwardAmount = 0;
                    moving = false;
                }
            }

            lastPos = transform.position;

            //Update our animator constantly
            UpdateAnimator();
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
                                        if (MoveWhileAttacking && CurrentTarget) agent.SetDestination(CurrentTarget.position);

                                        targetInSightRange = true;

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

                                    targetInSightRange = false;
                                    targetInAttackRange = false;

                                    if (UseAttacks)
                                    {
                                        StopCoroutine(CheckAttackDistance());
                                        AttackCancel();
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

                    targetInSightRange = false;
                    targetInAttackRange = false;

                    if (UseAttacks)
                    {
                        StopCoroutine(CheckAttackDistance());
                        AttackCancel();
                    }
                }

                if (CurrentTarget != null)
                {
                    if (MoveWhileAttacking && CurrentTarget) agent.SetDestination(CurrentTarget.position);

                    targetInSightRange = true;

                    if (UseAttacks)
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
                        transform.LookAt(CurrentTarget, Vector3.up);

                        AttackTargetFixed();

                        targetInAttackRange = true;
                    }
                    if (dist > AttackDistance)
                    {
                        ChaseTarget();

                        targetInAttackRange = false;
                    }
                }
            }
        }


        //public void DoRandomAttack()
        //{
        //    if (_randomAttacks.Length > 0)
        //    {
        //        //StartCoroutine(AttackResetDelay());
        //    }
        //}

        //IEnumerator AttackResetDelay()
        //{
        //    while (enabled)
        //    {
        //        OnAttack.Invoke();

        //        transform.LookAt(CurrentTarget, Vector3.up);

        //        //Select Attack
        //        var selectedAttack = Random.Range(0, _randomAttacks.Length);

         
        //        //Disable Attack
        //        CanAttack = false;
        //        IsAttacking = true;

        //        yield return AttackFrequency;


        //        selectedAttack = Random.Range(0, _randomAttacks.Length);
        //        CanAttack = true;

        //        float dist = Vector3.Distance(CurrentTarget.position, transform.position);

        //        if (dist > AttackDistance)
        //        {
        //            IsAttacking = false;
        //            yield break;
        //        }

        //    }
        //}

        public void AttackTargetFixed()
        {
            if (!MoveWhileAttacking) agent.SetDestination(transform.position);
            if (MoveWhileAttacking && m_Animator) m_Animator.applyRootMotion = true;

            if (CurrentTarget)
            {
                Vector3 targetPostitionXZ = new Vector3(CurrentTarget.position.x, transform.position.y, CurrentTarget.position.z);
                transform.LookAt(targetPostitionXZ);

                isAttacking = true;

                //shootPoint.LookAt(target);
                //AiGun.Fire();
            }
        }
        public void AttackTarget()
        {
            if (MoveWhileAttacking && m_Animator) m_Animator.applyRootMotion = true;

            //Move and shoot
            if (CurrentTarget)
            {
                Vector3 targetPostitionXZ = new Vector3(CurrentTarget.position.x, transform.position.y, CurrentTarget.position.z);
                transform.LookAt(targetPostitionXZ);

                isAttacking = true;

                //shootPoint.LookAt(target);
                //AiGun.Fire();
            }
        }
        public void AttackCancel()
        {
            isAttacking = false;
            overrideAttack = false;

            if (MoveWhileAttacking && !UseWander && m_Animator) m_Animator.applyRootMotion = false;
        }



        public void Patroling()
        {

            if (!wanderPointSet) SearchwanderPoint();

            if (wanderPointSet && UseWander)
                agent.SetDestination(wanderPoint);

            Vector3 distanceTowanderPoint = transform.position - wanderPoint;

            //wanderPoint reached
            if (distanceTowanderPoint.magnitude < 1f)
                wanderPointSet = false;
        }
        public void SearchwanderPoint()
        {
            //Calculate random point in range
            float randomZ = Random.Range(-wanderRadius, wanderRadius);
            float randomX = Random.Range(-wanderRadius, wanderRadius);

            wanderPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            if (Physics.Raycast(wanderPoint, -transform.up, 2f, groundLayers))
                wanderPointSet = true;
        }
        public void ChaseTarget()
        {
            if (MoveWhileAttacking) agent.SetDestination(CurrentTarget.position);
        }


        public void UpdateAnimator()
        {
            //Update the blend trees
            if (m_Animator) m_Animator.SetBool("Attack", targetInAttackRange);
            
            if (m_Animator && (UseWander || MoveWhileAttacking)) m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
            if (m_Animator && (UseWander || MoveWhileAttacking)) m_Animator.SetFloat("Turn", m_TurnAmount * 0.3f, 0.1f, Time.deltaTime);
        }

    }
}
