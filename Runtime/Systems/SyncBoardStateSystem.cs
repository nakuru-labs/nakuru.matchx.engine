using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace MatchX.Engine
{
	
	// TODO: Run only when board state is invalidated
	// for now it's running every frame
	public partial struct SyncBoardStateSystem : ISystem
	{
		private EntityQuery _boardQuery;
		
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			_boardQuery = SystemAPI.QueryBuilder().WithAll<Board.Tag, Board.Size, Board.CellState>().Build();
			state.RequireForUpdate(_boardQuery);

			var elementQuery = SystemAPI.QueryBuilder().WithAll<Element.Tag, Board.Position>().Build();
			state.RequireForUpdate(elementQuery);
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{	
			var boardSize = _boardQuery.GetSingleton<Board.Size>();
			var boardState = _boardQuery.GetSingletonBuffer<Board.CellState>();
			var occupiedIndexes = new NativeHashSet<int>(20, Allocator.Temp);
			
			foreach (var positionRo in SystemAPI.Query<RefRO<Board.Position>>().WithAll<Element.Tag>()) {
				var elementPosition = positionRo.ValueRO.Value;
				var slotIndex = elementPosition.y * boardSize.Width + elementPosition.x;
				occupiedIndexes.Add(slotIndex);
			}
			
			for (var i = 0; i < boardState.Length; i++) {
				var isOccupied = occupiedIndexes.Contains(i);
				boardState[i] = new Board.CellState { Value = isOccupied };
			}
		}
	}

}