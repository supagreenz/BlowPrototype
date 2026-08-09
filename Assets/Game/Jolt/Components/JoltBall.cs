using Game.Jolt;
using UnityEngine;

public class JoltBall : JoltBody
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
        return JoltShapePose.Create(JoltShapeData.Ball(0.5f), transform.position, Quaternion.identity);
    }
}
