using System;

[Serializable]
public class BaseUserData {
    const string DEFAULT_VAL = "";

    public string email, role, name, matricula, turma, description, rpmUrl, mainInterest;
    public bool onboarded;
    public string interesses;
    protected string[] interests => interesses.Split(',');
    protected bool _visitor;

    public BaseUserData() { 
        mainInterest = rpmUrl = description = turma = matricula = DEFAULT_VAL;
        interesses = ",,,";
    }
}
