using UnityEngine;

public class ForceDeviceOrientation : MonoBehaviour
{
    public ScreenOrientation orientation;
    public bool runOnAwake = true;
    public bool runOnStart = false;
    public bool runOnUpdate = false;


    // Start is called before the first frame update
    void Awake()
    {
        if(runOnAwake){
            Screen.orientation = orientation;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if(runOnStart){
            Screen.orientation = orientation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(runOnUpdate){
            Screen.orientation = orientation;
        }
    }
}
