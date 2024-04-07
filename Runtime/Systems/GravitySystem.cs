using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace MatchX.Engine
{

	public struct ElementSortData
	{
		public Entity Entity;
		public int2 Position;
	}

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
			var stateCopy = boardState.Reinterpret<bool>().ToNativeArray(Allocator.Temp);

			var allElementsData = new NativeList<ElementSortData>(10, Allocator.Temp);

			foreach (var (positionRo, entity) in SystemAPI.Query<RefRO<Board.Position>>()
			                                              .WithAll<Element.Tag>()
			                                              .WithEntityAccess()) {
				allElementsData.Add(new ElementSortData {
					Entity = entity,
					Position = positionRo.ValueRO.Value
				});
			}
			
			allElementsData.Sort(new PointDataComparer(boardGravity.Value));

			foreach (var elementData in allElementsData) {
				var shapesBuffer = state.EntityManager.GetBuffer<Element.Shape>(elementData.Entity);
				var shapes = shapesBuffer.Reinterpret<int2>().ToNativeArray(Allocator.Temp);
				var nextElementPosition = elementData.Position + boardGravity.Value;
	
				var allShapesCanFallDown = true;
				var globalElementShapes = new NativeHashSet<int2>(shapes.Length, Allocator.Temp);
	
				foreach (var shape in shapes) {
					globalElementShapes.Add(elementData.Position + shape);
				}
	
				foreach (var shape in shapes) {
					var shapePosition = elementData.Position + shape;
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
						var shapePosition = elementData.Position + shape;
						var shapeSlotIndex = shapePosition.y * boardSize.Width + shapePosition.x;
	
						if (shapeSlotIndex >= 0 && shapeSlotIndex < stateCopy.Length)
							stateCopy[shapeSlotIndex] = false;
					}
	
					// occupy all new cells for the element
					foreach (var shape in shapes) {
						var shapePosition = elementData.Position + shape + boardGravity.Value;
						var shapeSlotIndex = shapePosition.y * boardSize.Width + shapePosition.x;
	
						if (shapeSlotIndex >= 0 && shapeSlotIndex < stateCopy.Length)
							stateCopy[shapeSlotIndex] = true;
					}
	
					var elementId = SystemAPI.GetComponentRO<Element.Id>(elementData.Entity);
					var positionRw = SystemAPI.GetComponentRW<Board.Position>(elementData.Entity);
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
		}
		
		private struct PointDataComparer : IComparer<ElementSortData>
		{
			private int2 _gravity;

			public PointDataComparer(int2 gravity) => _gravity = gravity;

			public int Compare(ElementSortData a, ElementSortData b)
			{
				var value = (b.Position - a.Position) * _gravity;
				return value.y + value.x;
			}
		}
	}

}