using Unity.Burst;
using Unity.Entities;

namespace MatchX.Engine
{
	
	public partial struct EngineTickSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.EntityManager.AddComponent<Simulation.Tick>(state.SystemHandle);
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			SystemAPI.GetComponentRW<Simulation.Tick>(state.SystemHandle).ValueRW.Value++;
		}
	}

}