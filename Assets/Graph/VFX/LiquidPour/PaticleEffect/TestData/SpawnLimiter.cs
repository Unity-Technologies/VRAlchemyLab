using UnityEngine;
using UnityEngine.VFX;

public class SpawnLimiter : VFXSpawnerCallbacks
{
	// Input properties exposed to the VFX Graph
	public class InputProperties
	{
		public uint maxSpawnPerFrame = 1;
	}

	// Name of the exposed property in the VFX Graph
	static readonly string maxSpawnPerFrameName = "maxSpawnPerFrame";

	public override void OnPlay(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
	{
		// Called when the spawner starts playing
		// No initialization needed for this behavior
	}

	public override void OnStop(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
	{
		// Called when the spawner stops
		// No cleanup required
	}

	public override void OnUpdate(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
	{
		// Limit the number of particles spawned per frame
		// Ensures the spawnCount never exceeds the user-defined maximum
		uint maxPerFrame = vfxValues.GetUInt(maxSpawnPerFrameName);
		state.spawnCount = Mathf.Min(state.spawnCount, maxPerFrame);
	}
}
