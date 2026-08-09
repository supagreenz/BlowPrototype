using Game.Jolt;
using UnityEngine;

public class JoltCapsule : JoltBody
{
    protected override JoltBodyDesc ConstructJoltBodyDesc()
    {
        return new JoltBodyDesc
        {
            ShapePose = GetInitialPose(),
            MotionType = JoltMotion.Dynamic,
            Mass = 1f,
            Friction = 0.05f,
            Restitution = 0.25f,
            GravityFactor = 1f,
            IsSensor = 0u,
        };
    }
    
    private JoltShapePose GetInitialPose()
    {
        return JoltShapePose.Create(JoltShapeData.Capsule(0.4f, 0.4f), transform.position, Quaternion.identity);
    }
}
