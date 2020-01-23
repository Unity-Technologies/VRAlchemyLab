using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

public class SpawnLimiter : VFXSpawnerCallbacks
{
    public class InputProperties
    {
        public uint maxSpawnPerFrame = 1;
    }
    static readonly ExposedProperty maxSpawnPerFrameName = "maxSpawnPerFrame";
    public override void OnPlay(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
    {

    }

    public override void OnStop(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
    {

    }

    public override void OnUpdate(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
    {
        state.spawnCount = Mathf.Min(state.spawnCount, vfxValues.GetUInt(maxSpawnPerFrameName));
    }
}
