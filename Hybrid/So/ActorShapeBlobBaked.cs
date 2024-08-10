#if UNITY_EDITOR
using Core.Hybrid;
using Unity.Entities;
using UnityEngine;
using Xacce.BlobActor.Collide.Runtime;

namespace Actor.Collider.Hybrid.Hybrid.So
{
    [CreateAssetMenu(menuName = "BlobActor/Create shape")]
    public class ActorShapeBlobBaked : BakedScriptableObject<BlobActorShape.Blob>
    {
        [SerializeField] private BlobActorShape.Blob blob_s = BlobActorShape.Blob.Default;
        public override void Bake(ref BlobActorShape.Blob data, ref BlobBuilder blobBuilder)
        {
            data = blob_s;
        }
    }
}
#endif