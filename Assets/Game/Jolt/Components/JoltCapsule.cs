using Game.Jolt;
using UnityEngine;

public class JoltCapsule : JoltBody
{
    protected override JoltBodyDesc ConstructJoltBodyDesc()
    {
        return new JoltBodyDesc
        (
            GetInitialPose(),
            JoltMotion.Dynamic,
            1f,
            0.3f,
            0.25f,
            1f
        );
    }
    
    private JoltShapePose GetInitialPose()
    {
        return new JoltShapePose(JoltShapeData.Capsule(0.4f, 0.4f), transform.position, Quaternion.identity);
    }
}
