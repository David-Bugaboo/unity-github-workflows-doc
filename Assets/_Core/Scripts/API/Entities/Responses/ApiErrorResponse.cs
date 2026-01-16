using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ApiErrorResponse
{
    public List<string> message;
    public string error;
    public int statusCode;
}