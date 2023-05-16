using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetMaenad.FPS
{
    public class DEMOKillCounterEntity : MonoBehaviour
    {
        internal DEMOKillCounter killCounter;


        void Start()
        {
            killCounter = GameObject.FindGameObjectWithTag("KillCounter").GetComponent<DEMOKillCounter>();
        }


        void OnDestroy()
        {
            if(killCounter)
            {
                killCounter.CurrentKillCount += 1;
                killCounter.UpdateKillHUD();
            }

        }


    }
}
