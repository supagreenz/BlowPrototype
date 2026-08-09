using Game.Jolt;
using UnityEngine;

public class JoltBox : JoltBody
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
        return new JoltShapePose(JoltShapeData.Box(Vector3.one * 0.5f), transform.position, Quaternion.identity);
    }
}
