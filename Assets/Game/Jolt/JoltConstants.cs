using System.Runtime.InteropServices;
using UnityEngine;

namespace Game.Jolt
{
    public static class JoltConstants
    {
        public static readonly int MaxWorldBodies = 4096;

    }

    
    /// <summary>
    /// Mirrors JU_BodyDesc. 68 bytes; Vector3 and Quaternion are already laid
    /// out as 3 and 4 consecutive floats, so the struct is blittable as is.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct JoltBodyDesc
    {
        public JoltShapePose ShapePose;
        public JoltMotion MotionType;
        public float Mass; // <= 0 computes mass from the shape
        public float Friction;
        public float Restitution;
        public float GravityFactor;
        public uint IsSensor;
    }
    
    public struct JoltShapePose
    {
        public JoltShapeData ShapeData;
        public Vector3 Position;
        public Quaternion Rotation;
        
        public static JoltShapePose Create(JoltShapeData shapeData, Vector3 position, Quaternion rotation) => 
            new () { ShapeData = shapeData, Position = position, Rotation = rotation };
    }
    
    public struct JoltShapeData
    {
        public JoltShape Shape;
        public float A, B, C;

        public static JoltShapeData Ball(float radius) => new () { Shape = JoltShape.Ball, A = radius };
        public static JoltShapeData Box(float halfX, float halfY, float halfZ) => new () { Shape = JoltShape.Box, A = halfX, B = halfY, C = halfZ };
        public static JoltShapeData Box(Vector3 halfEx) => new () { Shape = JoltShape.Box, A = halfEx.x, B = halfEx.y, C = halfEx.z };
        public static JoltShapeData Capsule(float halfH, float radius) => new () { Shape = JoltShape.Capsule, A = halfH, B = radius };
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