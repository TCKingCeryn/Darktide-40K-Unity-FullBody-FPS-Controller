using UnityEngine;
using UnityEngine.AI;

namespace scgFullBodyController
{
    public class AIShooterController : AIController
    {

        public AIShooterWeapon AiGun;
        public Transform shootPoint;



        void Awake()
        {
            if(HealthControl) HealthControl.OnReceiveDamage.AddListener(AiGun.FireCancel);

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
                    ShootTargetFixed();
                else if (AiGun.firing)
                {                  
                    AiGun.FireCancel();
                }
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

       

        public void ShootTargetFixed()
        {
            AttacktargetFixed();

            if (target)
            {
                Vector3 targetPostitionXZ = new Vector3(target.position.x, transform.position.y, target.position.z);
                transform.LookAt(targetPostitionXZ);
                shootPoint.LookAt(target);
            }

            //Fire the gun located in our hand bone
            AiGun.Fire();
        }

        public void Shoottarget()
        {
            Attacktarget();

            shootPoint.LookAt(target);

            //Fire the gun located in our hand bone
            AiGun.Fire();
        }

       
    }
}
