using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple meteor object that rotates around the center of the game world
/// </summary>
public class AoIMeteor : NetworkBehaviour, IInterestEnter, IInterestExit
{
    [SerializeField, Tooltip("Reference to the MeshRenderer for the meteor")]
    private MeshRenderer meshRenderer;

    [SerializeField, Tooltip("Reference to the MeshFilter for the meteor")]
    private MeshFilter meshFilter;

    [SerializeField, Tooltip("The meshes that can be assigned to this object.")]
    public Mesh[] meteorMeshes;

    [Networked(), OnChangedRender(nameof(MeteorTypeChanged)), Tooltip("What meteor type does this currently represent.")]
    public int MeteorType { get; set; } = -1;

    [Networked(), OnChangedRender(nameof(MeteorColorChanged)), Tooltip("The color of the meteor")]
    public Color MeteorColor { get; set; }

    [Networked, Tooltip("The rate at which the meteors rotate.")]
    public float RotationSpeed { get; set; }

    [Networked, Tooltip("The speed at which this meteor orbits [0,0,0]")]
    public float OrbitSpeed { get; set; }
    public int NumberOfMeteorTypes => meteorMeshes.Length;

    /// <summary>
    /// The material instantiated for the meteor
    /// </summary>
    private Material instantiatedMaterial;

    public override void Spawned()
    {
        base.Spawned();

        MeteorTypeChanged();
        MeteorColorChanged();
    }

    public void MeteorTypeChanged()
    {
        if (MeteorType < 0)
            meshFilter.sharedMesh = null;
        else
            meshFilter.sharedMesh = meteorMeshes[MeteorType];
    }

    public void MeteorColorChanged()
    {
        if (instantiatedMaterial == null)
            instantiatedMaterial = meshRenderer.material;

        instantiatedMaterial.color = MeteorColor;
    }

    protected void OnDestroy()
    {
        if (instantiatedMaterial != null)
            Destroy(instantiatedMaterial);
    }

    public override void FixedUpdateNetwork()
    {
        float runnerDeltaTimer = Runner.DeltaTime;

        // Orbits the meteor around the center of the world and then rotates it around the world.
        transform.RotateAround(Vector3.zero, Vector3.up, OrbitSpeed * runnerDeltaTimer);
        transform.Rotate(transform.position.normalized, RotationSpeed * runnerDeltaTimer);
    }

    /// <summary>
    /// When the meteor enters the area of interest of the local player, the meteor is activated.
    /// </summary>
    /// <param name="player">The player whose area of interest this object has entered.</param>
    public void InterestEnter(PlayerRef player)
    {
        if (Runner.LocalPlayer != player)
            return;

        gameObject.SetActive(true);
    }

    /// <summary>
    /// When the meteor exits the area of interest of the local player, the meteor is deactivated.
    /// </summary>
    /// <param name="player">The player whose area of interest this object has left.</param>
    public void InterestExit(PlayerRef player)
    {
        if (Runner.LocalPlayer != player)
            return;

        gameObject.SetActive(false);
    }
}