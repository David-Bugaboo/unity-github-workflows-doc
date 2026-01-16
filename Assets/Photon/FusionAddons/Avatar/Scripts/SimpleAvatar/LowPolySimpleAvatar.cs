using System.Collections.Generic;
using Fusion.Samples.IndustriesComponents;
using UnityEngine;

public class LowPolySimpleAvatar : MonoBehaviour, IAvatar
{
    [Header("Componentes Visuais")]
    public MeshRenderer hairRenderer;
    public MeshFilter hairMeshFilter;
    public MeshRenderer bodyRenderer;
    public MeshRenderer clothRenderer;

    [Header("Opções do Avatar")]
    public List<Mesh> hairMeshes = new List<Mesh>();

    private AvatarRepresentation avatarRepresentation;
    private AvatarStatus status = AvatarStatus.NotLoaded;

    #region Implementação da Interface IAvatar

    public AvatarKind AvatarKind => AvatarKind.SimpleAvatar;
    public AvatarStatus AvatarStatus => status;
    public GameObject AvatarGameObject => this.gameObject;

    // Este avatar é simples e carregado localmente, então não precisa de uma URL externa.
    public AvatarUrlSupport SupportForURL(string url)
    {
        // Verifica se a URL começa com o prefixo para este tipo de avatar
        if (url.StartsWith("simpleavatar://"))
        {
            return AvatarUrlSupport.Compatible;
        }
        return AvatarUrlSupport.Incompatible;
    }

    // Este é o novo ponto de entrada para a customização!
    public void ChangeAvatar(string avatarURL)
    {
        status = AvatarStatus.RepresentationLoading;
        // A URL agora carrega as informações que antes estavam em AvatarDescription.
        // Ex: "simpleavatar://skin=1,0,0&cloth=0,1,0&hair=0,0,1&hairmesh=2&bald=false"
        
        // --- Lógica de Parse da URL (Substitui LoadAvatarDescription) ---
        var parameters = ParseSimpleAvatarURL(avatarURL);

        // Aplica as cores
        LoadAvatarColors(parameters.skinColor, parameters.clothColor, parameters.hairColor);

        // Aplica o estilo de cabelo
        if (parameters.isBald) {
            LoadAvatarBaldness(true);
        } else {
            LoadAvatarBaldness(false);
            LoadAvatarLODHairStyle(parameters.hairStyle);
        }
        
        // --- Fim da Lógica ---

        // Ativa o GameObject do avatar se ele estiver desativado
        this.gameObject.SetActive(true);
        status = AvatarStatus.RepresentationAvailable;

        // Notifica o gerente que este avatar está pronto e visível
        if(avatarRepresentation) avatarRepresentation.RepresentationAvailable(this);
    }

    public void RemoveCurrentAvatar()
    {
        this.gameObject.SetActive(false);
        status = AvatarStatus.NotLoaded;
    }

    // Outras propriedades da interface podem ter valores padrão
    public int TargetLODLevel => 0;
    public bool ShouldLoadLocalAvatar => true;
    public string RandomAvatar() { return ""; /* Não implementado para este exemplo */ }

    #endregion

    private void Awake()
    {
        avatarRepresentation = GetComponentInParent<AvatarRepresentation>();
        if (hairMeshFilter == null && hairRenderer != null)
        {
            hairMeshFilter = hairRenderer.GetComponent<MeshFilter>();
        }
        // O avatar começa desativado até ser carregado
        this.gameObject.SetActive(false);
    }
    
    // Os métodos auxiliares agora são privados e não precisam do parâmetro 'IAvatar'
    private void LoadAvatarColors(Color skinColor, Color clothColor, Color hairColor)
    {
        bodyRenderer.material.color = skinColor;
        clothRenderer.material.color = clothColor;
        hairRenderer.material.color = hairColor;
    }

    private void LoadAvatarBaldness(bool isBald)
    {
        hairRenderer.enabled = !isBald;
    }

    private void LoadAvatarLODHairStyle(int hairStyle)
    {
        if (hairStyle >= 0 && hairStyle < hairMeshes.Count)
        {
            hairMeshFilter.sharedMesh = hairMeshes[hairStyle];
        }
    }
    
    // Método auxiliar para interpretar a URL customizada
    private (Color skinColor, Color clothColor, Color hairColor, int hairStyle, bool isBald) ParseSimpleAvatarURL(string url)
    {
        // Lógica de exemplo para extrair os dados da URL.
        // Em um projeto real, isso seria mais robusto.
        Color skin = Color.white;
        Color cloth = Color.blue;
        Color hair = Color.black;
        int hairStyle = 0;
        bool isBald = false;

        // Adicione aqui sua lógica para ler os parâmetros da string 'url'
        // Ex: ... url.Split('&') ...

        return (skin, cloth, hair, hairStyle, isBald);
    }
}