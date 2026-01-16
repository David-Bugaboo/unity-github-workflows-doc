using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RegistrationData
{
    public string name;
    public string password;
    public string cpf;
    public string email;
    public string main_interest;
    public string bio;
    public bool onboarded;
    public string avatar;
    public string interests;
}

[Serializable]
public class PatchRegistrationData
{
    public string name;
    public string main_interest;
    public string bio;
    public bool onboarded;
    public string avatar;
    public string interests;
}
