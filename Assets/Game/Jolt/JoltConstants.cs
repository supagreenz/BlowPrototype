using UnityEngine;

namespace Game.Jolt
{
    public static class JoltConstants
    {
        public static readonly int MaxWorldBodies = 4096;

    }

    public struct JoltShapeCollTest
    {
        public JoltShapeData ShapeData;
        public Vector3 Position;
        public Quaternion Rotation;
        
        public JoltShapeCollTest(JoltShapeData shapeData, Vector3 position, Quaternion rotation) => 
            (ShapeData, Position, Rotation) = (shapeData, position, rotation);
    }
    
    public struct JoltShapeData
    {
        public JoltShape Shape;
        public float A, B, C;

        public JoltShapeData Ball(float radius) => new () { A = radius };
        public JoltShapeData Box(float halfX, float halfY, float halfZ) => new () { A = halfX, B = halfY, C = halfZ };
        public JoltShapeData Box(Vector3 halfEx) => new () { A = halfEx.x, B = halfEx.y, C = halfEx.z };
        public JoltShapeData Capsule(float halfH, float radius) => new () { A = halfH, B = radius };
    }

    public enum JoltShape
    {
        Ball = 0,  // Shape.x = radius
        Box = 1,     // Shape = half extents
        Capsule = 2, // Shape.x = half height of the cylinder, Shape.y = radius
    }

    public enum JoltMotion
    {
        Static = 0,
        Kinematic = 1,
        Dynamic = 2,
    }   
}