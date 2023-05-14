using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetMaenad.FPS
{
    public class FPSPlayerController : MonoBehaviour
    {
        public FPSCameraController camController;
        public HealthController healthControl;
        public Animator _animator;
        public Rigidbody m_Rigidbody;
        public CapsuleCollider PlayerCollider;
        public CharacterController _controller;
        [Space(10)]


        public KeyCode CrouchButton = KeyCode.LeftControl;
        public KeyCode SprintButton = KeyCode.LeftShift;
        public KeyCode JumpButton = KeyCode.Space;
        public KeyCode ProneButton = KeyCode.Z;
        public KeyCode KickButton = KeyCode.Mouse2;
        public KeyCode SlideButton = KeyCode.Q;
        [Space(10)]

        public float DialoguePercentageChance = 0.5f;
        public AudioSource DialogueAudioSource;
        public AudioClip[] DialogueAudioClips;
        public GameObject[] DialogueHUDTexts;
        [Space(5)]



        public bool IsGrounded;
        public bool IsCrouching;
        public bool IsSliding;
        public bool IsSprinting;
        [Space(5)]
        public LayerMask GroundLayers;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        [Space(10)]


        public float JumpHeight = 1.2f;
        public float Gravity = -9.81f;
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;
        [Space(10)]


        public float MoveSpeed = 3f;
        public float SprintSpeed = 7f;
        [Space(5)]
        public float sensitivity = 1f;
        public float SpeedChangeRate = 10.0f;
        [Range(0.0f, 0.3f)] public float RotationSmoothTime = 0.12f;
        [Space(10)]



        public float slideTime = .8f;
        public float kickTime = .8f;
        public float vaultCancelTime = 1.5f;
        [Space(5)]
        public Collider KickTrigger;
        public float m_RunCycleLegOffset = 0.2f;
        public float m_AnimSpeedMultiplier = 1f;
        [Space(10)]



        #region Internal Variables

        internal float DialogueTimer = 0f;

        //These are messy, combined my personal code.  Some of these can be deleted, havent pruned the extra ones yet
        internal float _verticalVelocity;
        internal Vector2 move;

        internal bool jump;
        internal bool sprint;

        internal float _initialCapsuleHeight;
        internal float _initialCapsuleRadius;

        internal float _speed;
        internal float _animationBlend;
        internal float _targetRotation = 0.0f;
        internal float _rotationVelocity;
        internal float _terminalVelocity = 53.0f;

        internal float _jumpTimeoutDelta;
        internal float _fallTimeoutDelta;
        internal float m_OrigGroundCheckDistance;

        const float k_Half = 0.5f;

        internal float m_TurnAmount;
        internal float m_ForwardAmount;

        internal float m_CapsuleHeight;
        internal Vector3 m_CapsuleCenter;


        bool crouchToggle = false;
        bool proneToggle = false;
        bool crouch = false;
        bool prone = false;
        bool slide;

        bool strafe;
        bool forwards;
        bool backwards;
        bool right;
        bool left;

        float horizontalInput;
        float verticalInput;

        internal Vector3 m_CamForward;

        float maxCamOriginal;
        float minCamOriginal;
        bool toggle;
        bool m_Jump;

        #endregion



        void Start()
        {
            Physics.IgnoreCollision(PlayerCollider, _controller, true);

            m_CapsuleHeight = PlayerCollider.height;
            m_CapsuleCenter = PlayerCollider.center;

            _initialCapsuleHeight = _controller.height;
            _initialCapsuleRadius = _controller.radius;

            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

            maxCamOriginal = camController.maxPitch;
            m_OrigGroundCheckDistance = 0.3f;

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }
        void Update()
        {
            CheckInputValues();
            CheckGrounded();
            CheckAnimator();

            DoRandomDialogue();

            //Sprint
            if (Input.GetKeyDown(SprintButton))
            {
                SprintInput(true);
                IsSprinting = true;
            }
            if (Input.GetKeyUp(SprintButton))
            {
                SprintInput(false);
                IsSprinting = false;
            }


            //Crouch
            if (Input.GetKeyDown(CrouchButton))
            {
                crouchToggle = !crouchToggle;
                if (crouchToggle)
                {
                    crouch = true;
                }
                else
                {
                    crouch = false;
                }
            }
            if (Input.GetKeyDown(CrouchButton) && prone)
            {
                proneToggle = !proneToggle;
                crouch = true;
                prone = false;
            }


            //Jump
            if (Input.GetKey(JumpButton))
            {
                JumpInput(true);
            }
            if (Input.GetKeyUp(JumpButton))
            {
                JumpInput(false);
            }


            //Prone
            if (Input.GetKeyDown(ProneButton) && crouch)
            {
                crouchToggle = !crouchToggle;
                crouch = false;
                prone = true;
            }
            if (Input.GetKeyDown(ProneButton))
            {
                proneToggle = !proneToggle;

                if (proneToggle)
                {
                    prone = true;
                }
                else
                {
                    prone = false;
                }
            }

            //Slide
            if (Input.GetKeyDown(SlideButton) && Input.GetKey(KeyCode.W))
            {
                _animator.CrossFade("Slide", 0.1f);

                slide = true;
                crouch = false;

                Invoke("slideCancel", slideTime);
            }
            if (Input.GetKeyDown(SlideButton) && Input.GetKey(KeyCode.S))
            {
                _animator.CrossFade("SlideBack", 0.1f);

                slide = true;
                crouch = false;

                Invoke("slideCancel", slideTime);
            }

            //Kick
            if (Input.GetKeyDown(KickButton))
            {
                _animator.CrossFade("Kick", 0.1f);
                KickTrigger.enabled = true;

                Invoke("kickCancel", kickTime);
            }



            //Forward Direction
            if (Input.GetAxis("Vertical") < 0)
            {
                backwards = true;
            }
            else if (Input.GetAxis("Vertical") > 0)
            {
                forwards = true;
            }
            else if (Input.GetAxis("Vertical") == 0)
            {
                backwards = false;
                forwards = false;
            }


            //Horizontal Strafe
            if (Input.GetAxis("Horizontal") > 0 || Input.GetAxis("Horizontal") < 0)
            {
                strafe = true;
            }
            else
            {
                strafe = false;
            }


            StandardMove();
            StandardJumpAndGravity();
        }


        void CheckGrounded()
        {
            //Spherecast Offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);

            //Standard Ground Check
            if (_controller.enabled)
            {
                IsGrounded = _controller.isGrounded;
            }
        }
        void CheckInputValues()
        {
            move.x = Input.GetAxis("Horizontal");
            move.y = Input.GetAxis("Vertical");

            horizontalInput = move.x;
            verticalInput = move.y;
            m_ForwardAmount = verticalInput;

            //_animator.SetFloat("Mouse X", Input.GetAxis("Mouse X"));
            //_animator.SetFloat("Mouse Y", Input.GetAxis("Mouse Y"));

            _animator.SetBool("IsGrounded", IsGrounded);
            _animator.SetBool("IsJumping", jump);
        }        
        void CheckAnimator()
        {
            // update the animator parameters

            if (backwards)
            {
                _animator.SetFloat("Forward", m_ForwardAmount * -1, 0.1f, Time.deltaTime);
                _animator.SetFloat("animSpeed", -1);
            }
            else if (forwards)
            {
                _animator.SetFloat("animSpeed", 1);
                _animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
            }
            else
            {
                _animator.SetFloat("animSpeed", 1);
                _animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
            }

            if (strafe)
            {
                _animator.SetBool("strafe", true);
            }
            else
            {
                _animator.SetBool("strafe", false);
            }

            _animator.SetFloat("Horizontal", horizontalInput, 0.1f, Time.deltaTime);
            _animator.SetFloat("Vertical", verticalInput, 0.1f, Time.deltaTime);

            if (prone)
            {
                _animator.SetBool("prone", true);
                camController.maxPitch = 45;
            }
            else
            {
                _animator.SetBool("prone", false);
                camController.maxPitch = maxCamOriginal;
            }


            _animator.SetFloat("Turn", m_TurnAmount * 0.3f, 0.1f, Time.deltaTime);
            _animator.SetBool("Crouch", IsCrouching);

            if (!IsGrounded)
            {
                _animator.SetFloat("Jump", _verticalVelocity);
            }

            // calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
            float runCycle =
                Mathf.Repeat(
                    _animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
            float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
            if (IsGrounded)
            {
                _animator.SetFloat("JumpLeg", jumpLeg);
            }

            // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
            // which affects the movement speed because of the root motion.
            if (IsGrounded && move.magnitude > 0)
            {
                _animator.speed = m_AnimSpeedMultiplier;
            }
            else
            {
                // don't use that while airborne
                _animator.speed = 1;
            }
        }

        void StandardMove()
        {
            if (enabled)
            {
                //Current Speed
                float targetSpeed = sprint ? SprintSpeed : MoveSpeed;

                //No Input
                if (move == Vector2.zero) targetSpeed = 0.0f;

                //Horizontal Speed     
                float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
                float speedOffset = 0.1f;

                //Acceleration
                if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
                {
                    _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * 1, Time.deltaTime * SpeedChangeRate);
                    _speed = Mathf.Round(_speed * 1000f) / 1000f;
                }
                else
                {
                    _speed = targetSpeed;
                }


                //Input direction
                Vector3 inputDirection = new Vector3(move.x, 0.0f, move.y).normalized;

                //MainCamera Rotation
                var StrafeRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + camController.MainCam.transform.eulerAngles.y;
                _targetRotation = StrafeRotation;
                var SmoothRotation = Mathf.Lerp(transform.eulerAngles.y, camController.MainCam.transform.eulerAngles.y, RotationSmoothTime);

                transform.rotation = Quaternion.Euler(0.0f, camController.MainCam.transform.eulerAngles.y, 0.0f);

                //Forward Direction Check
                Vector3 ForwardDirection = Quaternion.Euler(0, _targetRotation, 0) * Vector3.forward;
                var GravityDirection = new Vector3(0, _verticalVelocity, 0);

                if(_controller.enabled) _controller.Move(ForwardDirection.normalized * (_speed * Time.deltaTime) + GravityDirection * Time.deltaTime);


                //Animator Lock
                //_animator.SetFloat("HorizontalMagnitude", Input.GetAxis("Horizontal"));
                //_animator.SetFloat("VerticalMagnitude", Input.GetAxis("Vertical"));

                _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);

                //_animator.SetFloat("InputSpeed", _animationBlend);
                //_animator.SetFloat("MotionSpeed", 1);

                if (_animationBlend < 0.01f)
                {
                    _animationBlend = 0f;
                }
            }
        }
        void StandardJumpAndGravity()
        {

            if (IsGrounded)
            {
                //Clear Fall Timeout
                _fallTimeoutDelta = FallTimeout;
                //_animator.SetBool("FreeFall", false);

                //Clear Vertical Speed
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                //Do Jump
                if (jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    //_animator.CrossFade("Jump", 0.2f);
                }

                //Jump Timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {

                //Clear Jump Timeout
                _jumpTimeoutDelta = JumpTimeout;

                //Fall Timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    //_animator.SetBool("FreeFall", true);
                }

                //Reset Jump
                jump = false;
            }

            //Apply Gravity
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
            m_Jump = jump;
        }
        void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        public void kickCancel()
        {
            KickTrigger.enabled = false;
        }
        public void slideCancel()
        {
            slide = false;
        }




        public void DoRandomDialogue()
        {
            if(DialogueAudioClips.Length > 0)
            {
                DialogueTimer += Time.deltaTime;

                if (DialogueTimer >= 10f)
                {
                    float random = Random.Range(0f, 1f);

                    if (random <= DialoguePercentageChance)
                    {
                        int randomIndex = Random.Range(0, DialogueAudioClips.Length);
                        DialogueAudioSource.clip = DialogueAudioClips[randomIndex];

                        if (!DialogueAudioSource.isPlaying) DialogueAudioSource.Play();
                        if (DialogueHUDTexts.Length > 0) DialogueHUDTexts[randomIndex].SetActive(true);
                    }

                    DialogueTimer = 0f;
                }
            }       
        }
    }

}
