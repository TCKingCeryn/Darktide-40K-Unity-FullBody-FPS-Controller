using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DestroyWithDelay : MonoBehaviour
{
    public bool DestroyOnStart;
    public bool DestroyOnEnable;
    [Space]
    public float DestroyDelay = 5f;


    void Start()
    {
        if(DestroyOnStart)
        {
            DoDestroy();
        }
    }
    void OnEnable()
    {
        if (DestroyOnEnable)
        {
            DoDestroy();
        }
    }

    public void DoDestroy()
    {
        StartCoroutine(Destroy());
    }
    IEnumerator Destroy()
    {
        yield return new WaitForSeconds(DestroyDelay);

        Destroy(gameObject);
    }


}
