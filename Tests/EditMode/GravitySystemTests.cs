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
		
		[Test]
		public void When_GravityDownX1_Elements2x2_ShouldMoveDownByTheGravity()
		{
			var gravity = new int2(0, -1);
			var startPosition = new int2(0, 4);
			
			var entity = CreateDynamicElement(startPosition, GetShape2x2());

			SetGravity(gravity);
			Update();

			var targetPosition = startPosition + gravity;

			Assert.AreEqual(targetPosition, EntityManager.GetComponentData<Board.Position>(entity).Value);
		}
		
		[Test]
		public void When_GravityUpX1_Elements1x1_ShouldMoveUpByTheGravity()
		{
			var gravity = new int2(0, 1);
			var startPosition = new int2(0, 0);
			
			var entity = CreateDynamicElement(startPosition, GetShape1x1());

			SetGravity(gravity);
			Update();

			var targetPosition = startPosition + gravity;

			Assert.AreEqual(targetPosition, EntityManager.GetComponentData<Board.Position>(entity).Value);
		}
		
		[Test]
		public void When_GravityLeftX1_Elements1x1_ShouldMoveLeftByTheGravity()
		{
			var gravity = new int2(-1, 0);
			var startPosition = new int2(4, 0);
			
			var entity = CreateDynamicElement(startPosition, GetShape1x1());

			SetGravity(gravity);
			Update();

			var targetPosition = startPosition + gravity;

			Assert.AreEqual(targetPosition, EntityManager.GetComponentData<Board.Position>(entity).Value);
		}
		
		[Test]
		public void When_GravityRightX1_Elements1x1_ShouldMoveRightByTheGravity()
		{
			var gravity = new int2(1, 0);
			var startPosition = new int2(0, 0);
			
			var entity = CreateDynamicElement(startPosition, GetShape1x1());

			SetGravity(gravity);
			Update();

			var targetPosition = startPosition + gravity;

			Assert.AreEqual(targetPosition, EntityManager.GetComponentData<Board.Position>(entity).Value);
		}
		
		[Test]
		public void When_GravityUpX1_Elements2x2_ShouldMoveUpByTheGravity()
		{
			var gravity = new int2(0, 1);
			var startPosition = new int2(0, 0);
			
			var entity = CreateDynamicElement(startPosition, GetShape2x2());

			SetGravity(gravity);
			Update();

			var targetPosition = startPosition + gravity;

			Assert.AreEqual(targetPosition, EntityManager.GetComponentData<Board.Position>(entity).Value);
		}
		
		[Test]
		public void When_GravityLeftX1_Elements2x2_ShouldMoveLeftByTheGravity()
		{
			var gravity = new int2(-1, 0);
			var startPosition = new int2(4, 0);
			
			var entity = CreateDynamicElement(startPosition, GetShape2x2());

			SetGravity(gravity);
			Update();

			var targetPosition = startPosition + gravity;

			Assert.AreEqual(targetPosition, EntityManager.GetComponentData<Board.Position>(entity).Value);
		}
		
		[Test]
		public void When_GravityRightX1_Elements2x2_ShouldMoveRightByTheGravity()
		{
			var gravity = new int2(1, 0);
			var startPosition = new int2(0, 0);
			
			var entity = CreateDynamicElement(startPosition, GetShape2x2());

			SetGravity(gravity);
			Update();

			var targetPosition = startPosition + gravity;

			Assert.AreEqual(targetPosition, EntityManager.GetComponentData<Board.Position>(entity).Value);
		}
		
		[Test]
		public void When_GravityDownX1_Elements2x2_FallOnTheElement1x1()
		{
			var gravity = new int2(0, -1);
			var startPosition1x1 = new int2(0, 0);
			var startPosition2x2 = new int2(0, 2);
			
			var entity1x1 = CreateDynamicElement(startPosition1x1, GetShape1x1());
			var entity2x2 = CreateDynamicElement(startPosition2x2, GetShape2x2());

			SetGravity(gravity);
			Update();

			var targetPosition2x2 = startPosition2x2 + gravity;
			
			Assert.That(EntityManager.GetComponentData<Board.Position>(entity2x2).Value, Is.EqualTo(targetPosition2x2));
			
			Update();

			Assert.That(EntityManager.GetComponentData<Board.Position>(entity2x2).Value, Is.EqualTo(targetPosition2x2));
		}
		
		[Test]
		public void When_GravityDownX1_Elements2x2_And_Element1x1_MoveTogetherByTheGravity()
		{
			var gravity = new int2(0, -1);
			var startPosition1x1 = new int2(1, 2);
			var startPosition2x2 = new int2(0, 3);
			
			var entity1x1 = CreateDynamicElement(startPosition1x1, GetShape1x1());
			var entity2x2 = CreateDynamicElement(startPosition2x2, GetShape2x2());

			SetGravity(gravity);
			Update();

			var targetPosition1x1 = startPosition1x1 + gravity;
			var targetPosition2x2 = startPosition2x2 + gravity;
			
			Assert.That(EntityManager.GetComponentData<Board.Position>(entity1x1).Value, Is.EqualTo(targetPosition1x1));
			Assert.That(EntityManager.GetComponentData<Board.Position>(entity2x2).Value, Is.EqualTo(targetPosition2x2));
			
			Update();
			
			targetPosition1x1 += gravity;
			targetPosition2x2 += gravity;
			
			Assert.That(EntityManager.GetComponentData<Board.Position>(entity1x1).Value, Is.EqualTo(targetPosition1x1));
			Assert.That(EntityManager.GetComponentData<Board.Position>(entity2x2).Value, Is.EqualTo(targetPosition2x2));
		}
	}

}