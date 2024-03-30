using MatchX.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace MatchX.Engine
{

	public partial struct CreateElementInputSystem : ISystem
	{
		private EntityQuery _boardQuery;
		
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			_boardQuery = SystemAPI.QueryBuilder().WithAll<Board.Tag, Board.Size, Board.Slots>().Build();
			state.RequireForUpdate(_boardQuery);
			
			var query = SystemAPI.QueryBuilder().WithAll<EngineInput.CreateElement>().Build();
			state.RequireForUpdate(query);
			state.RequireForUpdate<IdGenerator>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var ecb = new EntityCommandBuffer(Allocator.Temp);
			var boardSize = _boardQuery.GetSingleton<Board.Size>();
			var boardSlots = _boardQuery.GetSingletonBuffer<Board.Slots>(true);
			var idGenerator = SystemAPI.GetSingleton<IdGenerator>();

			foreach (var (positionRo, shapeBuffer, requestEntity) in SystemAPI.Query<RefRO<Board.Position>, DynamicBuffer<Element.Shape>>()
			                                                 .WithAll<EngineInput.CreateElement>()
			                                                 .WithEntityAccess()) {
				ecb.AddComponent<ReadyToDestroy>(requestEntity);

				var position = positionRo.ValueRO.Value;
				var slotIndex = position.y * boardSize.Width + position.x;
				var elementEntity = ecb.CreateEntity();
				var elementId = new Element.Id { Value = idGenerator.Next };
				
				ecb.SetName(elementEntity, $"Element<{elementId.Value}>");
				ecb.AddComponent<Element.Tag>(elementEntity);
				ecb.AddComponent<Element.Is.Dynamic>(elementEntity);
				ecb.AddComponent(elementEntity, elementId);
				ecb.AddComponent(elementEntity, positionRo.ValueRO);
				ecb.AddComponent(elementEntity, positionRo.ValueRO);
				ecb.AddBuffer<Element.Shape>(elementEntity).CopyFrom(shapeBuffer);
				
				// TODO: Move the code to some utility method to use also in Sync element size system
				var indices = shapeBuffer.Reinterpret<uint2>();
				var size = new Element.Size();

				foreach (var index in indices) {
					var indexToSize = index + 1;
					if (indexToSize.x > size.Value.x)
						size.Value.x = indexToSize.x;
					
					if (indexToSize.y > size.Value.y)
						size.Value.y = indexToSize.y;
				}
				//
				
				ecb.AddComponent(elementEntity, size);

				if (boardSlots[slotIndex].Value == Entity.Null) {
					// TODO: if there is no slot at position Generate error output
				}
				
				ecb.AppendToBuffer(boardSlots[slotIndex].Value, new Slot.Elements { Value = elementEntity });
				
				var outputEntity = ecb.CreateEntity();
				ecb.SetName(outputEntity, $"Output<{nameof(EngineOutput.ElementCreated)}>");
				ecb.AddComponent<EngineOutput.Tag>(outputEntity);
				ecb.AddComponent<EngineOutput.ElementCreated>(outputEntity);
				ecb.AddComponent(outputEntity, elementId);
				ecb.AddComponent(outputEntity, size);
				ecb.AddComponent(outputEntity, positionRo.ValueRO);
			}
			
			ecb.Playback(state.EntityManager);
		}
	}

}