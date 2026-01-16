using UnityEngine;

namespace IEL.UI {
    public class UI_LoadingScreen : MonoBehaviour {
        [SerializeField] EnableDisableAnimation anim;
        public void StartLoading() => gameObject.SetActive( true );//anim.OnContentIn();
        public void StopLoading() => gameObject.SetActive( false );//anim.OnContentOff();
    }
}
