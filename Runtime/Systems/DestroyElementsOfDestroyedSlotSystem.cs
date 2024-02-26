using MatchX.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace MatchX.Engine
{

	[RequireMatchingQueriesForUpdate]
	public partial struct DestroyElementsOfDestroyedSlotSystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var ecb = new EntityCommandBuffer(Allocator.Temp); 
			
			foreach (var elementsBuffer in SystemAPI.Query<DynamicBuffer<Slot.Elements>>()
			                                  .WithAll<Slot.Tag, ReadyToDestroy>()) {
				foreach (var element in elementsBuffer) {
					if (element.Value == Entity.Null)
						continue;
					
					ecb.AddComponent<ReadyToDestroy>(element.Value);
				}
			}
			
			ecb.Playback(state.EntityManager);
		}
	}

}