using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Text.RegularExpressions;
using System.Linq;

public class LinkOpener : MonoBehaviour, IPointerClickHandler {

    [SerializeField] TMP_Text target;

    const string LINK_HYPERTEXT = "<link=\"{0}\"><color=#0000EE><u>{1}</u></color></link>";
    const string REGEX_PATTERN = @"(?:(?:https?|ftp):\/\/)?(?:www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b(?:[-a-zA-Z0-9@:%_\+.~#?&//=]*)?";

    private void OnValidate() {
        if (!target) target = GetComponent<TMP_Text>();
    }

    [ContextMenu( "Get Links" )]
    public void GetLinks() {
        var match = Regex.Matches( target.text, REGEX_PATTERN, RegexOptions.IgnoreCase );
        foreach (Match m in match.Cast<Match>()) {
            var regex = new Regex( m.Value );
            target.text = regex.Replace( target.text, string.Format( LINK_HYPERTEXT, m.Value, m.Value ), 1 );
        }
    }

    public void OnPointerClick( PointerEventData eventData ) {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink( target, eventData.position, null ); // reference camera if not overlay
        if (linkIndex < 0) return;
        TMP_LinkInfo linkInfo = target.textInfo.linkInfo[ linkIndex ];
        var link = linkInfo.GetLinkID();
        if (!link.Contains( "www" ))
            link = $"www.{link}";
        Application.OpenURL( link );
    }

}