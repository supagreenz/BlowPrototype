// using System;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class BlowField : MonoBehaviour
// {
//     [SerializeField] private BlowFieldCollider fieldCollider;
//     [SerializeField] private float pushForce = 10;
//     
//     private Transform _thisT;
//     private Vector3 cPos;
//     
//     private void Awake()
//     {
//         _thisT = transform;
//         
//         // EventBus<BlowFieldRegisterEvent>.Raise(new (){BlowField = this});
//     }
//
//     private void OnDestroy()
//     {
//         // EventBus<BlowFieldUnregisterEvent>.Raise(new (){BlowField = this});
//     }
//
//     public void GetColliderBox(out Vector3 center, out Vector3 extents, out Quaternion rot)
//     {
//         fieldCollider.GetColliderBox(out center, out extents, out rot);
//     }
//
//     private void FixedUpdate()
//     {
//         cPos = _thisT.position;
//     }
//
//     public Vector3 CalculatePushFrom(Vector3 bodyPos)
//     {
//         Vector3 dn = bodyPos - cPos;
//         dn = dn.normalized * pushForce;
//         return dn;
//     }
// }