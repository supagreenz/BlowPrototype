using System;
using Game.Jolt;
using UnityEngine;

public class GrabBlower : Blower
{
    
    
    
    public override JoltShapePose GetCurrentShapeTest()
    {
        throw new NotImplementedException();
    }

    public override Vector3 CalculatePushAt(Vector3 at)
    {
        throw new NotImplementedException();
    }

    public override void CalculatePushesAt(int collisions, JoltBodyHandle[] bodies, Vector3[] forcesBuffer, ReadOnlySpan<JoltBodyState> statesBuffer)
    {
        throw new NotImplementedException();
    }
}
