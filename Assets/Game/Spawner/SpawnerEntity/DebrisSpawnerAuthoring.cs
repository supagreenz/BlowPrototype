using Unity.Entities;
using UnityEngine;

public class DebrisSpawnerAuthoring : MonoBehaviour
{
    public GameObject DebrisPrefab;

    [Tooltip("Total debris to spawn once, at startup.")]
    public int TotalToSpawn = 2000;

    [Tooltip("How many of that total go out per frame. At 1 the full batch " +
             "takes TotalToSpawn frames to land.")]
    public int PerFrame = 1;

    private class SpawnerBaker : Baker<DebrisSpawnerAuthoring>
    {
        public override void Bake(DebrisSpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new DebrisSpawner()
            {
                DebrisPrefab = GetEntity(authoring.DebrisPrefab, TransformUsageFlags.Dynamic),
                PerFrame = Mathf.Max(1, authoring.PerFrame)
            });
            AddComponent(entity, new DebrisSpawnCounter()
            {
                Remaining = Mathf.Max(0, authoring.TotalToSpawn)
            });
        }
    }
}

public struct DebrisSpawner : IComponentData
{
    public Entity DebrisPrefab;
    public int PerFrame;
}

/// <summary>
/// What is left of the startup batch. Counts down to zero and stays there;
/// the spawner is done for the run once it does.
/// </summary>
public struct DebrisSpawnCounter : IComponentData
{
    public int Remaining;
}
