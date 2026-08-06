using Game.Jolt;
using Unity.Entities;
using UnityEngine;

public class JoltBoxAuthoring : JoltBodyAuthoring
{
    [SerializeField] private Vector3 halfExtents = Vector3.one * 0.25f;

    public override JoltBodyDesc ConstructJoltBodyDesc()
    {
        return JoltBodyDesc.Box(halfExtents, Vector3.zero, Quaternion.identity, JoltMotion.Dynamic);
    }

    private class Baker : Baker<JoltBoxAuthoring>
    {
        public override void Bake(JoltBoxAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            JoltBodyDesc desc = authoring.ConstructJoltBodyDesc();

            AddComponent(entity, new JoltBodyDescData { Desc = desc });
            AddComponent(entity, new JoltBodyRef { Handle = JoltBodyHandle.Invalid });
        }
    }
}
