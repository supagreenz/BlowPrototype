using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Game.Jolt
{
    /// <summary>
    /// Identifies a body within one world. Packs a slot index and a generation
    /// counter, so a handle to a removed body is rejected rather than silently
    /// addressing whatever reused its slot. Only meaningful in the world that
    /// issued it.
    /// </summary>
    public readonly struct JoltBodyHandle : IEquatable<JoltBodyHandle>
    {
        internal const uint InvalidRaw = 0xFFFFFFFFu;

        public static readonly JoltBodyHandle Invalid = new JoltBodyHandle(InvalidRaw);

        internal readonly uint Raw;

        internal JoltBodyHandle(uint raw) => Raw = raw;

        /// <summary>
        /// False if the body was rejected on creation. Says nothing about
        /// whether the body still exists; use JoltWorld.IsAlive for that.
        /// </summary>
        public bool IsValid => Raw != InvalidRaw;

        /// <summary>
        /// This body's slot in the span returned by JoltWorld.ReadStates.
        /// </summary>
        public int Index => (int)(Raw & 0xFFFFu);

        public bool Equals(JoltBodyHandle other) => Raw == other.Raw;
        public override bool Equals(object obj) => obj is JoltBodyHandle other && Equals(other);
        public override int GetHashCode() => (int)Raw;
        public override string ToString() => IsValid ? $"Body({Index}:{Raw >> 16})" : "Body(invalid)";

        public static bool operator ==(JoltBodyHandle a, JoltBodyHandle b) => a.Raw == b.Raw;
        public static bool operator !=(JoltBodyHandle a, JoltBodyHandle b) => a.Raw != b.Raw;
    }

    /// <summary>
    /// Mirrors JU_BodyState. 60 bytes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct JoltBodyState
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 LinearVelocity;
        public readonly Vector3 AngularVelocity;
        public readonly uint Flags;

        /// <summary>
        /// Raw handle of this slot's occupant. Prefer <see cref="Handle"/>.
        /// </summary>
        public readonly uint RawHandle;

        /// <summary>False for a free slot, whose other fields are zeroed.</summary>
        public readonly bool IsValid => (Flags & 1u) != 0u;

        /// <summary>True if the body is awake and simulating.</summary>
        public readonly bool IsActive => (Flags & 2u) != 0u;

        /// <summary>
        /// The body occupying this slot, ready to pass back to JoltWorld.
        /// Invalid for a free slot.
        /// </summary>
        public JoltBodyHandle Handle => new JoltBodyHandle(RawHandle);
    }

    /// <summary>
    /// Receives Jolt trace output. Invoked from Jolt's job threads during
    /// Jolt_Step, so the handler must be thread safe.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void JoltLogCallback(IntPtr message);

    /// <summary>
    /// Raw bindings to JoltUnity.dll. Prefer driving these through JoltWorld
    /// rather than calling them directly.
    /// </summary>
    internal static class JoltNative
    {
        const string Dll = "JoltUnity";

        // Process wide, shared by every world.
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Jolt_SetLogCallback(JoltLogCallback callback);

        /// <returns>Opaque JU_World*, or IntPtr.Zero on failure.</returns>
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Jolt_CreateWorld(uint maxBodies, uint numThreads);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Jolt_DestroyWorld(IntPtr world);

        /// <returns>Raw handle, or JoltBodyHandle.InvalidRaw.</returns>
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint Jolt_AddBody(IntPtr world, ref JoltBodyDesc desc);

        // C++ bool is one byte; without this the default 4 byte Win32 BOOL
        // marshalling reads garbage.
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Jolt_RemoveBody(IntPtr world, uint body);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Jolt_IsBodyValid(IntPtr world, uint body);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Jolt_GetBodyCount(IntPtr world);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Jolt_GetSlotCount(IntPtr world);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Jolt_SetBodyTransform(IntPtr world, uint body, in Vector3 position, in Quaternion rotation,
            [MarshalAs(UnmanagedType.I1)] bool activate);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Jolt_MoveKinematic(IntPtr world, uint body, in Vector3 position, in Quaternion rotation, float deltaTime);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Jolt_SetBodyVelocity(IntPtr world, uint body, in Vector3 linear, in Vector3 angular);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Jolt_AddImpulse(IntPtr world, uint body, in Vector3 linear, in Vector3 angular);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Jolt_AddImpulseAtPoint(IntPtr world, uint body, in Vector3 impulse, in Vector3 point);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Jolt_AddForce(IntPtr world, uint body, in Vector3 force, in Vector3 torque);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Jolt_AddForceAtPoint(IntPtr world, uint body, in Vector3 force, in Vector3 point);

        /// <param name="outBodies">
        /// Pointer to the first element of a pinned uint[], or IntPtr.Zero
        /// with a zero capacity to count without collecting.
        /// </param>
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Jolt_OverlapShape(IntPtr world, in JoltShapePose pose, IntPtr outBodies, int capacity);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Jolt_Step(IntPtr world, float deltaTime, int collisionSteps);

        /// <param name="outStates">
        /// Pointer to the first element of a pinned JoltBodyState[].
        /// </param>
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Jolt_ReadBodies(IntPtr world, IntPtr outStates, int capacity);
    }
}
