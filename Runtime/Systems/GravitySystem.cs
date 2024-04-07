using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace MatchX.Engine
{

	public partial struct GravitySystem : ISystem
	{
		private EntityQuery _boardQuery;
		private EntityQuery _elementsQuery;

		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			_boardQuery = SystemAPI.QueryBuilder().WithAll<Board.Tag, Board.Size, Board.CellState, Board.Gravity>().Build();
			state.RequireForUpdate(_boardQuery);

			_elementsQuery = SystemAPI.QueryBuilder().WithAll<Element.Tag, Board.Position, Element.Shape, Element.Is.Dynamic>().Build();
			state.RequireForUpdate(_elementsQuery);

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
			// var stateOrigin = boardState.Reinterpret<bool>().ToNativeArray(Allocator.Temp);
			var stateCopy = boardState.Reinterpret<bool>().ToNativeArray(Allocator.Temp);
			// var fallingCells = new NativeHashSet<int>(10, Allocator.Temp);
			var elementsMap = new NativeHashMap<int2, Entity>(10, Allocator.Temp);

			var endingPositions = FindAllEndingPositions(ref state, boardSize, boardGravity);

			foreach (var (positionRo, entity) in SystemAPI.Query<RefRO<Board.Position>>()
			                                              .WithAll<Element.Tag>()
			                                              .WithEntityAccess()) {
				elementsMap.Add(positionRo.ValueRO.Value, entity);
			}

			foreach (var endingPosition in endingPositions) {
				var previousPosition = endingPosition;
				bool isOutOfBoardBounds;
				int counter = 0;

				do {
					isOutOfBoardBounds = previousPosition.x < 0 || previousPosition.x >= boardSize.Width || previousPosition.y < 0
					                     || previousPosition.y >= boardSize.Height;

					if (isOutOfBoardBounds)
						continue;

					if (elementsMap.TryGetValue(previousPosition, out var elementEntity)) {
						var shapesBuffer = state.EntityManager.GetBuffer<Element.Shape>(elementEntity);
						var shapes = shapesBuffer.Reinterpret<int2>().ToNativeArray(Allocator.Temp);
						var nextElementPosition = previousPosition + boardGravity.Value;

						var allShapesCanFallDown = true;
						var globalElementShapes = new NativeHashSet<int2>(shapes.Length, Allocator.Temp);

						foreach (var shape in shapes) {
							globalElementShapes.Add(previousPosition + shape);
						}

						foreach (var shape in shapes) {
							var shapePosition = previousPosition + shape;
							var nextShapePosition = shapePosition + boardGravity.Value;
							var slotIndex = nextShapePosition.y * boardSize.Width + nextShapePosition.x;
							
							var isNextPosOutOfBoardBounds = nextShapePosition.x < 0 || nextShapePosition.x >= boardSize.Width 
							                                             || nextShapePosition.y < 0 
							                                             || nextShapePosition.y >= boardSize.Height;
							
							if (isNextPosOutOfBoardBounds) {
								allShapesCanFallDown = false;
								break;
							}

							// if it's a position of the element itself
							if (globalElementShapes.Contains(nextShapePosition))
								continue;

							if (stateCopy[slotIndex]) {
								allShapesCanFallDown = false;
								break;
							}
						}

						if (allShapesCanFallDown) {
							
							// free up all cells for the element
							foreach (var shape in shapes) {
								var shapePosition = previousPosition + shape;
								var shapeSlotIndex = shapePosition.y * boardSize.Width + shapePosition.x;

								if (shapeSlotIndex >= 0 && shapeSlotIndex < stateCopy.Length)
									stateCopy[shapeSlotIndex] = false;
							}

							// occupy all new cells for the element
							foreach (var shape in shapes) {
								var shapePosition = previousPosition + shape + boardGravity.Value;
								var shapeSlotIndex = shapePosition.y * boardSize.Width + shapePosition.x;

								if (shapeSlotIndex >= 0 && shapeSlotIndex < stateCopy.Length)
									stateCopy[shapeSlotIndex] = true;
							}

							var elementId = SystemAPI.GetComponentRO<Element.Id>(elementEntity);
							var positionRw = SystemAPI.GetComponentRW<Board.Position>(elementEntity);
							positionRw.ValueRW.Value = nextElementPosition;

							// create output
							var outputEntity = ecb.CreateEntity();
							ecb.SetName(outputEntity, $"Output<{nameof(EngineOutput.ElementMoved)}>");
							ecb.AddComponent<EngineOutput.Tag>(outputEntity);
							ecb.AddComponent<EngineOutput.ElementMoved>(outputEntity);
							ecb.AddComponent(outputEntity, elementId.ValueRO);
							ecb.AddComponent(outputEntity, new Board.Position { Value = nextElementPosition });
						}
					}

					previousPosition -= boardGravity.Value;
					counter++;
				} while (!isOutOfBoardBounds || counter >= 100);
			}
		}

		private NativeList<int2> FindAllEndingPositions(ref SystemState state, Board.Size boardSize, Board.Gravity boardGravity)
		{
			var result = new NativeList<int2>(boardSize.Width * boardSize.Height, Allocator.Temp);

			foreach (var slotPosition in SystemAPI.Query<RefRO<Board.Position>>().WithAll<Slot.Tag>()) {
				var nextPosition = slotPosition.ValueRO.Value + boardGravity.Value;

				// if nextPosition is out of board bounds then it's an ending position
				// Ending position means that the element can't be moved anywhere further by the gravity 
				if (nextPosition.x < 0 || nextPosition.x >= boardSize.Width)
					result.Add(slotPosition.ValueRO.Value);
				else if (nextPosition.y < 0 || nextPosition.y >= boardSize.Height)
					result.Add(slotPosition.ValueRO.Value);
			}

			return result;
		}
	}

}