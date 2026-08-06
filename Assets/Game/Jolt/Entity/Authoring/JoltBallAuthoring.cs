using Game.Jolt;
using Unity.Entities;
using UnityEngine;

public class JoltBallAuthoring : JoltBodyAuthoring
{
    [SerializeField] private float radius = 0.25f;

    public override JoltBodyDesc ConstructJoltBodyDesc()
    {
        return JoltBodyDesc.Sphere(radius, Vector3.zero, JoltMotion.Dynamic);
    }

    private class Baker : Baker<JoltBallAuthoring>
    {
        public override void Bake(JoltBallAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            JoltBodyDesc desc = authoring.ConstructJoltBodyDesc();

            AddComponent(entity, new JoltBodyDescData { Desc = desc });
            AddComponent(entity, new JoltBodyRef { Handle = JoltBodyHandle.Invalid });
        }
    }
}
