using UnityEngine;
using UnityEngine.AI;

namespace scgFullBodyController
{
    public class AIController : MonoBehaviour
    {
        public NavMeshAgent agent;
        public Animator m_Animator;
        public HealthController HealthControl;
        [Space(5)]
        public float health;
        [Space(10)]


        public Transform target;
        [Space(10)]


        public LayerMask groundLayers;
        public LayerMask enemyLayers;
        [Space(10)]


        public Vector3 walkPoint;
        public float walkPointRange;
        public bool moving;
        [Space(10)]


        public float m_ForwardAmount;
        public float m_TurnAmount;
        public float moveDamping;
        [Space(10)]


        public float sightRange = 8;
        public float attackRange = 6;
        public bool targetInSightRange, targetInAttackRange;
        public bool overrideAttack;

        //Weapon orienting



        //Increase this in correspondence with the navmesh agent speed and acceleration to match the animation with it

        internal bool walkPointSet;
        internal Vector3 lastPos;

        void Awake()
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }

        void Update()
        {
            //Check for sight and attack range
            targetInSightRange = Physics.CheckSphere(transform.position, sightRange, enemyLayers);
            targetInAttackRange = Physics.CheckSphere(transform.position, attackRange, enemyLayers);

            //If we have not been attacked
            if (!overrideAttack)
            {
                if (!targetInSightRange && !targetInAttackRange) Patroling();
                if (targetInSightRange && !targetInAttackRange) Chasetarget();
                if (targetInAttackRange && targetInSightRange)
                    AttacktargetFixed();
                //else if (AiGun.firing)
                //{                  
                //    AiGun.FireCancel();
                //}
            }
            else
            {
                //Immeditaley attack target if attacked
                if (!targetInAttackRange && target != null)
                    ChasetargetAttack();

                if (targetInAttackRange)
                    overrideAttack = false;
            }

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

            lastPos = transform.position;

            //Update our animator constantly
            UpdateAnimator();
        }

        public void Patroling()
        {
            if (!walkPointSet) SearchWalkPoint();

            if (walkPointSet)
                agent.SetDestination(walkPoint);

            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            //Walkpoint reached
            if (distanceToWalkPoint.magnitude < 1f)
                walkPointSet = false;
        }

        public void SearchWalkPoint()
        {
            //Calculate random point in range
            float randomZ = Random.Range(-walkPointRange, walkPointRange);
            float randomX = Random.Range(-walkPointRange, walkPointRange);

            walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            if (Physics.Raycast(walkPoint, -transform.up, 2f, groundLayers))
                walkPointSet = true;
        }

        public void Chasetarget()
        {
            agent.SetDestination(target.position);
        }

        public void ChasetargetAttack()
        {
            agent.SetDestination(target.position);
            Attacktarget();
        }

        public void AttacktargetFixed()
        {
            //Make sure we don't move while shooting
            agent.SetDestination(transform.position);

            if(target)
            {
                Vector3 targetPostitionXZ = new Vector3(target.position.x, transform.position.y, target.position.z);
                transform.LookAt(targetPostitionXZ);

                //shootPoint.LookAt(target);
            }

            //Fire the gun located in our hand bone
            //AiGun.Fire();
        }

        public void Attacktarget()
        {
            //Move and shoot

            Vector3 targetPostitionXZ = new Vector3(target.position.x, transform.position.y, target.position.z);
            transform.LookAt(targetPostitionXZ);

            //shootPoint.LookAt(target);

            //Fire the gun located in our hand bone
            //AiGun.Fire();
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRange);
        }

        public void UpdateAnimator()
        {
            //Update the blend trees
            if (m_Animator) m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
            if (m_Animator) m_Animator.SetFloat("Turn", m_TurnAmount * 0.3f, 0.1f, Time.deltaTime);
        }
    }
}
