
using Game.Jolt;
using UnityEngine;

public class JoltWall : JoltBody
{
    protected override JoltBodyDesc ConstructJoltBodyDesc()
    {
        return new JoltBodyDesc
        {
            ShapePose = GetInitialPose(),
            MotionType = JoltMotion.Static,
            Mass = 1f,
            Friction = 0.05f,
            Restitution = 0.25f,
            GravityFactor = 1f,
            IsSensor = 0u,
        };
    }

    public override void StateUpdate(JoltBodyState newState)
    {
        
    }
    
    private JoltShapePose GetInitialPose()
    {
        return JoltShapePose.Create(JoltShapeData.Box(transform.localScale * 0.5f), transform.position, Quaternion.identity);
    }
}