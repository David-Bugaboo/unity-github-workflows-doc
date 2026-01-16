using System;
using UnityEngine;

[Serializable]
public class FileUploadPayload
{
    public string name; // "lobby1", "banner_esquerda", etc.
    public string url;
    public string type; // "BANNER" ou "PORTIFOLIO"
    public ImageDataPayload img;
}
