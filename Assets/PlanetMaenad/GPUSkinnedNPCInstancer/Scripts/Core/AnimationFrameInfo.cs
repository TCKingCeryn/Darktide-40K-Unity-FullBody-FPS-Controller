using System;
using UnityEngine;

[Serializable]
public class AnimationFrameInfo
{
    public string Name;
    [Space(10)]

    public int FrameCount;
    [Space(10)]

    public int StartFrame;
    public int EndFrame;
    

    //public bool UseForce;
    //public int ForceFrame;
    //[Space(5)]
    //public float UpwardForce = 15f;
    //public float ForwardForce = 10f;
    //public Vector3 TorqueAngle;


    public AnimationFrameInfo(string name, int startFrame, int endFrame, int frameCount)//, bool useForce, int forceFrame, float upwardForce, float forwardForce, Vector3 torqueAngle)
    {
        Name = name;

        FrameCount = frameCount;

        StartFrame = startFrame;
        EndFrame = endFrame;

        //UseForce = useForce;
        //ForceFrame = forceFrame;
        //UpwardForce = upwardForce;
        //ForwardForce = forwardForce;
        //TorqueAngle = torqueAngle;
    }
}