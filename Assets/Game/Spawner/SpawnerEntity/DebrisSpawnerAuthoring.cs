using Unity.Entities;
using UnityEngine;

public class DebrisSpawnerAuthoring : MonoBehaviour
{
    public GameObject DebrisPrefab;
    
    private class SpawnerBaker : Baker<DebrisSpawnerAuthoring>
    {
        public override void Bake(DebrisSpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new DebrisSpawner()
            {
                DebrisPrefab = GetEntity(authoring.DebrisPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct DebrisSpawner : IComponentData
{
    public Entity DebrisPrefab;
}