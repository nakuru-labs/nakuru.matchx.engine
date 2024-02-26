using MatchX.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace MatchX.Engine
{

	[RequireMatchingQueriesForUpdate]
	public partial struct CreateBoardInputSystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var ecb = new EntityCommandBuffer(Allocator.Temp);
			
			foreach (var (sizeRo, gravityRo, requestEntity) in SystemAPI.Query<RefRO<Board.Size>, RefRO<Board.Gravity>>()
			                                          .WithAll<EngineInput.CreateBoard>()
			                                          .WithEntityAccess()) {
				ecb.AddComponent<ReadyToDestroy>(requestEntity);
				
				var boardEntity = ecb.CreateEntity();
				ecb.SetName(boardEntity, "Board");
				ecb.AddComponent<Board.Tag>(boardEntity);
				ecb.AddComponent(boardEntity, sizeRo.ValueRO);
				ecb.AddComponent(boardEntity, gravityRo.ValueRO);
				
				var numSlots = sizeRo.ValueRO.Width * sizeRo.ValueRO.Height;
				ecb.AddBuffer<Board.CellState>(boardEntity)
				   .ResizeUninitialized(numSlots);
				
				var slotsBuffer = ecb.AddBuffer<Board.Slots>(boardEntity);

				for (var i = 0; i < numSlots; i++) {
					var column = i % sizeRo.ValueRO.Width;
					var row = i / sizeRo.ValueRO.Width;
					var slotEntity = ecb.CreateEntity();
					ecb.SetName(slotEntity, $"Slot<{column}, {row}>");
					ecb.AddComponent<Slot.Tag>(slotEntity);
					ecb.AddComponent(slotEntity, new Board.Position { Value = new int2(column, row) });
					ecb.AddBuffer<Slot.Elements>(slotEntity);
					slotsBuffer.Add(new Board.Slots { Value = slotEntity });
				}
				
				var outputEntity = ecb.CreateEntity();
				ecb.SetName(outputEntity, $"Output<{nameof(EngineOutput.BoardCreated)}>");
				ecb.AddComponent<EngineOutput.Tag>(outputEntity);
				ecb.AddComponent<EngineOutput.BoardCreated>(outputEntity);
				ecb.AddComponent(outputEntity, sizeRo.ValueRO);
				ecb.AddComponent(outputEntity, gravityRo.ValueRO);
			}
			
			ecb.Playback(state.EntityManager);
		}
	}

}