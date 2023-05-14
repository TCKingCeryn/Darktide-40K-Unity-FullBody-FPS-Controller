using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimpleEvents : MonoBehaviour
{
    public bool DoOnStart;
    public bool DoOnEnable;

    [System.Serializable]
    public class StartEvent
    {
        public float Delay;
        public UnityEvent OnEvent;

        public IEnumerator DoEventDelay()
        {
            yield return new WaitForSeconds(Delay);

            OnEvent.Invoke();
        }
    }
    public StartEvent[] _startEvent;



    void Start()
    {
        if(DoOnStart)
        {
            DoAllEvents();
        }

    }
    void OnEnable()
    {
        if (DoOnEnable)
        {
            DoAllEvents();
        }
    }

    public void DoAllEvents()
    {
        if(_startEvent.Length > 0)
        {
            for (int i = 0; i < _startEvent.Length; i++)  //The "iBall" for-loop Goes through all of the Array.
            {
                StartCoroutine(_startEvent[i].DoEventDelay());
            }
        }
    }

    public void DoEvent(int index)
    {
        if (_startEvent.Length > 0)
        {
            StartCoroutine(_startEvent[index].DoEventDelay());
        }
    }
}
