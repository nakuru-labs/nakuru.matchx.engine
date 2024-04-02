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
		public void When_GravityZero_Element1x1_DoesntMove()
		{
			var startPosition = new int2(0, 0);
			var entity = CreateDynamicElement(startPosition, GetShape1x1());

			Update();

			Assert.AreEqual(startPosition, EntityManager.GetComponentData<Board.Position>(entity).Value);
		}
		
		[Test]
		public void When_GravityDownX1_Element1x1_ShouldNotMoveOutOfBounds()
		{
			var gravity = new int2(0, -1);
			var startPosition = new int2(0, 4);
			var endPosition = new int2(0, 0);
			var entity = CreateDynamicElement(startPosition, GetShape1x1());

			SetGravity(gravity);
			
			Update();
			Update();
			Update();
			Update();
			Update();
			Update();

			Assert.AreEqual(endPosition, EntityManager.GetComponentData<Board.Position>(entity).Value);
		}

		[Test]
		public void When_GravityDownX1_Element1x1_ShouldMoveDownByOneEachFrame()
		{
			var gravity = new int2(0, -1);
			var startPosition = new int2(0, 4);
			var entity = CreateDynamicElement(startPosition, GetShape1x1());
			
			Assert.AreEqual(startPosition, EntityManager.GetComponentData<Board.Position>(entity).Value);

			SetGravity(gravity);
			Update();

			var currentPosition = startPosition + gravity;
			
			Assert.AreEqual(currentPosition, EntityManager.GetComponentData<Board.Position>(entity).Value);

			Update();

			currentPosition += gravity;
			Assert.AreEqual(currentPosition, EntityManager.GetComponentData<Board.Position>(entity).Value);
		}
		
		[Test]
		public void When_GravityDownX1_TwoSubsequentElements1x1_ShouldMoveDownByTheGravity()
		{
			var gravity = new int2(0, -1);
			var start1Position = new int2(0, 4);
			var start2Position = new int2(0, 3);
			
			var entity1 = CreateDynamicElement(start1Position, GetShape1x1());
			var entity2 = CreateDynamicElement(start2Position, GetShape1x1());

			SetGravity(gravity);
			Update();

			var target1Position = start1Position + gravity;
			var target2Position = start2Position + gravity;

			Assert.AreEqual(target1Position, EntityManager.GetComponentData<Board.Position>(entity1).Value);
			Assert.AreEqual(target2Position, EntityManager.GetComponentData<Board.Position>(entity2).Value);
		}
		
		[Test]
		public void When_GravityDownX1_Element2x1_ShouldNotMoveDownThroughOtherElement1x1()
		{
			// Template
			// XX
			// _X
			
			var gravity = new int2(0, -1);
			var startPosition1x1 = new int2(1, 0);
			var startPosition2x1 = new int2(0, 1);
			
			var entity1x1 = CreateDynamicElement(startPosition1x1, GetShape1x1());
			var entity2x1 = CreateDynamicElement(startPosition2x1, GetShape2x1());

			SetGravity(gravity);
			Update();

			Assert.AreEqual(startPosition2x1, EntityManager.GetComponentData<Board.Position>(entity2x1).Value);
		}
	}

}