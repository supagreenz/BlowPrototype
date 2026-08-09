
using Game.Jolt;
using UnityEngine;

public class JoltWall : JoltBody
{
    protected override JoltBodyDesc ConstructJoltBodyDesc()
    {
        return new JoltBodyDesc
        (
            GetInitialPose(),
            JoltMotion.Static,
            1f,
            0.3f,
            0.25f,
            1f
        );
    }

    public override void StateUpdate(JoltBodyState newState)
    {
        
    }
    
    private JoltShapePose GetInitialPose()
    {
        return new JoltShapePose(JoltShapeData.Box(transform.localScale * 0.5f), transform.position, Quaternion.identity);
    }
}