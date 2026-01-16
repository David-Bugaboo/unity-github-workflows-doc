using Fusion.Samples.Stage;
using Fusion.XR.Shared.Rig;
using UnityEngine;
using UnityEngine.Events;

public class ProximityTrigger : MonoBehaviour
{
    private Transform reference;
    public static Transform target;
    public float enterRange = 4f;
    public float exitRange = 4f;

    public UnityEvent onEnterRange;
    public UnityEvent onExitRange;

    private Seat seat;
    
    [SerializeField] bool isNear = false;

    RigInfo _rigInfo;
    HardwareRig _rig;

    public bool startActivated = false;
    private void Start()
    {
        seat = GetComponent<Seat>();
        if (!startActivated) onExitRange.Invoke();
    }

    void FixedUpdate()
    {
        if (reference == null) reference = transform;
        if ( _rigInfo == null ) { if ( ( _rigInfo = FindObjectOfType<RigInfo>() ) == null ) return; }// make an event for when the player logs in and subscribe to it
        if ( (_rig = _rigInfo.localHardwareRig) == null ) return;
        if ( (target = _rig.transform) == null ) return;

        float distance = Vector3.Distance(reference.position, target.position);
        bool checkEnter = distance < enterRange;
        bool checkExit = distance > exitRange;

        if (checkEnter && !isNear)
        {
            onEnterRange.Invoke();
            isNear = true;
        }

        if (checkExit && isNear)
        {
            onExitRange.Invoke();
            isNear = false;
        }
    }

    // void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         seat.beamHoverable.onBeamRelease?.Invoke(); 
    //     }
    // }
}