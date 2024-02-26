using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace MatchX.Engine
{
	
	public partial struct GravitySystem : ISystem
	{
		private EntityQuery _boardQuery;
		
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			_boardQuery = SystemAPI.QueryBuilder().WithAll<Board.Tag, Board.Size, Board.CellState, Board.Gravity>().Build();
			state.RequireForUpdate(_boardQuery);

			var elementQuery = SystemAPI.QueryBuilder().WithAll<Element.Tag, Board.Position>().Build();
			state.RequireForUpdate(elementQuery);
			
			state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
		}

		/// <summary>
		/// !!! Gravity algorithm now supports only vertical gravity down.
		/// </summary>
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
			var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
			
			var boardSize = _boardQuery.GetSingleton<Board.Size>();
			var gravity = _boardQuery.GetSingleton<Board.Gravity>();
			var boardState = _boardQuery.GetSingletonBuffer<Board.CellState>();
			var stateCopy = boardState.Reinterpret<bool>().AsNativeArray();

			var fallingCells = new NativeHashSet<int>(10, Allocator.Temp);
			
			for (var row = 0; row < boardSize.Height; row++) {
				for (var column = 0; column < boardSize.Width; column++) {
					var nextColumn = column + gravity.Value.x;
					var nextRow = row + gravity.Value.y;
					var slotIndex = row * boardSize.Width + column;
					
					if (nextColumn < 0 || nextColumn >= boardSize.Width)
						continue;

					if (nextRow < 0 || nextRow >= boardSize.Height)
						continue;
					
					var nextSlotIndex = nextRow * boardSize.Width + nextColumn;
					var isSlotOccupied = stateCopy[nextSlotIndex];
					
					if (isSlotOccupied)
						continue;

					fallingCells.Add(slotIndex);
					stateCopy[slotIndex] = false;
					stateCopy[nextSlotIndex] = true;
				}
			}

			foreach (var (elementId, boardPosition) in SystemAPI.Query<RefRO<Element.Id>, RefRW<Board.Position>>().WithAll<Element.Tag, Element.Is.Dynamic>()) {
				var elementPosition = boardPosition.ValueRO.Value;
				var slotIndex = elementPosition.y * boardSize.Width + elementPosition.x;

				if (!fallingCells.Contains(slotIndex))
					continue;

				var targetPosition = elementPosition + gravity.Value;
				boardPosition.ValueRW.Value = targetPosition;
				
				// create output
				var outputEntity = ecb.CreateEntity();
				ecb.SetName(outputEntity, $"Output<{nameof(EngineOutput.ElementMoved)}>");
				ecb.AddComponent<EngineOutput.Tag>(outputEntity);
				ecb.AddComponent<EngineOutput.ElementMoved>(outputEntity);
				ecb.AddComponent(outputEntity, elementId.ValueRO);
				ecb.AddComponent(outputEntity, boardPosition.ValueRO);
			}
		}
	}

}