using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The NetworkObject controlled by players.
/// </summary>
public class AoIPlayer : NetworkBehaviour, IInterestEnter, IInterestExit
{
    [Networked(), OnChangedRender(nameof(OnSetColor)), Tooltip("The spaceship color associated with this player.")]
    public Color color { get; set; }

    [SerializeField, Tooltip("The speeds the player moves on each axis; X is rotation; Y is vertical; Z is forward / back.")]
    private Vector3 speed;

    [SerializeField, Tooltip("Reference to the Mesh Renderer that is changed colors.")]
    private MeshRenderer meshRenderer;

    [SerializeField, Tooltip("Reference to the spaceship's transform.")]
    private Transform spaceShipTransform;

    [Tooltip("Reference to the transform that display the player's area of interest radius.")]
    public Transform aoiRadiusSphere;

    [SerializeField, Tooltip("Reference to this player's camera, which will only be active if this player is the local player and has InputAuthority.")]
    public GameObject cameraGameObject;

    [SerializeField()]
    private float secondaryRotationRate = 10;

    [SerializeField()]
    private float secondaryRotationAngle = 20;

    [SerializeField(), Tooltip("Reference to the settings of this sample.")]
    private AoIMPSettings settings;

    /// <summary>
    /// The previous area of interest value.
    /// </summary>
    int previousAoIRadius = -1;

    /// <summary>
    /// The previosu area of interest cell value.
    /// </summary>
    int previousAoICell = -1;

    /// <summary>
    /// The previous area of interest grid value.
    /// </summary>
    Vector3Int previousAoIGrid = new Vector3Int(-1, -1, -1);

    public override void Spawned()
    {
        base.Spawned();

        cameraGameObject.gameObject.SetActive(Object.InputAuthority == Runner.LocalPlayer);

        if (HasStateAuthority)
            color = Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f, 1f, 1f);

        OnSetColor();
    }

    public void OnSetColor()
    {
        meshRenderer.material.color = color;
    }

    public override void FixedUpdateNetwork()
    {
        Vector3 endEuler = Vector3.zero;

        if (GetInput(out ArcadePlayerInput input))
        {
            if (input.buttons.IsSet(ArcadePlayerInput.LEFT))
            {
                transform.Rotate(Vector3.up * -speed.x * Runner.DeltaTime);
                endEuler.z = secondaryRotationAngle;
            }
            else if (input.buttons.IsSet(ArcadePlayerInput.RIGHT))
            {
                transform.Rotate(Vector3.up * speed.x * Runner.DeltaTime);
                endEuler.z = -secondaryRotationAngle;
            }

            if (input.buttons.IsSet(ArcadePlayerInput.FORWARD))
            {
                transform.position += transform.forward * speed.z * Runner.DeltaTime;
                endEuler.x = secondaryRotationAngle;
            }
            else if (input.buttons.IsSet(ArcadePlayerInput.BACK))
            {
                transform.position -= transform.forward * speed.z * Runner.DeltaTime;
                endEuler.x = -secondaryRotationAngle;
            }

            if (input.buttons.IsSet(ArcadePlayerInput.UP))
            {
                transform.position += Vector3.up * speed.y * Runner.DeltaTime;
                endEuler.x = -secondaryRotationAngle;
            }
            else if (input.buttons.IsSet(ArcadePlayerInput.DOWN))
            {
                transform.position -= Vector3.up * speed.y * Runner.DeltaTime;
                endEuler.x = secondaryRotationAngle;
            }
        }

        Quaternion endRotation = Quaternion.Euler(endEuler);
        spaceShipTransform.localRotation = Quaternion.Slerp(spaceShipTransform.localRotation, endRotation, Runner.DeltaTime * secondaryRotationRate);

        if ((Runner.IsServer || Object.HasStateAuthority) && !Object.InputAuthority.IsNone)
        {
            // The player interest must be cleared when no in share mode.
            if (Runner.GameMode != GameMode.Shared)
                Runner.ClearPlayerAreaOfInterest(Object.InputAuthority);
            Runner.AddPlayerAreaOfInterest(Object.InputAuthority, transform.position, settings.AreaOfInterestRadius);
        }
    }

    public void Update()
    {
        if (aoiRadiusSphere.gameObject.activeSelf != settings.ShowPlayerRadius)
            aoiRadiusSphere.gameObject.SetActive(settings.ShowPlayerRadius);

        if (previousAoIRadius != settings.AreaOfInterestRadius)
        {
            previousAoIRadius = settings.AreaOfInterestRadius;
            aoiRadiusSphere.localScale = Vector3.one * previousAoIRadius * 2f;
        }

        // As of now, these items do not work for Shared Mode, so these values are not checked.
        if (Runner.GameMode == GameMode.Shared)
            return;

        // Only the server can set these items.
        if (Runner.IsServer && Object.InputAuthority == Runner.LocalPlayer)
        {
            if (previousAoICell != settings.AreaOfInterestCellSize)
            {
                Runner.SetAreaOfInterestCellSize(settings.AreaOfInterestCellSize);
                previousAoICell = settings.AreaOfInterestCellSize;
            }

            if (previousAoIGrid != settings.AreaOfInterestGrid)
            {
                Runner.SetAreaOfInterestGrid(settings.AreaOfInterestGrid.x, settings.AreaOfInterestGrid.y, settings.AreaOfInterestGrid.z);
                previousAoIGrid = settings.AreaOfInterestGrid;
            }
        }
    }

    /// <summary>
    /// If this player enters AoI Interest, this GameObject wll be enabled.
    /// </summary>
    /// <param name="player"></param>
    public void InterestEnter(PlayerRef player)
    {
        if (Runner.LocalPlayer != player || !Runner.GetVisible())
            return;

        gameObject.SetActive(true);
    }

    /// <summary>
    /// If the player exits AoI interest, this GameObject will be disabled.
    /// </summary>
    /// <param name="player"></param>
    public void InterestExit(PlayerRef player)
    {
        if (Runner.LocalPlayer != player || !Runner.GetVisible())
            return;

        gameObject.SetActive(false);
    }
}

/// <summary>
/// INetworkStruct for Input.
/// </summary>
public struct ArcadePlayerInput : INetworkInput
{
    public const int LEFT = 0;
    public const int RIGHT = 1;
    public const int UP = 2;
    public const int DOWN = 3;
    public const int FORWARD = 5;
    public const int BACK = 6;

    public NetworkButtons buttons;
}
