using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LookAtTarget : MonoBehaviour
{
    public Canvas WorldSpaceCanvas;
    public Camera _camera;
    public Transform target;
    public bool UseMainCamera;


    void Start()
    {
        if (target == null || UseMainCamera)
        {
            if (!_camera) _camera = Camera.main;
            target = _camera.transform;
        }

        if(WorldSpaceCanvas && UseMainCamera)
        {
            WorldSpaceCanvas.worldCamera = _camera;
        }
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + target.forward);
    }
}
