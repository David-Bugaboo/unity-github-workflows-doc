using System;
using Fusion;
using UnityEngine;

public class Buga_NetworkAnimationSync : MonoBehaviour
{
    public NetworkTransform networkTransform;
    public NetworkMecanimAnimator networkAnimator;
    public float increaseSmooth = 5f;
    public float decreaseSmooth = 1.5f;

    Vector3 lastPosition = Vector3.zero;
    float lastTime = 0f;

    float targetSpeed;

    private void Start()
    {
        enabled = networkAnimator.IsProxy;
        if (networkTransform == null) networkTransform = GetComponent<NetworkTransform>();
        if (networkAnimator == null) networkAnimator = GetComponent<NetworkMecanimAnimator>();
    }

    private void FixedUpdate()
    {
        if (!networkAnimator.IsProxy) return;
        
        Vector3 displacement = networkTransform.Data.Position - lastPosition;
        float deltaTime = Time.time - lastTime;
        
        Vector3 velocity = displacement / deltaTime;
        targetSpeed = velocity.magnitude;
        
        float currentValue = networkAnimator.Animator.GetFloat("MoveSpeed");
        
        var moveValue = Mathf.Lerp(currentValue, targetSpeed,
            Time.deltaTime * (targetSpeed > currentValue ? increaseSmooth : decreaseSmooth));
        
        networkAnimator.Animator.SetFloat("MoveSpeed", moveValue);
        
        lastPosition = networkTransform.Data.Position;
        lastTime = Time.time;
    }
}
