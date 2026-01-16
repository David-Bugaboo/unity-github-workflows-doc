using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a set of meteors in the world.
/// </summary>
public class AoIMeteorManager : NetworkBehaviour
{
    [SerializeField, Tooltip("Reference to the meteor prefab.")]
    private NetworkObject meteorPrefab;

    // The number of meteors to create
    public int meteorCount = 64;

    // The number of meteors to create
    public float meteorMinRadius = 64;

    // The number of meteors to create
    public float meteorMaxRadius = 1000;

    [SerializeField, Tooltip("A gradient used to determine the colors of the meteors.")]
    private Gradient meteorColorGradient;

    [SerializeField, Tooltip("The minimum scale of the meteors.")]
    private float meteorScaleMin;

    [SerializeField, Tooltip("The maximum scale of the meteors.")]
    private float meteorScaleMax;

    [SerializeField, Tooltip("The minimum orbit speed of the spawned meteors.")]
    private float minOrbitSpeed;

    [SerializeField, Tooltip("The maximum orbit speed of the spawned meteors.")]
    private float maxOrbitSpeed;

    [SerializeField, Tooltip("The minimum rotation speed of the spawned meteors.")]
    private float minRotationSpeed;

    [SerializeField, Tooltip("The maximum rotation speed of the spawned meteors.")]
    private float maxRotationSpeed;

    public override void Spawned()
    {
        base.Spawned();

        // On spawn, the stat authority creates the specified number of meteors with various randomized attributes.
        if (HasStateAuthority && Runner.CanSpawn)
        {
            for (int i = 0; i < meteorCount; i++)
            {
                Vector3 position = Random.onUnitSphere * Mathf.Lerp(meteorMinRadius, meteorMaxRadius, Random.value);
                Quaternion rotation = Random.rotation;

                NetworkObject newMeteor = Runner.Spawn(meteorPrefab, position, rotation, null);

                var aoiMeteor = newMeteor.GetBehaviour<AoIMeteor>();

                aoiMeteor.MeteorColor = meteorColorGradient.Evaluate(Random.value);

                aoiMeteor.transform.localScale = Vector3.one * Mathf.Lerp(meteorScaleMin, meteorScaleMax, Random.value);

                aoiMeteor.OrbitSpeed = Mathf.Lerp(minOrbitSpeed, maxOrbitSpeed, Random.value);

                aoiMeteor.RotationSpeed = Mathf.Lerp(minRotationSpeed, maxRotationSpeed, Random.value);

                aoiMeteor.MeteorType = Random.Range(0, aoiMeteor.NumberOfMeteorTypes);
            }
        }
    }
}
