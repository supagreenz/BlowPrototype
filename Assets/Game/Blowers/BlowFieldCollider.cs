// using UnityEngine;
//
// [RequireComponent(typeof(BoxCollider))]
// public class BlowFieldCollider : MonoBehaviour
// {
//     private BoxCollider box;
//
//     private void Awake()
//     {
//         box = GetComponent<BoxCollider>();
//     }
//     
//     //
//     // public int ColliderTest(Collider[] results)
//     // {
//     //     GetBoxWorld(out var center, out var extents, out var rot);
//     //     return Physics.OverlapBoxNonAlloc(center, extents, results, rot, mask, triggerMode);
//     // }
//
//     public void GetColliderBox(out Vector3 center, out Vector3 extents, out Quaternion rot)
//     {
//         center  = transform.TransformPoint(box.center);
//         extents = Vector3.Scale(box.size, transform.lossyScale) * 0.5f;
//         rot     = transform.rotation;
//     }
// }