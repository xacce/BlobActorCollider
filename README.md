Collide job and shape config for blobactor, currently works only with UniformSpatialHashing from this package https://github.com/xacce/SpatialHashing.git
COntains single job for handle transform displacements

Put this code to ur simulation system and provide lookups

Job:

```csharp
new ActorInUniformGridCollisionJob()
{
    bridge = new UniformSpatialDatabaseReadonlyBridge()
    {
        entity = SystemAPI.GetSingletonEntity<UniformSpatialDatabase>(),
        uniformSpatialDatabaseRo = _lookups.uniformSpatialDatabaseRo,
        uniformSpatialElementRo = _lookups.uniformSpatialElementRo,
        uniformSpatialCellRo = _lookups.uniformSpatialCellRo,
    },
    blobActorShapeRo = _lookups.blobActorShapeRo,
    flagsRo = _lookups.flagsRo,
}.ScheduleParallel();
```

Lookups:

```csharp
[BurstCompile]
        public struct Lookups
        {
            [ReadOnly] public ComponentLookup<UniformSpatialDatabase> uniformSpatialDatabaseRo;
            [ReadOnly] public BufferLookup<UniformSpatialElement> uniformSpatialElementRo;
            [ReadOnly] public BufferLookup<UniformSpatialCell> uniformSpatialCellRo;
            [ReadOnly]public ComponentLookup<BlobActorShape> blobActorShapeRo;
            [ReadOnly]public ComponentLookup<ActorFlags> flagsRo;

            public Lookups(ref SystemState state) : this()
            {
                uniformSpatialDatabaseRo = state.GetComponentLookup<UniformSpatialDatabase>(false);
                flagsRo = state.GetComponentLookup<ActorFlags>(false);
                blobActorShapeRo = state.GetComponentLookup<BlobActorShape>(false);
                uniformSpatialCellRo = state.GetBufferLookup<UniformSpatialCell>(true);
                uniformSpatialElementRo = state.GetBufferLookup<UniformSpatialElement>(true);
            }

            [BurstCompile]
            public void Update(ref SystemState state)
            {
                uniformSpatialElementRo.Update(ref state);
                uniformSpatialCellRo.Update(ref state);
                uniformSpatialDatabaseRo.Update(ref state);
                blobActorShapeRo.Update(ref state);
                flagsRo.Update(ref state);
            }
        }
```