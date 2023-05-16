using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class DemoGPU_AIWander : MonoBehaviour
{
    public NavMeshAgent agent;
    public DemoGPU_Animator GPUMeshAnimator;
    [Space(10)]

    public float DefaultSpeed;
    public float CurrentVelocity;
    [Space(5)]
    public bool UseWander;
    public float wanderRadius;
    public float wanderTimer;
    [Space(10)]



    public string[] IdleNames;
    public string[] WalkNames;
    [Space(5)]
    public UnityEvent OnIdle;
    public UnityEvent OnWalk;
    [Space(10)]




    internal Transform target;
    internal float timer;
    internal bool IsWalking;

    internal float AnimationOffset;
    internal string selectedIdle;
    internal string selectedWalk;

    internal WaitForEndOfFrame EndOfFrameDelay = new WaitForEndOfFrame();
    internal WaitForSeconds TinyDelay = new WaitForSeconds(0.1f);
    internal WaitForSeconds ShortDelay = new WaitForSeconds(0.2f);
    internal WaitForSeconds DetectionFrequency;
    internal WaitForSeconds AttackFrequency;



    void OnEnable()
    {
        agent.speed = DefaultSpeed;

        selectedIdle = IdleNames[Random.Range(0, IdleNames.Length)];
        selectedWalk = WalkNames[Random.Range(0, WalkNames.Length)];

        wanderTimer = Random.Range(wanderTimer - 3, wanderTimer + 4);
        timer = wanderTimer;

        AnimationOffset = GPUMeshAnimator.CurrentPlayingOffset;
    }


    void Update()
    {
        if (agent)
        {
            if (agent.enabled)
            {
                if (agent.isOnNavMesh)
                {
                    CurrentVelocity = agent.velocity.magnitude;

                    //Check Speed
                    if (CurrentVelocity <= .1f && IsWalking == true)
                    {
                        OnIdle.Invoke();
                        IsWalking = false;
                    }
                    if (CurrentVelocity > .1f && IsWalking == false)
                    {
                        OnWalk.Invoke();
                        IsWalking = true;
                    }

                    //Play Animation
                    if (GPUMeshAnimator)
                    {
                        //Idle
                        if (CurrentVelocity <= .1f)
                        {
                            GPUMeshAnimator.Play(selectedIdle, AnimationOffset); 
                        }
                        //Walking
                        if (CurrentVelocity > .1f)
                        {
                            GPUMeshAnimator.Play(selectedWalk, AnimationOffset); 
                        }
                    }

                    timer += Time.deltaTime;

                    if (timer >= wanderTimer)
                    {
                        //Find New Destination
                        Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);

                        if(UseWander) agent.SetDestination(newPos);

                        timer = 0;
                    }
                }

            }
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }







}
