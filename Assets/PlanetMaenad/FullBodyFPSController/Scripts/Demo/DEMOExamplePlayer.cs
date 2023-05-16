using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PlanetMaenad.FPS
{
    public class DEMOExamplePlayer : MonoBehaviour
    {
        public bool LockCursor = true;
        [Space(10)]


        public FPSBodyController PlayerBodyController;
        public FPSArmsController PlayerArmsController;
        public HealthController healthControl;
        [Space(5)]
        public Animator BodyAnimator;
        public Animator ArmsAnimator;
        [Space(10)]

        public HealthUIController HUDController;
        public DEMOKillCounter KillCounter;
        [Space(10)]



        public KeyCode BlockButton = KeyCode.Mouse2;
        public KeyCode KickButton = KeyCode.F;
        [Space(10)]


        public float kickTime = .8f;
        public Collider KickTrigger;
        public Transform DamageCrosshairHolder;
        [Space(10)]


        public bool UseRandomDialogue;
        public float DialoguePercentageChance = 0.5f;
        public AudioSource DialogueAudioSource;
        public AudioClip[] DialogueAudioClips;
        public GameObject[] DialogueHUDTexts;



        internal float DialogueTimer = 0f;
        internal bool IsBlocking;

        void Start()
        {
            if (!HUDController) HUDController = GameObject.FindGameObjectWithTag("HUD").GetComponent<HealthUIController>();
            if (!KillCounter) KillCounter = GameObject.FindGameObjectWithTag("KillCounter").GetComponent<DEMOKillCounter>();

            if (LockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        void Update()
        {
            if (UseRandomDialogue)
            {
                HandleRandomDialogue();
            }

            CheckInputValues();
            UpdateAnimator();
        }


        public void CheckInputValues()
        {
            //Kick
            if (Input.GetKeyDown(KickButton))
            {
                Kick();
            }

            //Block
            if (Input.GetKey(BlockButton))
            {
                BlockInput(true);

            }
            if (Input.GetKeyUp(BlockButton))
            {
                BlockInput(false);
            }

        }
        public void HandleRandomDialogue()
        {
            if (DialogueAudioClips.Length > 0)
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
        public void UpdateAnimator()
        {
            ArmsAnimator.SetBool("Block", IsBlocking);
        }

        void BlockInput(bool newBlockState)
        {
            IsBlocking = newBlockState;
            healthControl.IsBlocking = newBlockState;
        }

        public void Kick()
        {
            PlayerBodyController._animator.CrossFade("Kick", 0.1f);
            KickTrigger.enabled = true;

            Invoke("kickCancel", kickTime);
        }
        public void kickCancel()
        {
            KickTrigger.enabled = false;
        }
    }


}
