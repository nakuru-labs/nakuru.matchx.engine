using MatchX.Engine.Tests.Common;
using NUnit.Framework;
using Unity.Mathematics;

namespace MatchX.Engine.Tests.EditMode
{
	
	public class GravitySystemTests : EngineTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			SetBoardSize(5, 5);
		}

		[Test]
		public void When_GravityIsZero_ElementDoesntMove()
		{
			var startPosition = new int2(0, 0);
			var entity = CreateDynamicElement(startPosition, GetShape1x1());

			Update();

			Assert.AreEqual(startPosition, EntityManager.GetComponentData<Board.Position>(entity).Value);
		}
		
		[Test]
		public void When_GravityIsOne_ByAxisY_ElementShouldNotMoveOutOfBounds()
		{
			var gravity = new int2(0, 1);
			var startPosition = new int2(0, 2);
			var endPosition = new int2(0, 4);
			var entity = CreateDynamicElement(startPosition, GetShape1x1());

			SetGravity(gravity);
			
			Update();
			Update();
			Update();
			Update();

			Assert.AreEqual(endPosition, EntityManager.GetComponentData<Board.Position>(entity).Value);
		}

		[Test]
		public void When_GravityIsOne_ByAxisY_ElementShouldMoveDownByOne()
		{
			var gravity = new int2(0, 1);
			var startPosition = new int2(0, 0);
			var entity = CreateDynamicElement(startPosition, GetShape1x1());

			SetGravity(gravity);
			Update();

			var currentPosition = startPosition + gravity;
			
			Assert.AreEqual(currentPosition, EntityManager.GetComponentData<Board.Position>(entity).Value);

			Update();

			currentPosition += gravity;
			Assert.AreEqual(currentPosition, EntityManager.GetComponentData<Board.Position>(entity).Value);
		}

		
		
		[Test]
		public void When_GravityIsOne_ByAxisY_TwoSubsequentElementsShouldMoveDownByTheGravity()
		{
			var gravity = new int2(0, 1);
			var start1Position = new int2(0, 0);
			var start2Position = new int2(0, 1);
			
			var entity1 = CreateDynamicElement(start1Position, GetShape1x1());
			var entity2 = CreateDynamicElement(start2Position, GetShape1x1());

			SetGravity(gravity);
			Update();

			var target1Position = start1Position + gravity;
			var target2Position = start2Position + gravity;

			Assert.AreEqual(target1Position, EntityManager.GetComponentData<Board.Position>(entity1).Value);
			Assert.AreEqual(target2Position, EntityManager.GetComponentData<Board.Position>(entity2).Value);
		}
	}

}