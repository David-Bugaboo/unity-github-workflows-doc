using System;

[Serializable]
public class UserInfoResponse {

    public AccountInfoResponse AccountInfo;
    // public BadgeResponse[] UserInventory;


    [Serializable]
    public class AccountInfoResponse {
        public string PlayFabId;
        public OpenIdInfoResponse[] OpenIdInfo;

        [Serializable]
        public class OpenIdInfoResponse {
            public string Issuer;
        }
    }
}