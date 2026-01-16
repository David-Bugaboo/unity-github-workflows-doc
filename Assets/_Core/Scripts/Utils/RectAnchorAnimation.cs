using UnityEngine;
using DG.Tweening;
using DG.Tweening.Plugins.Options;
using DG.Tweening.Core;
// ADICIONADO: Namespace necessário para usar UnityEvent
using UnityEngine.Events;

public class RectAnchorAnimation : MonoBehaviour {
    [SerializeField] float Duration, lower_Y, mid_Y, higher_Y;
    RectTransform Rect;
    TweenerCore<Vector2, Vector2, VectorOptions> Tween;
    [SerializeField] Vector2 DefaultMin, DefaultMax;
    
    [Header("Eventos")]
    [Tooltip("Evento que será disparado ao final de qualquer animação de âncora.")]
    public UnityEvent OnAnimationComplete;

    private void Awake() => Rect = (RectTransform)transform;
    
    public void ResetAnchor() {
        Tween?.Kill();
        Rect.DOAnchorMin( DefaultMin, Duration );
        
        Rect.DOAnchorMax( DefaultMax, Duration ).OnComplete(() => {
            OnAnimationComplete?.Invoke();
        });
    }

    public void SetMaxAnchorY( float Val )
    {
        CallTween(Rect.DOAnchorMax(new Vector2(Rect.anchorMax.x, Val), Duration), Val);
    }

    public void SetMinAnchorY( float Val ) => CallTween( Rect.DOAnchorMin( new Vector2( Rect.anchorMin.x, Val ), Duration ), Val );
    public void SetMaxAnchorX( float Val ) => CallTween( Rect.DOAnchorMax( new Vector2( Val, Rect.anchorMax.y ), Duration ), Val );
    public void SetMinAnchorX( float Val ) => CallTween( Rect.DOAnchorMin( new Vector2( Val, Rect.anchorMin.y ), Duration ), Val );
    
    void CallTween( TweenerCore<Vector2, Vector2, VectorOptions> Target, float val) {
        Tween?.Kill();
        Tween = Target;
        
        Tween.OnComplete(() => {
            if(val <= 0) OnAnimationComplete?.Invoke();
        });
    }

    public void SetVerticalAnchorToZero() => SetMaxAnchorY( 0 );
    public void SetVerticalAnchorToLower() => SetMaxAnchorY( lower_Y );
    public void SetVerticalAnchorToMid() => SetMaxAnchorY( mid_Y );
    public void SetVerticalAnchorToHigher() => SetMaxAnchorY( higher_Y );
}