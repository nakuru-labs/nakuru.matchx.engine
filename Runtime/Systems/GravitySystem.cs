using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

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
			var boardGravity = _boardQuery.GetSingleton<Board.Gravity>();
			var boardState = _boardQuery.GetSingletonBuffer<Board.CellState>();
			var stateCopy = boardState.Reinterpret<bool>().AsNativeArray();

			var fallingCells = new NativeHashSet<int>(10, Allocator.Temp);

			var endingPositions = FindAllEndingPositions(ref state, boardSize, boardGravity);

			foreach (var endingPosition in endingPositions) {
				GoThroughReversedGravityPathAndMoveElements(ref stateCopy, ref fallingCells, endingPosition, boardSize, boardGravity);
			}

			foreach (var (elementId, boardPosition, shapeBuffer) in SystemAPI.Query<RefRO<Element.Id>, RefRW<Board.Position>, DynamicBuffer<Element.Shape>>()
			                                                    .WithAll<Element.Tag, Element.Is.Dynamic>()) {
				var elementPosition = boardPosition.ValueRO.Value;
				var slotIndex = elementPosition.y * boardSize.Width + elementPosition.x;

				if (!fallingCells.Contains(slotIndex))
					continue;
				
				// check shape
				var shapes = shapeBuffer.Reinterpret<int2>().ToNativeArray(Allocator.Temp);
				var canElementBeMoved = CanElementBeMoved(elementPosition, boardSize, boardGravity, shapes, stateCopy);
				
				if (!canElementBeMoved)
					continue;
				
				var targetPosition = elementPosition + boardGravity.Value;
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

		private bool CanElementBeMoved(int2 elementPosition, Board.Size boardSize, Board.Gravity boardGravity, NativeArray<int2> elementShapes, NativeArray<bool> boardState)
		{
			foreach (var shape in elementShapes) {
				var shapePosition = elementPosition + shape;
				var nextShapePosition = shapePosition + boardGravity.Value;
				var slotIndex = nextShapePosition.y * boardSize.Width + nextShapePosition.x;
				
				// if it's a position of the element itself
				if (elementShapes.Contains(nextShapePosition - shapePosition))
					continue;

				if (boardState[slotIndex])
					return false;
			}

			return true;
		}

		private NativeList<int2> FindAllEndingPositions(ref SystemState state, Board.Size boardSize, Board.Gravity boardGravity)
		{
			var result = new NativeList<int2>(boardSize.Width * boardSize.Height, Allocator.Temp);
			
			foreach (var slotPosition in SystemAPI.Query<RefRO<Board.Position>>().WithAll<Slot.Tag>()) {
				var nextPosition = slotPosition.ValueRO.Value + boardGravity.Value;
				
				// if nextPosition is out of board bounds then it's an ending position
				// Ending position means that the element can't be moved anywhere further by the gravity 
				if (nextPosition.x < 0 || nextPosition.x > boardSize.Width)
					result.Add(slotPosition.ValueRO.Value);
				else if (nextPosition.y < 0 || nextPosition.y > boardSize.Height)
					result.Add(slotPosition.ValueRO.Value);
			}

			return result;
		}
		
		private void GoThroughReversedGravityPathAndMoveElements(ref NativeArray<bool> boardState,
		                                                         ref NativeHashSet<int> result,
		                                                         int2 endingPosition, 
		                                                         Board.Size boardSize, 
		                                                         Board.Gravity boardGravity)
		{
			var previousPosition = endingPosition;
			bool isOutOfBoardBounds;
			bool isNextSlotFree;
			int counter = 0;

			do {
				var nextSlotIndex = previousPosition.y * boardSize.Width + previousPosition.x;

				isNextSlotFree = !boardState[nextSlotIndex];
				previousPosition -= boardGravity.Value;
				isOutOfBoardBounds = previousPosition.x < 0 || previousPosition.x >= boardSize.Width || previousPosition.y < 0
				                     || previousPosition.y >= boardSize.Height;

				if (isOutOfBoardBounds)
					continue;

				var previousSlotIndex = previousPosition.y * boardSize.Width + previousPosition.x;
				var isPreviousSlotFree = !boardState[previousSlotIndex];

				if (!isPreviousSlotFree && isNextSlotFree) {
					result.Add(previousSlotIndex);
					boardState[previousSlotIndex] = false;
					boardState[nextSlotIndex] = true;
				}

				counter++;

			} while (!isOutOfBoardBounds || counter >= 100);
		}
	}

}