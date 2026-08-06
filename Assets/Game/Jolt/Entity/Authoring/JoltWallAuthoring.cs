using Game.Jolt;
using Unity.Entities;
using UnityEngine;

public class JoltWallAuthoring : JoltBodyAuthoring
{
    public override JoltBodyDesc ConstructJoltBodyDesc()
    {
        // Walls are placed in a scene rather than spawned, so their own
        // transform sizes them. lossyScale rather than JoltWall's localScale,
        // because baking flattens the hierarchy and a nested wall would
        // otherwise register at the wrong size.
        Vector3 halfExtents = transform.lossyScale * 0.5f;
        return JoltBodyDesc.Box(halfExtents, Vector3.zero, Quaternion.identity, JoltMotion.Static);
    }

    private class Baker : Baker<JoltWallAuthoring>
    {
        public override void Bake(JoltWallAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            JoltBodyDesc desc = authoring.ConstructJoltBodyDesc();

            AddComponent(entity, new JoltBodyDescData { Desc = desc });
            AddComponent(entity, new JoltBodyRef { Handle = JoltBodyHandle.Invalid });
            AddComponent<JoltStaticBody>(entity);
        }
    }
}
