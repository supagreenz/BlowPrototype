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
    public readonly struct JoltBodyDesc
    {
        public readonly JoltShapePose ShapePose;
        public readonly JoltMotion MotionType;
        public readonly float Mass; // <= 0 computes mass from the shape
        public readonly float Friction;
        public readonly float Restitution;
        public readonly float GravityFactor;
        public readonly uint IsSensor;
        
        public JoltBodyDesc(
            JoltShapePose shapePose,
            JoltMotion motionType,
            float mass = 0f,
            float friction = 0.2f,
            float restitution = 0f,
            float gravityFactor = 1f,
            bool isSensor = false)
        {
            ShapePose = shapePose;
            MotionType = motionType;
            Mass = mass;
            Friction = friction;
            Restitution = restitution;
            GravityFactor = gravityFactor;
            IsSensor = isSensor ? 1u : 0u;
        }
    }
    
    public readonly struct JoltShapePose
    {
        public readonly JoltShapeData ShapeData;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public JoltShapePose(JoltShapeData shapeData, Vector3 position, Quaternion rotation)
        {
            ShapeData = shapeData;
            Position = position;
            Rotation = rotation is { x: 0f, y: 0f, z: 0f, w: 0f } ? Quaternion.identity : rotation;
        }
    }
    
    public struct JoltShapeData
    {
        public readonly JoltShape Shape;
        public readonly float A, B, C;

        public static JoltShapeData Ball(float radius) => new(JoltShape.Ball, radius);
        public static JoltShapeData Box(float halfX, float halfY, float halfZ) => new(JoltShape.Box, halfX, halfY, halfZ);
        public static JoltShapeData Box(Vector3 halfEx) => new (JoltShape.Box, halfEx.x, halfEx.y, halfEx.z);
        public static JoltShapeData Capsule(float halfH, float radius) => new (JoltShape.Capsule, halfH, radius);

        public JoltShapeData(JoltShape shape, float a = 0, float b = 0, float c = 0)
        {
            Shape = shape; A = a; B = b; C = c;
        }
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