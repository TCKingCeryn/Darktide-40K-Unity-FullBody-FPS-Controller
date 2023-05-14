using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace PlanetMaenad.FPS
{
    public class AIShooterController : AIController
    {
        public Transform shootPoint;
        public AIShooterWeapon AiGun;


        public float SpineYOffset = 65;
        public OffsetYRotation OffsetRotation;



        void OnEnable()
        {
            if (!MoveWhileAttacking || !UseWander) m_Animator.applyRootMotion = false;

            if (HealthControl) HealthControl.OnReceiveDamage.AddListener(AiGun.FireCancel);

            DetectionFrequencyTimer = Random.Range(DetectionFrequencyTimer - 0.25f, _randomAttacks.Length + 0.25f);
            DetectionFrequency = new WaitForSeconds(DetectionFrequencyTimer);
            AttackFrequency = new WaitForSeconds(AttackFrequencyTimer);

            StartCoroutine(CheckForTargets());
        }

        void Update()
        {
            //Check for sight and attack range
            //targetInSightRange = Physics.CheckSphere(transform.position, DetectionRadius, enemyLayers);
            //targetInAttackRange = Physics.CheckSphere(transform.position, AttackDistance, enemyLayers);

            //If we have not been attacked
            if (CurrentTarget)
            {
                if (!targetInSightRange && !targetInAttackRange) Patroling();
                if (targetInSightRange && !targetInAttackRange) ChaseTarget(); 

                //if (targetInAttackRange && targetInSightRange) ShootTarget();
            }
            //else
            //{
            //    //Immeditaley attack target if attacked
            //    if (!targetInAttackRange && target != null) ChaseTarget(); 
            //    if (targetInAttackRange) overrideAttack = false;
            //}

            if(UseWander || MoveWhileAttacking && CurrentTarget)
            {
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
                yield return DetectionFrequency;

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

                        targetInAttackRange = false;
                    }
                }
            }
        }

    


        public void ShootTarget()
        {
            if (!MoveWhileAttacking) agent.SetDestination(transform.position);
            if (MoveWhileAttacking) m_Animator.applyRootMotion = true;

            if (CurrentTarget)
            {
                Vector3 targetPostitionXZ = new Vector3(CurrentTarget.position.x, transform.position.y, CurrentTarget.position.z);
                transform.LookAt(targetPostitionXZ, Vector3.up);

                shootPoint.LookAt(CurrentTarget);

                if (!m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Hit") && !m_Animator.GetCurrentAnimatorStateInfo(0).IsName("KickHit")) AiGun.Fire();
                if (m_Animator) m_Animator.SetBool("Fire", true);
            }

            if (OffsetRotation) OffsetRotation.offsetRotation = SpineYOffset;
        }

        public void ShootCancel()
        {
            if (m_Animator) m_Animator.SetBool("Fire", false);
            overrideAttack = false;

            if (OffsetRotation) OffsetRotation.offsetRotation = 0;

            if (MoveWhileAttacking && !UseWander) m_Animator.applyRootMotion = false;
        }
       
    }
}
