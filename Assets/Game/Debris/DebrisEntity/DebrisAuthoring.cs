using Unity.Entities;
using UnityEngine;

public class DebrisAuthoring : MonoBehaviour
{
    private class Baker : Baker<DebrisAuthoring>
    {
        public override void Bake(DebrisAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new DebrisData());
        }
    }
}
