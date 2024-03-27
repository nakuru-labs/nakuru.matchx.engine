using MatchX.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace MatchX.Engine
{

	[RequireMatchingQueriesForUpdate]
	public partial struct TriggerElementInputSystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var ecb = new EntityCommandBuffer(Allocator.Temp);
			
			foreach (var (elementIdRo, entity) in SystemAPI.Query<RefRO<Element.Id>>()
			                                       .WithAll<EngineInput.TriggerElement>()
			                                       .WithEntityAccess()) {
				
				ecb.AddComponent<ReadyToDestroy>(entity);
				
				var outputEntity = ecb.CreateEntity();
				ecb.SetName(outputEntity, $"Output<{nameof(EngineOutput.ElementDestroyed)}>");
				ecb.AddComponent<EngineOutput.Tag>(outputEntity);
				ecb.AddComponent<EngineOutput.ElementDestroyed>(outputEntity);
				ecb.AddComponent(outputEntity, elementIdRo.ValueRO);
			}
			
			ecb.Playback(state.EntityManager);
		}
	}

}