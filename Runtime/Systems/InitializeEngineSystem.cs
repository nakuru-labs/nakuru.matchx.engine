using Unity.Burst;
using Unity.Entities;

namespace MatchX.Engine
{

	public partial struct InitializeEngineSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			var idGeneratorEntity = state.EntityManager.CreateEntity();
			state.EntityManager.SetName(idGeneratorEntity, "Id Generator");
			state.EntityManager.AddComponent<IdGenerator>(idGeneratorEntity);

			state.Enabled = false;
		}
	}

}