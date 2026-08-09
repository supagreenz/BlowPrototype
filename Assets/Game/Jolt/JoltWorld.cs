using System;
using System.Runtime.InteropServices;
using AOT;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

namespace Game.Jolt
{
    /// <summary>
    /// An independent Jolt simulation. Plain C# with no Unity lifecycle of its
    /// own: create it, call <see cref="Step"/> on whatever cadence you choose,
    /// read the results with <see cref="ReadStates"/>, and
    /// <see cref="Dispose"/> it.
    ///
    /// Worlds are self contained. Create as many as you like, in any order,
    /// and destroy them independently; they share no state.
    ///
    /// Not thread safe: drive one world from a single thread. Jolt uses its
    /// own worker threads internally during Step. Create and destroy worlds
    /// from one thread as well.
    /// </summary>
    public sealed class JoltWorld : IDisposable
    {
        /// <summary>
        /// Jolt trace output, for every world. Raised from Jolt's job threads
        /// during Step, so the handler must be thread safe. Hook this to
        /// Debug.Log if you want native diagnostics in the console.
        /// </summary>
        [AutoStaticsCleanup] public static event Action<string> Log;

        // Rooted so the GC cannot collect the delegate while native code still
        // holds the function pointer. Installed once, before any world exists.
        [AutoStaticsCleanup] static readonly JoltLogCallback sLogCallback = OnNativeLog;
        [AutoStaticsCleanup] static bool sLogCallbackInstalled;

        IntPtr _handle;

        // Readback buffer, pinned for the lifetime of the world so reading is
        // a single native call with no allocation or copying.
        readonly JoltBodyState[] _states;
        GCHandle _statesHandle;
        IntPtr _statesPtr;

        // Current occupant of each slot, mirroring the native generation
        // counters. Lets TryGetState reject a stale handle without paying for
        // a native call per body.
        readonly uint[] _slotHandles;

        /// <summary>Live bodies right now.</summary>
        public int BodyCount => IsValid ? JoltNative.Jolt_GetBodyCount(_handle) : 0;

        /// <summary>
        /// Slots in use, live or free. This is the length of the span
        /// <see cref="ReadStates"/> returns.
        /// </summary>
        public int SlotCount => IsValid ? JoltNative.Jolt_GetSlotCount(_handle) : 0;

        /// <summary>Maximum bodies this world was created with.</summary>
        public int Capacity => _states.Length;

        /// <summary>False once <see cref="Dispose"/> has run.</summary>
        public bool IsValid => _handle != IntPtr.Zero;

        /// <param name="maxBodies">
        /// Hard cap. Adding beyond this fails; the buffers are sized to it.
        /// </param>
        /// <param name="workerThreads">0 uses processor count - 1.</param>
        public JoltWorld(int maxBodies = 4096, int workerThreads = 0)
        {
            if (maxBodies <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxBodies));

            if (!sLogCallbackInstalled)
            {
                JoltNative.Jolt_SetLogCallback(sLogCallback);
                sLogCallbackInstalled = true;
            }

            _handle = JoltNative.Jolt_CreateWorld((uint)maxBodies, (uint)Math.Max(0, workerThreads));
            if (_handle == IntPtr.Zero)
                throw new InvalidOperationException("Jolt_CreateWorld failed.");

            _states = new JoltBodyState[maxBodies];
            _statesHandle = GCHandle.Alloc(_states, GCHandleType.Pinned);
            _statesPtr = _statesHandle.AddrOfPinnedObject();

            _slotHandles = new uint[maxBodies];
            for (int i = 0; i < _slotHandles.Length; i++)
                _slotHandles[i] = JoltBodyHandle.InvalidRaw;
        }

        /// <summary>
        /// Add a body to this world. Bodies with identical shape dimensions
        /// share one cached shape, so spawning many identical objects is cheap.
        /// </summary>
        /// <returns>
        /// A handle, or <see cref="JoltBodyHandle.Invalid"/> if the body was
        /// rejected — bad shape dimensions, or the world is full.
        /// </returns>
        public JoltBodyHandle AddBody(in JoltBodyDesc desc)
        {
            ThrowIfDisposed();

            JoltBodyDesc local = desc;
            var body = new JoltBodyHandle(JoltNative.Jolt_AddBody(_handle, ref local));

            if (body.IsValid)
                _slotHandles[body.Index] = body.Raw;

            return body;
        }

        /// <summary>
        /// Remove and destroy a single body, freeing its slot for reuse.
        /// Removing an already removed body is a no-op.
        /// </summary>
        /// <returns>True if a body was removed.</returns>
        public bool RemoveBody(JoltBodyHandle body)
        {
            ThrowIfDisposed();

            if (!JoltNative.Jolt_RemoveBody(_handle, body.Raw))
                return false;

            _slotHandles[body.Index] = JoltBodyHandle.InvalidRaw;
            return true;
        }

        /// <summary>
        /// Whether a handle still refers to a live body. False for handles to
        /// removed bodies, even if their slot has since been reused.
        /// </summary>
        public bool IsAlive(JoltBodyHandle body)
        {
            // Answered from the managed mirror rather than Jolt_IsBodyValid so
            // that checking a few thousand handles costs no native calls. The
            // two are kept in lockstep by AddBody and RemoveBody.
            return IsValid
                && body.IsValid
                && body.Index < _slotHandles.Length
                && _slotHandles[body.Index] == body.Raw;
        }

        /// <summary>
        /// Teleport a body, discontinuously. Use for spawning and respawning;
        /// driving animation with this skips collision detection between the
        /// old and new pose.
        /// </summary>
        /// <param name="activate">
        /// Wake the body. False places it without disturbing a settled pile.
        /// </param>
        /// <returns>False if the handle is stale.</returns>
        public bool SetTransform(JoltBodyHandle body, Vector3 position, Quaternion rotation, bool activate = true)
        {
            ThrowIfDisposed();
            return JoltNative.Jolt_SetBodyTransform(_handle, body.Raw, position, rotation, activate);
        }

        /// <summary>
        /// Drive a kinematic body toward a target over deltaTime. Jolt derives
        /// the velocity needed to arrive, so the body sweeps and pushes things
        /// properly instead of tunnelling. This is what to call when following
        /// a Transform. No effect on non-kinematic bodies.
        /// </summary>
        /// <returns>False if the handle is stale.</returns>
        public bool MoveKinematic(JoltBodyHandle body, Vector3 position, Quaternion rotation, float deltaTime)
        {
            ThrowIfDisposed();
            return JoltNative.Jolt_MoveKinematic(_handle, body.Raw, position, rotation, deltaTime);
        }

        /// <summary>
        /// Set velocities directly, e.g. to launch freshly spawned debris.
        /// Ignored for static bodies. Wakes the body if either velocity is
        /// non-zero.
        /// </summary>
        /// <returns>False if the handle is stale.</returns>
        public bool SetVelocity(JoltBodyHandle body, Vector3 linear, Vector3 angular = default)
        {
            ThrowIfDisposed();
            return JoltNative.Jolt_SetBodyVelocity(_handle, body.Raw, linear, angular);
        }

        /// <summary>
        /// Apply linear and angular impulses about the center of mass. Dynamic
        /// bodies only; wakes the body.
        /// </summary>
        /// <returns>False if the handle is stale.</returns>
        public bool AddImpulse(JoltBodyHandle body, Vector3 linear, Vector3 angular = default)
        {
            ThrowIfDisposed();
            return JoltNative.Jolt_AddImpulse(_handle, body.Raw, linear, angular);
        }

        /// <summary>
        /// Apply a linear impulse at a world space point, which also imparts
        /// spin. Dynamic bodies only; wakes the body.
        /// </summary>
        /// <returns>False if the handle is stale.</returns>
        public bool AddImpulseAtPoint(JoltBodyHandle body, Vector3 impulse, Vector3 point)
        {
            ThrowIfDisposed();
            return JoltNative.Jolt_AddImpulseAtPoint(_handle, body.Raw, impulse, point);
        }

        /// <summary>
        /// Accumulate a force and torque for the next step. Unlike an impulse,
        /// this is continuous: Jolt scales it by the timestep, and it is
        /// cleared after every <see cref="Step"/>. Reapply each tick for a
        /// sustained push. Dynamic bodies only; wakes the body.
        /// </summary>
        /// <returns>False if the handle is stale.</returns>
        public bool AddForce(JoltBodyHandle body, Vector3 force, Vector3 torque = default)
        {
            ThrowIfDisposed();
            return JoltNative.Jolt_AddForce(_handle, body.Raw, force, torque);
        }

        /// <summary>
        /// Accumulate a force at a world space point, which also produces
        /// torque. Cleared after every <see cref="Step"/>. Dynamic bodies
        /// only; wakes the body.
        /// </summary>
        /// <returns>False if the handle is stale.</returns>
        public bool AddForceAtPoint(JoltBodyHandle body, Vector3 force, Vector3 point)
        {
            ThrowIfDisposed();
            return JoltNative.Jolt_AddForceAtPoint(_handle, body.Raw, force, point);
        }

        /// <summary>
        /// Every body whose shape overlaps the given volume. This is an exact
        /// narrow phase test against the real shapes, not their bounding boxes.
        ///
        /// Results are sorted by slot, so both the set and its order are
        /// reproducible and safe to use in deterministic logic.
        ///
        /// Query shapes share the cache with body shapes, so querying the same
        /// volume every frame allocates nothing after the first call.
        /// </summary>
        /// <param name="dims">
        /// Dimensions, interpreted per <see cref="JoltShape"/> exactly as in
        /// <see cref="JoltBodyDesc.Shape"/>.
        /// </param>
        /// <param name="results">
        /// Receives the overlapping handles. Reused across calls; size it to
        /// the most you expect, since extras beyond its length are dropped.
        /// </param>
        /// <param name="rotation">Identity gives an axis aligned volume.</param>
        /// <returns>
        /// Number of handles written. Equal to results.Length if the buffer
        /// filled, in which case there may have been more.
        /// </returns>
        public int OverlapShape(JoltShape shape, Vector3 dims, Vector3 center, JoltBodyHandle[] results, Quaternion rotation = default)
        {
            ThrowIfDisposed();

            if (results == null || results.Length == 0)
                return 0;

            if (rotation.x == 0f && rotation.y == 0f && rotation.z == 0f && rotation.w == 0f)
                rotation = Quaternion.identity;

            // JoltBodyHandle wraps a single uint, so the array is blittable and
            // the native side can write handles straight into it.
            GCHandle pin = GCHandle.Alloc(results, GCHandleType.Pinned);
            try
            {
                return JoltNative.Jolt_OverlapShape(_handle, (int)shape, dims, center, rotation,
                    pin.AddrOfPinnedObject(), results.Length);
            }
            finally
            {
                pin.Free();
            }
        }

        /// <summary>
        /// Every body overlapping a box. Convenience wrapper over
        /// <see cref="OverlapShape"/>; identical behaviour and cost.
        /// </summary>
        /// <param name="halfExtent">Half extents. All three must be positive.</param>
        /// <param name="rotation">Identity gives an axis aligned box.</param>
        public int OverlapBox(Vector3 center, Vector3 halfExtent, JoltBodyHandle[] results, Quaternion rotation = default)
        {
            return OverlapShape(JoltShape.Box, halfExtent, center, results, rotation);
        }

        /// <summary>
        /// Every body overlapping a capsule. Convenience wrapper over
        /// <see cref="OverlapShape"/>; identical behaviour and cost.
        ///
        /// The capsule runs along its local Y axis, so an unrotated one stands
        /// upright. Its total length is 2 * (halfHeight + radius), since the
        /// hemispherical caps extend past the cylinder on each end.
        /// </summary>
        /// <param name="halfHeight">Half length of the cylindrical section.</param>
        public int OverlapCapsule(Vector3 center, float halfHeight, float radius, JoltBodyHandle[] results, Quaternion rotation = default)
        {
            return OverlapShape(JoltShape.Capsule, new Vector3(halfHeight, radius, 0f), center, results, rotation);
        }

        /// <summary>
        /// Every body overlapping a capsule spanning two points, with the caps
        /// bulging radius past each end. This is the form to use for a swept
        /// volume such as a blower cone or a melee arc, since it works out the
        /// centre and the Y axis alignment for you.
        /// </summary>
        public int OverlapCapsuleBetween(Vector3 start, Vector3 end, float radius, JoltBodyHandle[] results)
        {
            Vector3 axis = end - start;
            float length = axis.magnitude;

            // Degenerate to a sphere rather than handing Jolt a zero length
            // capsule, which it rejects.
            if (length < 1e-5f)
                return OverlapShape(JoltShape.Ball, new Vector3(radius, 0f, 0f), start, results);

            return OverlapCapsule(
                (start + end) * 0.5f,
                length * 0.5f,
                radius,
                results,
                Quaternion.FromToRotation(Vector3.up, axis / length));
        }

        /// <summary>
        /// Advance the simulation. Use a fixed deltaTime: a variable timestep
        /// gives up determinism.
        /// </summary>
        /// <param name="collisionSteps">Substeps per update; 1 is normal.</param>
        public void Step(float deltaTime, int collisionSteps = 1)
        {
            ThrowIfDisposed();
            JoltNative.Jolt_Step(_handle, deltaTime, collisionSteps);
        }

        /// <summary>
        /// Refresh and return the state of every slot, indexed by
        /// <see cref="JoltBodyHandle.Index"/>. Slots whose body was removed
        /// come back zeroed with <see cref="JoltBodyState.IsValid"/> false, so
        /// the span stays index addressable as bodies come and go.
        ///
        /// The span views an internal buffer that the next call overwrites, so
        /// consume it before stepping again rather than storing it.
        /// </summary>
        public ReadOnlySpan<JoltBodyState> ReadStates()
        {
            ThrowIfDisposed();

            int count = JoltNative.Jolt_ReadBodies(_handle, _statesPtr, _states.Length);
            return new ReadOnlySpan<JoltBodyState>(_states, 0, count);
        }

        /// <summary>
        /// State of a single body from the most recent <see cref="ReadStates"/>.
        /// Does not re-read the native side, so call ReadStates first.
        /// </summary>
        /// <returns>False if the handle is invalid or its body is gone.</returns>
        public bool TryGetState(JoltBodyHandle body, out JoltBodyState state)
        {
            state = default;

            if (!body.IsValid || !IsValid)
                return false;

            int index = body.Index;
            if (index >= _states.Length)
                return false;

            // Compare the whole handle, generation included. Checking only
            // that the slot is occupied would hand back the wrong body once a
            // slot has been recycled.
            if (_slotHandles[index] != body.Raw)
                return false;

            state = _states[index];
            return state.IsValid;
        }

        /// <summary>
        /// Destroy the native world. Idempotent. Every world you create must
        /// be disposed or its native memory leaks.
        /// </summary>
        public void Dispose()
        {
            if (_handle == IntPtr.Zero)
                return;

            JoltNative.Jolt_DestroyWorld(_handle);
            _handle = IntPtr.Zero;

            if (_statesHandle.IsAllocated)
                _statesHandle.Free();

            _statesPtr = IntPtr.Zero;
        }

        void ThrowIfDisposed()
        {
            if (_handle == IntPtr.Zero)
                throw new ObjectDisposedException(nameof(JoltWorld));
        }

        [MonoPInvokeCallback(typeof(JoltLogCallback))]
        static void OnNativeLog(IntPtr message)
        {
            Log?.Invoke(Marshal.PtrToStringAnsi(message));
        }
    }
}
