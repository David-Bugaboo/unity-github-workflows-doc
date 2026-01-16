using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/**
 * 
 * ScreenShareAppWindowsSettings is used to change the resolution when recorder rig is selected
 * 
 **/
public class ScreenShareAppWindowsSettings : MonoBehaviour
{
    
    void Start()
    {
        Debug.Log("Change Windows resolution");
        Screen.SetResolution(640, 360, false, 60);
    }
}
