using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PlanetMaenad.FPS
{
    public class HealthUIController : MonoBehaviour
    {
        public Text uiHealth;
        public Slider uiHealthSlider;
        public MeshRenderer HealthMeshFill;
        [Space(10)]

        //public Text uiBullets;
        public Transform DamageCrosshairParent;
        [Space(10)]


        public int MaximumScore = 1500;
        [Space(5)]
        public int CurrentKillCount;
        public Text CurrentText;
        [Space(5)]
        public int PreviousHighScore;
        public Text CurrentHighScore;
        [Space(10)]

        public UnityEvent OnMaximumScoreReached;



        void Start()
        {
            
        }


        public void UpdateKillHUD()
        {
            if (CurrentKillCount < MaximumScore)
            {
                CurrentText.text = CurrentKillCount.ToString();

                if (PreviousHighScore > 0) CurrentHighScore.text = PreviousHighScore.ToString();
            }
            if (CurrentKillCount >= MaximumScore)
            {
                CurrentText.text = CurrentKillCount.ToString();

                if (PreviousHighScore > 0) CurrentHighScore.text = PreviousHighScore.ToString();

                OnMaximumScoreReached.Invoke();
            }

        }



    }
}
