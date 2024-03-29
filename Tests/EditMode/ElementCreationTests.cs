using MatchX.Engine.Tests.Common;
using NUnit.Framework;
using Unity.Mathematics;

namespace MatchX.Engine.Tests.EditMode
{

	public class ElementCreationTests : EngineTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			SetBoardSize(5, 5);
		}
		
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