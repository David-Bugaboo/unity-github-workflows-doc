using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AoIMPSettings : ScriptableObject
{
    [Tooltip("The current Game Mode to use.  Can only set Shared or Client/Host.")]
    public GameMode currentGameMode = GameMode.Shared;

    [Tooltip("The number of players that will be spawned.")]
    public int PlayersToSpawn = 1;

    [Tooltip("The area of interest radius used for players.")]
    public int AreaOfInterestRadius = 32;

    [Tooltip("The cell sized used by the server to calculate Area of Interest.")]
    public int AreaOfInterestCellSize = 32;

    [Tooltip("The overall grid used for Area of Interest.")]
    public Vector3Int AreaOfInterestGrid = new Vector3Int(1024, 1024, 1024);

    [Tooltip("If true, the player radius will be displayed.")]
    public bool ShowPlayerRadius = true;

    [Tooltip("If true, the large Area of Interest Grid will be displayed.")]
    public bool ShowAreaOfInterestGrid = true;

    [Tooltip("If true, the active server zones or AoI Cells will be displayed.")]
    public bool ShowActiveServerZones = true;

    [Tooltip("If true, the active server zones will be highlighted if a player is in them.")]
    public bool ShowPlayerInterest = true;

    private void OnValidate()
    {
        if (currentGameMode != GameMode.Shared && currentGameMode != GameMode.AutoHostOrClient)
            currentGameMode = GameMode.Shared;

        if (currentGameMode == GameMode.Shared)
        {
            ShowActiveServerZones = false;
            ShowPlayerInterest = false;

            AreaOfInterestRadius = Mathf.Min(300, AreaOfInterestRadius);

            AreaOfInterestCellSize = 32;
            AreaOfInterestGrid = new Vector3Int(1024, 1024, 1024);
        }
    }
}
