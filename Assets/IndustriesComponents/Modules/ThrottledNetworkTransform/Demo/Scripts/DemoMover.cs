using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.PerformanceTools;

public class DemoMover : NetworkBehaviour
{
    [SerializeField]
    float moveLength = 1;
    [SerializeField]
    float timeScale = 1;

    public enum MoveKind
    {
        Circulary,
        XAxis,
        YAxis
    }

    [Header("Move kind")]
    [SerializeField]
    MoveKind moveKind;
    [SerializeField]
    bool useLocalMove = false;
    float startTime = 0;

    [Header("Throttling delay")]
    [SerializeField]
    int delayBeforeThrottlingMs = 2000;
    [SerializeField]
    bool delayMovement = true;
    [SerializeField]
    int delayBeforeMovement = 2000;
    
    [Header("Extrapolation")]
    [SerializeField]
    bool extrapolate = true;

    TrailRenderer trail;
    NetworkTransformRefreshThrottler throttler;


    private void Awake()
    {
        trail = GetComponentInChildren<TrailRenderer>();
        throttler = GetComponent<NetworkTransformRefreshThrottler>();
    }

    private void OnEnable()
    {
        startTime = Time.time;
    }

    public override void Spawned()
    {
        base.Spawned();
        if (trail)
        {
            if (Object.HasStateAuthority)
            {
                trail.startColor = Color.green;
                trail.endColor = Color.green;
            }
            else
            {
                trail.startColor = Color.red;
                trail.endColor = Color.red;
            }
        }
        DelayThrottling();

    }

    async void DelayThrottling()
    {
        if (Object.HasStateAuthority)
        {
            throttler.IsThrottled = false;
            if(delayMovement) enabled = false;
            await System.Threading.Tasks.Task.Delay(delayBeforeThrottlingMs);
            throttler.IsThrottled = true;
            await System.Threading.Tasks.Task.Delay(delayBeforeMovement);
            if (delayMovement) enabled = true;
        }
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (Object.HasStateAuthority == false) return;
        UpdatePos();
    }

    public override void Render()
    {
        base.Render();
        if (Object.HasStateAuthority == false) return;
        if (extrapolate == false) return;
        // Extrapolate
        UpdatePos();
    }

    void UpdatePos()
    {
        Vector3 pos;
        var time = (Time.time - startTime);
        if (moveKind == MoveKind.XAxis)
        {
            pos = new Vector3(Mathf.PingPong(timeScale * time, moveLength), 0, 0);
        }
        else if(moveKind == MoveKind.YAxis)
        {
            pos = new Vector3(0, Mathf.PingPong(timeScale * time, moveLength), 0);
        }
        else
        {
            pos = moveLength * (Quaternion.Euler(0, 0, time * 180 * timeScale) * Vector3.left);
        }

        if (useLocalMove)
            transform.localPosition = pos;
        else
            transform.position = pos;
    }
}
