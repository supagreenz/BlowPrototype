using Game.Jolt;
using Unity.Entities;
using UnityEngine;

public class JoltCapsuleAuthoring : JoltBodyAuthoring
{
    [SerializeField] private float halfHeight = 0.2f;
    [SerializeField] private float radius = 0.2f;

    public override JoltBodyDesc ConstructJoltBodyDesc()
    {
        return JoltBodyDesc.Capsule(halfHeight, radius, Vector3.zero, Quaternion.identity, JoltMotion.Dynamic);
    }

    private class Baker : Baker<JoltCapsuleAuthoring>
    {
        public override void Bake(JoltCapsuleAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            JoltBodyDesc desc = authoring.ConstructJoltBodyDesc();

            AddComponent(entity, new JoltBodyDescData { Desc = desc });
            AddComponent(entity, new JoltBodyRef { Handle = JoltBodyHandle.Invalid });
        }
    }
}
