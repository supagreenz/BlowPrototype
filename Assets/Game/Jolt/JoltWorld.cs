using System;
using System.Runtime.InteropServices;
using AOT;
using Unity.Scripting.LifecycleManagement;

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
            return new JoltBodyHandle(JoltNative.Jolt_AddBody(_handle, ref local));
        }

        /// <summary>
        /// Remove and destroy a single body, freeing its slot for reuse.
        /// Removing an already removed body is a no-op.
        /// </summary>
        /// <returns>True if a body was removed.</returns>
        public bool RemoveBody(JoltBodyHandle body)
        {
            ThrowIfDisposed();
            return JoltNative.Jolt_RemoveBody(_handle, body.Raw);
        }

        /// <summary>
        /// Whether a handle still refers to a live body. False for handles to
        /// removed bodies, even if their slot has since been reused.
        /// </summary>
        public bool IsAlive(JoltBodyHandle body)
        {
            return IsValid && JoltNative.Jolt_IsBodyValid(_handle, body.Raw);
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
            if (!body.IsValid || !IsValid)
            {
                state = default;
                return false;
            }

            int index = body.Index;
            if (index >= _states.Length)
            {
                state = default;
                return false;
            }

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
