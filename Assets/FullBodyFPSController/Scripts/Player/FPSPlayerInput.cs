using System;
using System.Collections.Generic;
using UnityEngine;


namespace scgFullBodyController
{

    [Tooltip("IMPORTANT, this script needs to be on the root transform")]
    public class FPSPlayerInput : MonoBehaviour
    {
        public FPSPlayerMover PlayerMover;
        [Space(10)]

        public KeyCode CrouchButton = KeyCode.LeftControl;
        public KeyCode SprintButton = KeyCode.LeftShift;
        public KeyCode ProneButton = KeyCode.Z;
        public KeyCode JumpButton = KeyCode.Space;
        public KeyCode KickButton = KeyCode.Mouse2;
        public KeyCode SlideButton = KeyCode.Q;
        public KeyCode VaultButton = KeyCode.V;


        [HideInInspector] public bool slide;
        bool crouchToggle = false;
        bool proneToggle = false;
        bool crouch = false;
        bool prone = false;
        bool sprint = false;
        bool canVault = false;
        bool vaulting = false;
        bool strafe;
        bool forwards;
        bool backwards;
        bool right;
        bool left;
        float horizontalInput;
        float verticalInput;


        internal Transform m_Cam;
        internal Vector3 m_CamForward;
        internal Vector3 m_Move;
        internal bool m_Jump;

        internal GameObject collidingObj;

        float maxCamOriginal;
        float minCamOriginal;
        bool toggle;



        void Start()
        {
            // get the transform of the main camera
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
            }


        }
        void Update()
        {
            verticalInput = Input.GetAxis("Vertical");
            horizontalInput = Input.GetAxis("Horizontal");

            if (!m_Jump)
            {
                m_Jump = Input.GetKeyDown(JumpButton);
            }
            if (m_Jump && canVault)
            {
                //collidingObj.GetComponent<Collider>().enabled = false;

                m_Jump = false;
                vaulting = true;

                Invoke("vaultCancel", PlayerMover.vaultCancelTime);
            }
            if (vaulting)
            {
                m_Jump = false;
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
            if (Input.GetKeyDown(SlideButton) && Input.GetKey(KeyCode.W))// && PlayerMover.m_Grounded)
            {
                PlayerMover.m_Animator.CrossFade("Slide", 0.1f);

                slide = true;
                crouch = false;

                Invoke("slideCancel", PlayerMover.slideTime);
            }
            if (Input.GetKeyDown(SlideButton) && Input.GetKey(KeyCode.S))// && PlayerMover.m_Grounded)
            {
                PlayerMover.m_Animator.CrossFade("SlideBack", 0.1f);

                slide = true;
                crouch = false;

                Invoke("slideCancel", PlayerMover.slideTime);
            }

            //Sprint
            if (Input.GetKey(SprintButton))
            {
                sprint = true;
            }
            else
            {
                sprint = false;
            }

            //Kick
            if (Input.GetKeyDown(KickButton))
            {
                PlayerMover.Kick();
            }

            //Kick
            if (Input.GetKeyDown(VaultButton) && canVault)
            {
                PlayerMover.Vault();
            }


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



            //Strafe
            if (Input.GetAxis("Horizontal") > 0 || Input.GetAxis("Horizontal") < 0)
            {
                strafe = true;
            }
            else
            {
                strafe = false;
            }


            PlayerMover.HandleInputUpdate(m_Move, crouch, prone, vaulting, forwards, backwards, strafe, horizontalInput, verticalInput);
        }
        void FixedUpdate()
        {
            // calculate camera relative direction to move:
            m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
            m_Move = verticalInput * m_CamForward * PlayerMover.walkSpeed;

            if (sprint) m_Move *= PlayerMover.sprintSpeed;


            // pass all parameters to the character control script
            PlayerMover.Move(m_Move, crouch, m_Jump, slide, vaulting);
            PlayerMover.HandleMovement(crouch, m_Jump, slide);

            m_Jump = false;
        }

        public void slideCancel()
        {
            slide = false;
        }
        public void vaultCancel()
        {
            vaulting = false;
            canVault = false;

            collidingObj.GetComponent<Collider>().enabled = true;
        }



        void OnCollisionEnter(Collision col)
        {
            if (col.transform.tag == "Vaultable")
            {
                collidingObj = col.gameObject;
                canVault = true;
            }
            else
            {
                canVault = false;
            }
        }
        void OnCollisionExit(Collision col)
        {
            canVault = false;
        }

    }

}
