using UnityEngine;

public class DetectMobileGame : MonoBehaviour
{
#if UNITY_STANDALONE

    void Start()
    {
        gameObject.SetActive(false);
    }

#endif
}
