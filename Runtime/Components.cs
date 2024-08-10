using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Xacce.BlobActor.Collide.Runtime
{
    public partial struct BlobActorShape : IComponentData
    {
        [Serializable]
        public struct Blob
        {
            public float radius;
            public float3 extents;

            [Tooltip("How much entities we try to collide")]
            public int capacity;

            public float collideIntensity;
            public float height;

            public static Blob Default => new Blob()
            {
                capacity = 10,
                extents = new float3(0.5f, 0.5f, 0.5f),
                height = 1.7f,
                radius = 0.25f,
                collideIntensity = 0.7f
            };
        }

        public BlobAssetReference<Blob> blob;

    }
}