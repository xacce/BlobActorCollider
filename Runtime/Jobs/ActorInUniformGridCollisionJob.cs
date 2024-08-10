using Core.Runtime;
using SpatialHashing.Uniform;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Xacce.BlobActor.Runtime;
using Random = Unity.Mathematics.Random;

namespace Xacce.BlobActor.Collide.Runtime
{
    struct Visitor : IUniformSpatialQueryCollector
    {
        public int added;
        public Random rng;
        public Entity im;
        public float3 myPosition;
        public BlobActorShape.Blob myShape;
        public DynamicObjectVelocity velocity;
        public ActorFlags flags;
        public ComponentLookup<ActorFlags> flagsRo;
        public ComponentLookup<BlobActorShape> shapeLookup;
        public float3 displace;
        public int hits;

        public void OnVisit(in UniformSpatialCell cell, in UnsafeList<UniformSpatialElement> elements, out bool shouldEarlyExit)
        {
            var end = cell.start + cell.length;
            for (int i = cell.start; i < end; i++)
            {
                if (added >= myShape.capacity)
                {
                    shouldEarlyExit = true;
                    return;
                }

                var e = elements[i];
                if ((e.type & XaObjectType.Character) == 0 || !shapeLookup.TryGetComponent(e.entity, out var otherShape) ||
                    !flagsRo.TryGetComponent(e.entity, out var otherFlags))
                {
                    continue;
                }

                added++;
                if ((otherFlags.flags & ActorFlags.Flag.DynamicCollide) == 0)
                    continue;
                ref var otherBlob = ref otherShape.blob.Value;
                float extent = myShape.extents.y * 0.5f;
                float otherExtent = otherBlob.extents.y * 0.5f;
                if (math.abs((myPosition.y + extent) - (e.position.y + otherExtent)) > extent + otherExtent) continue;

                float2 directionInv = myPosition.xz - e.position.xz;
                float distancesq = math.lengthsq(directionInv);
                float radius = myShape.radius + otherBlob.radius;
                if (distancesq > radius * radius || im.Equals(e.entity)) continue;

                float distance = math.sqrt(distancesq);
                float penetration = radius - distance;

                if (distance < 0.0001f)
                {
                    // Avoid both having same displacement
                    if (e.entity.Index > im.Index)
                    {
                        directionInv = -velocity.velocity.xz;
                    }
                    else
                    {
                        directionInv = velocity.velocity.xz;
                    }

                    if (math.length(directionInv) < 0.0001f)
                    {
                        float2 avoidDirection = rng.NextFloat2Direction();
                        directionInv = avoidDirection;
                    }

                    penetration = 0.01f;
                }
                else
                {
                    penetration = (penetration / distance) * myShape.collideIntensity;
                }

                displace += new float3(directionInv.x, 0, directionInv.y) * penetration;
                hits++;
            }

            shouldEarlyExit = false;
        }
    }


    [BurstCompile]
    [WithAll(typeof(Simulate))]
    [WithAll(typeof(BlobActorShape))]
    public partial struct ActorInUniformGridCollisionJob : IJobEntity, IJobEntityChunkBeginEnd
    {
        public UniformSpatialDatabaseReadonlyBridge bridge;

        [ReadOnly] public ComponentLookup<ActorFlags> flagsRo;
        [ReadOnly] public ComponentLookup<BlobActorShape> blobActorShapeRo;
        private Random _rng;


        [BurstCompile]
        private void Execute(ref LocalTransform transform, DynamicObjectVelocity velocity, Entity entity)
        {
            var myFlags = flagsRo[entity];
            if ((myFlags.flags & ActorFlags.Flag.DynamicCollide) == 0) return;
            var myShape = blobActorShapeRo[entity];
            var myBlob = myShape.blob.Value;
            var visitor = new Visitor()
            {
                myPosition = transform.Position,
                myShape = myBlob,
                shapeLookup = blobActorShapeRo,
                im = entity,
                rng = _rng,
                velocity = velocity,
                flagsRo = flagsRo,
            };
            UniformSpatialDatabase.QueryAABBCellProximityOrder(bridge.database, bridge.cellsUnsafe, bridge.elementsUnsafe, transform.Position, myBlob.extents, ref visitor);
            if (visitor.hits == 0) return;
            transform.Position += visitor.displace / visitor.hits;
        }

        public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            _rng = Random.CreateFromIndex(Utility.GetUniqueUIntFromInt(unfilteredChunkIndex));
            bridge.CreateBridge();
            return true;
        }

        public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted)
        {
        }
    }
}