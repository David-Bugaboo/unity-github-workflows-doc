using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ImageInfoListWrapper
{
    // O nome "items" é um placeholder. A conversão de um array JSON
    // para um wrapper precisa de uma função auxiliar.
    public List<ImageInfo> items; 
}