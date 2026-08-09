using Game.Jolt;

public struct DebrisSpawnedEvent
{
    public JoltBodyDesc joltBodyDesc;
    public JoltBody bodyRef;
}

public struct DebrisDestroyedEvent
{
    
}

public struct BlowFieldRegisterEvent
{
    public BlowField BlowField;
}

public struct BlowFieldUnregisterEvent
{
    public BlowField BlowField;
}