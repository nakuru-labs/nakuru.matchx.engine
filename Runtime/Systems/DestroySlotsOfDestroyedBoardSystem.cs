using MatchX.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace MatchX.Engine
{

	[RequireMatchingQueriesForUpdate]
	public partial struct DestroySlotsOfDestroyedBoardSystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var ecb = new EntityCommandBuffer(Allocator.Temp);
			
			foreach (var slotsBuffer in SystemAPI.Query<DynamicBuffer<Board.Slots>>()
			                                     .WithAll<Board.Tag, ReadyToDestroy>()) {
				foreach (var slot in slotsBuffer) {
					if (slot.Value == Entity.Null)
						continue;
					
					ecb.AddComponent<ReadyToDestroy>(slot.Value);
				}
			}
			
			ecb.Playback(state.EntityManager);
		}
	}

}