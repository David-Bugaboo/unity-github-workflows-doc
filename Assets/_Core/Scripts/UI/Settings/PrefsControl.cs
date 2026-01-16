using UnityEngine;
using static UnityEngine.PlayerPrefs;

public class PrefsControl
{// this might as well be called IELPrefsControl, should be using a file instead of this though

    const string USER_EMAIL = "LastEmail",
        USER_PASS = "LastPass",
        USER_DATA = "VisitorData",
        USER_DATA_AVATAR_URL = "avatarURL",
        EVENT_NAME = "SPACES_NAVIGATION_GROUPID", // Mesmo que o Space usa
        CHAT_KEY = "User_{0}_Channel_{1}";

    // this should pass the credentials still encrypted, the api class should decrypt it
    public static (string, string) LastCredentials() => new(GetString(USER_EMAIL, string.Empty), GetString(USER_PASS, string.Empty));
    public static T GetUserData<T>() => JsonUtility.FromJson<T>(GetString(USER_DATA, string.Empty));
    public static void SetCredentials(string email, string pass)
    {
        SetString(USER_EMAIL, email);
        SetString(USER_PASS, pass);
    }
    public static void SetVisitorData(string jsonData) => SetString(USER_DATA, jsonData);
    public static void SetPhotonUserData(string name, string avatarUrl) { SetString("userName", name); SetString("avatarURL", avatarUrl); }

    public static void SetUserAvatarURL(string url) => SetString(USER_DATA_AVATAR_URL, url);
    public static void SetEvent(string id) => SetString(EVENT_NAME, id);
    public static string GetCurrentEventId() => GetString(EVENT_NAME);
    public static void SaveReadChatChannel( string channel, string userMail, int id ) => SetInt( string.Format( CHAT_KEY, userMail, channel ), id );
    public static int LoadReadChatChannel( string channel, string userMail ) => GetInt( string.Format( CHAT_KEY, userMail, channel ), 0 );
    public static void SetCustomData( string id, string jsonData ) => SetString( id, jsonData );
    public static string GetCustomData( string id ) => GetString( id, null );
}
