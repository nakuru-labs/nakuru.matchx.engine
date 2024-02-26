using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace MatchX.Common.Systems
{

	[RequireMatchingQueriesForUpdate]
	public partial struct DestroyEntitiesSystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var ecb = new EntityCommandBuffer(Allocator.Temp);
			
			foreach (var  (_, entity) in SystemAPI.Query<RefRO<ReadyToDestroy>>().WithEntityAccess()) {
				ecb.DestroyEntity(entity);
			}
			
			ecb.Playback(state.EntityManager);
		}
	}

}