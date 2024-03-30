using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

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
			
			foreach (var (positionRo, shapeBufferRo) in SystemAPI.Query<RefRO<Board.Position>, DynamicBuffer<Element.Shape>>()
			                                                     .WithAll<Element.Tag>()) {
				var elementPosition = positionRo.ValueRO.Value;
				
				// go through all the element's shapes
				foreach (var shape in shapeBufferRo) {
					var shapePosition = elementPosition + (int2)shape.Index;
					var cellIndex = shapePosition.y * boardSize.Width + shapePosition.x;
					occupiedIndexes.Add(cellIndex);
				}
			}
			
			for (var i = 0; i < boardState.Length; i++) {
				var isOccupied = occupiedIndexes.Contains(i);
				boardState[i] = isOccupied;
			}
		}
	}

}