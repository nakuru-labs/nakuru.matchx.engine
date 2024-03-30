using MatchX.Engine.Tests.Common;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace MatchX.Engine.Tests.EditMode
{

	public class ElementCreationTests : EngineTestFixture
	{
		[Test]
		public void When_ElementCreated_ItShouldHaveProperPosition()
		{
			var position = new int2(4, 3);
			var entity = CreateDynamicElement(position, GetShape1x1());

			Update();

			Assert.That(position, Is.EqualTo(EntityManager.GetComponentData<Board.Position>(entity).Value));
		}
		
		[Test]
		public void When_ElementCreated_ItShouldHaveProperSize()
		{
			var expectedSize1x1 = new uint2(1, 1);
			var expectedSize2x1 = new uint2(2, 1);
			var expectedSize1x2 = new uint2(1, 2);
			var expectedSize2x2 = new uint2(2, 2);
			
			var entity1x1 = CreateDynamicElement(new int2(0, 1), GetShape1x1());
			var entity2x1 = CreateDynamicElement(new int2(0, 2), GetShape2x1());
			var entity1x2 = CreateDynamicElement(new int2(0, 3), GetShape1x2());
			var entity2x2 = CreateDynamicElement(new int2(0, 4), GetShape2x2());

			Update();

			Assert.That(expectedSize1x1, Is.EqualTo(EntityManager.GetComponentData<Element.Size>(entity1x1).Value));
			Assert.That(expectedSize2x1, Is.EqualTo(EntityManager.GetComponentData<Element.Size>(entity2x1).Value));
			Assert.That(expectedSize1x2, Is.EqualTo(EntityManager.GetComponentData<Element.Size>(entity1x2).Value));
			Assert.That(expectedSize2x2, Is.EqualTo(EntityManager.GetComponentData<Element.Size>(entity2x2).Value));
		}
		
		[Test]
		public void When_ElementCreated_ItShouldOccupyAllCellsRegardingItsShape()
		{
			var expectedSize1x1 = new uint2(1, 1);
			var expectedSize2x1 = new uint2(2, 1);
			var expectedSize1x2 = new uint2(1, 2);
			var expectedSize2x2 = new uint2(2, 2);

			var element1x1Pos = new int2(0, 0);
			var element2x1Pos = new int2(1, 0);
			var element1x2Pos = new int2(0, 1);
			var element2x2Pos = new int2(1, 1);

			var shape1x1 = GetShape1x1();
			var shape2x1 = GetShape2x1();
			var shape1x2 = GetShape1x2();
			var shape2x2 = GetShape2x2();
			
			CreateDynamicElement(element1x1Pos, shape1x1);
			CreateDynamicElement(element2x1Pos, shape2x1);
			CreateDynamicElement(element1x2Pos, shape1x2);
			CreateDynamicElement(element2x2Pos, shape2x2);

			Update();

			CheckElementOccupiedState(element1x1Pos, shape1x1);
			CheckElementOccupiedState(element2x1Pos, shape2x1);
			CheckElementOccupiedState(element1x2Pos, shape1x2);
			CheckElementOccupiedState(element2x2Pos, shape2x2);
		}

		private void CheckElementOccupiedState(int2 elementPos, NativeArray<uint2> shape)
		{
			var boardStateBuffer = GetBoardState().Reinterpret<bool>();

			foreach (var index in shape) {
				var shapeTilePosition = elementPos + (int2)index;
				var cellIndex = shapeTilePosition.y * BoardSize.Width + shapeTilePosition.x;	
				Assert.That(boardStateBuffer[cellIndex], Is.True);
			}
		}
		
		[Test]
		public void When_ElementShapeChanged_TheSizeShouldBeChangedRespectively()
		{
			var expectedSize1x1 = new uint2(1, 1);
			var expectedSize2x1 = new uint2(2, 1);
			
			var entity = CreateDynamicElement(new int2(0, 1), GetShape1x1());
			
			Update();
			
			Assert.That(expectedSize1x1, Is.EqualTo(EntityManager.GetComponentData<Element.Size>(entity).Value));
			
			// add shape tile
			var shapeBuffer = EntityManager.GetBuffer<Element.Shape>(entity);
			shapeBuffer.Add(new uint2(1, 0));

			Update();

			Assert.That(expectedSize2x1, Is.EqualTo(EntityManager.GetComponentData<Element.Size>(entity).Value));
			
			// remove shape tile
			shapeBuffer = EntityManager.GetBuffer<Element.Shape>(entity);
			shapeBuffer.RemoveAt(1);
			
			Update();

			Assert.That(expectedSize1x1, Is.EqualTo(EntityManager.GetComponentData<Element.Size>(entity).Value));
		}
	}

}