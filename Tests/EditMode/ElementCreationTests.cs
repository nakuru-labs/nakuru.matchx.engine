using MatchX.Engine.Tests.Common;
using NUnit.Framework;
using Unity.Collections;
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
			var shape = new NativeArray<uint2>(1, Allocator.Temp);
			shape[0] = uint2.zero;
			
			var entity = CreateDynamicElement(position, shape);

			Update();

			Assert.AreEqual(position, EntityManager.GetComponentData<Board.Position>(entity).Value);
		}
		
		[Test]
		public void When_ElementCreated_ItShouldHaveProperSize()
		{
			var expectedSize1x1 = new uint2(1, 1);
			var expectedSize2x1 = new uint2(2, 1);
			var expectedSize1x2 = new uint2(1, 2);
			
			var entity1x1 = CreateDynamicElement(new int2(0, 1), GetShape1x1());
			var entity2x1 = CreateDynamicElement(new int2(0, 2), GetShape2x1());
			var entity1x2 = CreateDynamicElement(new int2(0, 3), GetShape1x2());

			Update();

			Assert.AreEqual(expectedSize1x1, EntityManager.GetComponentData<Element.Size>(entity1x1).Value);
			Assert.AreEqual(expectedSize2x1, EntityManager.GetComponentData<Element.Size>(entity2x1).Value);
			Assert.AreEqual(expectedSize1x2, EntityManager.GetComponentData<Element.Size>(entity1x2).Value);
		}
	}

}