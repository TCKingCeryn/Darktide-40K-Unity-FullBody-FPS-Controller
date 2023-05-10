using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace scgFullBodyController
{
    [Tooltip("IMPORTANT, this script needs to be on the root transform")]
    public class FPSPlayerMover : MonoBehaviour
    {
        public FPSCameraController camController;
        [Space(5)]
        public bool m_Grounded;
        public bool m_Crouching;
        public bool m_Sliding;
        [Space(10)]


        public float sprintSpeed = 5f;
        public float walkSpeed = .8f;
        public float crouchSpeed = 0;
        public float jumpPower = 8f;
        [Space(20)]



        public float jumpDamping = .8f;
        public float m_GroundCheckDistance = .3f;
        [Range(1f, 10f)] public float m_GravityMultiplier = 4f;
        [Space(10)]


        public float sensitivity = 1f;
        public float slideTime = .8f;
        public float vaultCancelTime = 1.5f;
        [Space(5)]


        [SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
        [SerializeField] float m_MoveSpeedMultiplier = 1f;
        [SerializeField] float m_AnimSpeedMultiplier = 1f;
        [Space(10)]





        internal Rigidbody m_Rigidbody;
        internal Animator m_Animator;
        internal CapsuleCollider m_Capsule;


        internal float m_OrigGroundCheckDistance;
        const float k_Half = 0.5f;
        internal float m_TurnAmount;
        internal float m_ForwardAmount;
        internal Vector3 m_GroundNormal;
        internal float m_CapsuleHeight;
        internal Vector3 m_CapsuleCenter;

        float maxCamOriginal;
        float minCamOriginal;
        bool toggle;



        void Start()
        {
            m_Animator = GetComponent<Animator>();
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();

            m_CapsuleHeight = m_Capsule.height;
            m_CapsuleCenter = m_Capsule.center;

            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

            maxCamOriginal = camController.maxPitch;
            m_OrigGroundCheckDistance = m_GroundCheckDistance;
        }
        void OnAnimatorMove()
        {
            // we implement this function to override the default root motion. This allows us to modify the positional speed before it's applied.
            if (m_Grounded && Time.deltaTime > 0)
            {
                Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

                //We preserve the existing y part of the current velocity.
                v.y = m_Rigidbody.velocity.y;
                m_Rigidbody.velocity = v;
            }

        }





        public void HandleInputUpdate(Vector3 move, bool crouch, bool prone, bool vaulting, bool forwards, bool backwards, bool strafe, float horizontal, float vertical)
        {
            m_TurnAmount = camController.relativeYaw;
            transform.eulerAngles = new Vector3(0, camController.transform.eulerAngles.y, 0);
            UpdateAnimator(move, crouch, prone, vaulting, forwards, backwards, strafe, horizontal, vertical);
        }
        public void HandleMovement(bool crouch, bool jump, bool slide)
        {
            if (m_Grounded)
            {
                HandleGroundedMovement(crouch, jump, slide);
            }
            else
            {
                HandleAirborneMovement();
            }
        }

        void HandleAirborneMovement()
        {
            // apply extra gravity from multiplier:
            Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
            m_Rigidbody.AddForce(extraGravityForce);

            m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
        }
        void HandleGroundedMovement(bool crouch, bool jump, bool slide)
        {
            // check whether conditions are right to allow a jump:
            if (jump && !slide && !crouch)
            {
                if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
                {
                    Jump();
                }
                else if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("StrafeStanding"))
                {
                    Jump();
                }
            }
        }


        public void Move(Vector3 move, bool crouch, bool jump, bool slide, bool vaulting)
        {
            // convert the world relative moveInput vector into a local-relative
            // turn amount and forward amount required to head in the desired
            // direction.
            if (move.magnitude > 1f) move.Normalize();
            move = transform.InverseTransformDirection(move);
            CheckGroundStatus(vaulting);
            move = Vector3.ProjectOnPlane(move, m_GroundNormal);

            if (!vaulting)
                m_ForwardAmount = move.z;
            else
                m_ForwardAmount = 0;


            ScaleCapsuleForCrouching(crouch);
            ScaleCapsuleForSliding(slide);
            PreventStandingInLowHeadroom();
        }
        public void Jump()
        {
            // Jump
            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x * jumpDamping, jumpPower, m_Rigidbody.velocity.z * jumpDamping);
            m_Grounded = false;
            m_Animator.applyRootMotion = false;
            m_GroundCheckDistance = 0.1f;
        }
        public void Kick()
        {
            m_Animator.CrossFade("Kick", 0.1f);
        }
        public void Vault()
        {

        }
        IEnumerator VaultDelay()
        {
            m_Animator.CrossFade("Vault", 0.1f);
            m_Capsule.isTrigger = true;
            m_Rigidbody.isKinematic = true;
            m_Rigidbody.useGravity = false;

            yield return new WaitForSeconds(0.5f);

            m_Capsule.isTrigger = false;
            m_Rigidbody.isKinematic = false;
            m_Rigidbody.useGravity = true;
        }

        public void UpdateAnimator(Vector3 move, bool crouch, bool prone, bool vaulting, bool forwards, bool backwards, bool strafe, float horizontal, float vertical)
        {
            // update the animator parameters

            if (backwards)
            {
                m_Animator.SetFloat("Forward", m_ForwardAmount * -1, 0.1f, Time.deltaTime);
                m_Animator.SetFloat("animSpeed", -1);
            }
            else if (forwards)
            {
                m_Animator.SetFloat("animSpeed", 1);
                m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
            }
            else
            {
                m_Animator.SetFloat("animSpeed", 1);
                m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
            }

            if (strafe)
            {
                m_Animator.SetBool("strafe", true);
            }
            else
            {
                m_Animator.SetBool("strafe", false);
            }

            m_Animator.SetFloat("Horizontal", horizontal, 0.1f, Time.deltaTime);
            m_Animator.SetFloat("Vertical", vertical, 0.1f, Time.deltaTime);

            if (prone)
            {
                m_Animator.SetBool("prone", true);
                camController.maxPitch = 45;
            }
            else
            {
                m_Animator.SetBool("prone", false);
                camController.maxPitch = maxCamOriginal;
            }

            if (vaulting)
            {
                //m_Animator.SetBool("vaulting", true);
            }
            else
            {
                //m_Animator.SetBool("vaulting", false);
            }

            m_Animator.SetFloat("Turn", m_TurnAmount * 0.3f, 0.1f, Time.deltaTime);
            m_Animator.SetBool("Crouch", m_Crouching);
           // m_Animator.SetBool("slide", m_Sliding);
            m_Animator.SetBool("OnGround", m_Grounded);

            if (!m_Grounded)
            {
                m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);
            }

            // calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
            float runCycle =
                Mathf.Repeat(
                    m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
            float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
            if (m_Grounded)
            {
                m_Animator.SetFloat("JumpLeg", jumpLeg);
            }

            // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
            // which affects the movement speed because of the root motion.
            if (m_Grounded && move.magnitude > 0)
            {
                m_Animator.speed = m_AnimSpeedMultiplier;
            }
            else
            {
                // don't use that while airborne
                m_Animator.speed = 1;
            }
        }
        public void CheckGroundStatus(bool vaulting)
        {
            RaycastHit hitInfo;
#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
#endif
            // 0.1f is a small offset to start the ray from inside the character
            // it is also good to note that the transform position in the sample assets is at the base of the character
            if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
            {

                m_GroundNormal = hitInfo.normal;
                m_Grounded = true;
                m_Animator.applyRootMotion = true;
            }
            else if (!vaulting)
            {
                m_Grounded = false;
                m_GroundNormal = Vector3.up;
                m_Animator.applyRootMotion = false;
            }
        }

        void ScaleCapsuleForCrouching(bool crouch)
        {
            if (m_Grounded && crouch)
            {
                if (m_Crouching) return;
                m_Capsule.height = m_Capsule.height / 2f;
                m_Capsule.center = m_Capsule.center / 2f;
                m_Crouching = true;
            }
            else
            {
                Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
                float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
                if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    m_Crouching = true;
                    return;
                }
                m_Capsule.height = m_CapsuleHeight;
                m_Capsule.center = m_CapsuleCenter;
                m_Crouching = false;
            }
        }
        void ScaleCapsuleForSliding(bool slide)
        {
            if (m_Grounded && slide)
            {
                if (m_Sliding) return;
                m_Capsule.height = m_Capsule.height / 2f;
                m_Capsule.center = m_Capsule.center / 2f;
                m_Sliding = true;
            }
            else
            {
                Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
                float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
                if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    m_Sliding = true;
                    return;
                }
                m_Capsule.height = m_CapsuleHeight;
                m_Capsule.center = m_CapsuleCenter;
                m_Sliding = false;
            }
        }
        void PreventStandingInLowHeadroom()
        {
            // prevent standing up in crouch-only zones
            if (!m_Crouching)
            {
                Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
                float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
                if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    m_Crouching = true;
                }
            }
        }


    }

}

