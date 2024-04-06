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
				bool isNextSlotFree;
				int counter = 0;

				do {
					var nextPosition = previousPosition;
					var nextSlotIndex = nextPosition.y * boardSize.Width + nextPosition.x;

					isNextSlotFree = !stateCopy[nextSlotIndex];
					previousPosition -= boardGravity.Value;

					isOutOfBoardBounds = previousPosition.x < 0 || previousPosition.x >= boardSize.Width || previousPosition.y < 0
					                     || previousPosition.y >= boardSize.Height;

					if (isOutOfBoardBounds)
						continue;

					if (elementsMap.TryGetValue(previousPosition, out var elementEntity)) {
						var previousSlotIndex = previousPosition.y * boardSize.Width + previousPosition.x;
						var isPreviousSlotFree = !boardState[previousSlotIndex];
						var shapesBuffer = state.EntityManager.GetBuffer<Element.Shape>(elementEntity);
						var shapes = shapesBuffer.Reinterpret<int2>().ToNativeArray(Allocator.Temp);

						// if the origin tile can fall
						if (!isPreviousSlotFree && isNextSlotFree) {
							var allShapesCanFallDown = true;

							foreach (var shape in shapes) {
								var shapePosition = previousPosition + shape;
								var nextShapePosition = shapePosition + boardGravity.Value;
								var slotIndex = nextShapePosition.y * boardSize.Width + nextShapePosition.x;

								// if it's a position of the element itself
								if (shapes.Contains(shapePosition - nextShapePosition))
									continue;

								if (stateCopy[slotIndex]) {
									allShapesCanFallDown = false;
									break;
								}
							}

							if (allShapesCanFallDown) {
								foreach (var shape in shapes) {
									var shapePosition = previousPosition + shape;
									var shapeSlotIndex = shapePosition.y * boardSize.Width + shapePosition.x;

									if (shapeSlotIndex >= 0 && shapeSlotIndex < stateCopy.Length)
										stateCopy[shapeSlotIndex] = false;
								}

								foreach (var shape in shapes) {
									var shapePosition = nextPosition + shape;
									var shapeSlotIndex = shapePosition.y * boardSize.Width + shapePosition.x;
									stateCopy[shapeSlotIndex] = true;

									if (shapeSlotIndex >= 0 && shapeSlotIndex < stateCopy.Length)
										stateCopy[shapeSlotIndex] = true;
								}

								var elementId = SystemAPI.GetComponentRO<Element.Id>(elementEntity);
								var positionRw = SystemAPI.GetComponentRW<Board.Position>(elementEntity);
								positionRw.ValueRW.Value = nextPosition;

								// create output
								var outputEntity = ecb.CreateEntity();
								ecb.SetName(outputEntity, $"Output<{nameof(EngineOutput.ElementMoved)}>");
								ecb.AddComponent<EngineOutput.Tag>(outputEntity);
								ecb.AddComponent<EngineOutput.ElementMoved>(outputEntity);
								ecb.AddComponent(outputEntity, elementId.ValueRO);
								ecb.AddComponent(outputEntity, new Board.Position { Value = nextPosition });
							}
						}
					}

					counter++;
				} while (!isOutOfBoardBounds || counter >= 100);
			}
		}

		private bool CanElementBeMoved(
			int2 elementPosition, Board.Size boardSize, Board.Gravity boardGravity, NativeArray<int2> elementShapes,
			NativeArray<bool> boardState)
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
				if (nextPosition.x < 0 || nextPosition.x >= boardSize.Width)
					result.Add(slotPosition.ValueRO.Value);
				else if (nextPosition.y < 0 || nextPosition.y >= boardSize.Height)
					result.Add(slotPosition.ValueRO.Value);
			}

			return result;
		}
	}

}