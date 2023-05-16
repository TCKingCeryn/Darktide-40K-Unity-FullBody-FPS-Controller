using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace PlanetMaenad.FPS
{
    public class AIShooterController : AIController
    {

        public float SpineYOffset = 65;
        public OffsetYRotation OffsetRotation;
        [Space(10)]

        public Transform shootPoint;
        public AIShooterWeapon AiGun;



        void OnEnable()
        {
            if (!MoveWhileAttacking || !UseWander) m_Animator.applyRootMotion = false;

            if (HealthControl) HealthControl.OnReceiveDamage.AddListener(ShootCancel); 

            if (agent) agent.speed = MoveSpeed;

            DetectionFrequencyTimer = Random.Range(DetectionFrequencyTimer - 0.25f, _randomAttacks.Length + 0.25f);
            DetectionFrequency = new WaitForSeconds(DetectionFrequencyTimer);
            AttackFrequency = new WaitForSeconds(AttackFrequencyTimer);

            StartCoroutine(CheckForTargets());
        }
        void Update()
        {

            if (CurrentTarget)
            {
                if (!targetInSightRange && !targetInAttackRange && UseWander) Patroling();
                if (targetInSightRange && !targetInAttackRange && MoveWhileAttacking) ChaseTarget(); 
            }

            if (agent && agent.enabled && agent.isOnNavMesh) CurrentVelocity = agent.velocity.magnitude;

            m_TurnAmount = transform.rotation.y;

            if (transform.position != lastPos)
            {
                m_ForwardAmount = 1 * CurrentVelocity;
                moving = true;
            }
            else
            {
                m_ForwardAmount = 0;
                moving = false;
            }

            lastPos = transform.position;

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
                    //Check Each Tags
                    for (int i = 0; i < DetectionTags.Length; i++)
                    {
                        //Tag Match
                        if (PotentialTargets[0].transform.CompareTag(DetectionTags[i]))
                        {
                            CurrentTarget = PotentialTargets[0].transform;

                            Vector3 rayOrigin = DetectEyes.position;
                            RaycastHit hit;
                            var rayDirection = (CurrentTarget.position + AimOffset) - DetectEyes.position;

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
                                        StartCoroutine(CheckShootDistance());
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
                                    StopCoroutine(CheckShootDistance());
                                    ShootCancel();
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
                        StopCoroutine(CheckShootDistance());
                        ShootCancel();
                    }
                }

                if (CurrentTarget != null)
                {
                    if (MoveWhileAttacking && CurrentTarget) agent.SetDestination(CurrentTarget.position);

                    targetInSightRange = true;

                    if (UseAttacks)
                    {
                        StartCoroutine(CheckShootDistance());
                    }
                }
            }
        }
        IEnumerator CheckShootDistance()
        {
            while (enabled)
            {
                yield return AttackFrequency;

                //Has Target
                if (CurrentTarget)
                {
                    float dist = Vector3.Distance(CurrentTarget.position, transform.position);

                    if (dist < AttackDistance)
                    {
                        ShootTarget();

                        targetInAttackRange = true;
                    }
                    if (dist > AttackDistance)
                    {                    
                        ChaseTarget();
                        ShootCancel();

                        targetInAttackRange = false;
                    }
                }
            }
        }
   
        void ShootTarget()
        {
            if (!MoveWhileAttacking) agent.SetDestination(transform.position);

            OnAttack.Invoke();

            if (CurrentTarget)
            {
                Vector3 targetPostitionXZ = new Vector3(CurrentTarget.position.x, transform.position.y, CurrentTarget.position.z);
                transform.LookAt(targetPostitionXZ, Vector3.up);
                shootPoint.LookAt(CurrentTarget.position + AimOffset);

                if (m_Animator)
                {
                    if (!m_Animator.GetCurrentAnimatorStateInfo(1).IsName("Hit") && !m_Animator.GetCurrentAnimatorStateInfo(1).IsName("HitBig"))
                    {
                        //Shoot Gun
                        AiGun.currentShootPoint = CurrentTarget.position + AimOffset;
                        AiGun.Fire();

                        m_Animator.SetBool("Fire", true);
                    }
                }
            }

            if (OffsetRotation)
            {
                OffsetRotation.offsetRotation = SpineYOffset;
                OffsetRotation.RotateTarget = CurrentTarget;
                OffsetRotation.RotateOffset = AimOffset;
            }
        }
        void ShootCancel()
        {

            if (m_Animator && !m_Animator.GetCurrentAnimatorStateInfo(1).IsName("Hit") && !m_Animator.GetCurrentAnimatorStateInfo(1).IsName("HitBig"))
            {
                m_Animator.SetBool("Fire", false);

                overrideAttack = false;

                if (OffsetRotation)
                {
                    OffsetRotation.offsetRotation = 0;
                    OffsetRotation.RotateTarget = null;
                    OffsetRotation.RotateOffset = Vector3.zero;
                }
            }          
        }
       
    }
}
