using System;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit
{
    public static class GizmoHelpers
    { 
        public static void DrawWirePlaneOriented(Vector3 position, Quaternion rotation, float size)
        {
            float halfSize = size / 2.0f;
            Vector3 tl = new Vector3(halfSize, 0.0f, -halfSize);
            Vector3 tr = new Vector3(halfSize, 0.0f, halfSize);
            Vector3 bl = new Vector3(-halfSize, 0.0f, -halfSize);
            Vector3 br = new Vector3(-halfSize, 0.0f, halfSize);

            Gizmos.DrawLine((rotation * tl) + position,
                (rotation * tr) + position);

            Gizmos.DrawLine((rotation * tr) + position,
                (rotation * br) + position);

            Gizmos.DrawLine((rotation * br) + position,
             (rotation * bl) + position);

            Gizmos.DrawLine((rotation * bl) + position,
             (rotation * tl) + position);
        }

        public static void DrawWireCubeOriented(Vector3 position, Quaternion rotation, float size)
        {

            float halfSize = size / 2.0f;
            Vector3 tl = new Vector3(halfSize, 0.0f, -halfSize);
            Vector3 tr = new Vector3(halfSize, 0.0f, halfSize);
            Vector3 bl = new Vector3(-halfSize, 0.0f, -halfSize);
            Vector3 br = new Vector3(-halfSize, 0.0f, halfSize);

            Vector3 tlt = new Vector3(halfSize, size, -halfSize);
            Vector3 trt = new Vector3(halfSize, size, halfSize);
            Vector3 blt = new Vector3(-halfSize, size, -halfSize);
            Vector3 brt = new Vector3(-halfSize, size, halfSize);


            Gizmos.DrawLine((rotation * tl) + position,
                (rotation * tr) + position);

            Gizmos.DrawLine((rotation * tr) + position,
                (rotation * br) + position);

            Gizmos.DrawLine((rotation * br) + position,
             (rotation * bl) + position);

            Gizmos.DrawLine((rotation * bl) + position,
             (rotation * tl) + position);

            Gizmos.DrawLine((rotation * tlt) + position,
              (rotation * trt) + position);

            Gizmos.DrawLine((rotation * trt) + position,
                (rotation * brt) + position);

            Gizmos.DrawLine((rotation * brt) + position,
             (rotation * blt) + position);

            Gizmos.DrawLine((rotation * blt) + position,
             (rotation * tlt) + position);

            Gizmos.DrawLine((rotation * tlt) + position,
            (rotation * tl) + position);

            Gizmos.DrawLine((rotation * trt) + position,
                (rotation * tr) + position);

            Gizmos.DrawLine((rotation * brt) + position,
             (rotation * br) + position);

            Gizmos.DrawLine((rotation * blt) + position,
             (rotation * bl) + position);
        }


        public static void DrawAxisArrows(Transform transform, float size)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * size);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.up * size);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.right * size);
        }
    }
}