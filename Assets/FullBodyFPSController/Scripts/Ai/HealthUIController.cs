//SlapChickenGames
//2021
//Simple hud referencer

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace scgFullBodyController
{
    public class HealthUIController : MonoBehaviour
    {
        public Text uiHealth;
        public Slider uiHealthSlider;
        [Space(10)]


        public Text uiBullets;
        public GameObject uiCrosshair;
    }
}
