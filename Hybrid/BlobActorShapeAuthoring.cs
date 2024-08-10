#if UNITY_EDITOR
using Actor.Collider.Hybrid.Hybrid.So;
using Unity.Entities;
using UnityEngine;
using Xacce.BlobActor.Collide.Runtime;

namespace Xacce.BlobActor.Collide.Hybrid
{
    public class BlobActorShapeAuthoring : MonoBehaviour
    {
        [SerializeField] private ActorShapeBlobBaked shape_s;

        class BlobActorShapeBaker : Baker<BlobActorShapeAuthoring>
        {
            public override void Bake(BlobActorShapeAuthoring authoring)
            {
                var e = GetEntity(authoring);
                AddComponent(
                    e,
                    new BlobActorShape()
                    {
                        blob = authoring.shape_s.Bake(this)
                    });
            }
        }
    }
}
#endif