using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace MatchX.Engine.Tests.Common
{

	public abstract class EngineTestFixture : BaseEcsTestFixture
	{
		protected EngineIo EngineIo { get; private set; }
		protected Entity BoardEntity { get; private set; }

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			EngineIo.CreateBoard(5, 5, int2.zero);
			Update();

			var boardQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Board.Tag>().Build(EntityManager);
			BoardEntity = boardQuery.GetSingletonEntity();
		}

		protected override World InitializeWorld()
		{
			EngineIo = MatchXEngineBuilder.Build(false);
			return EngineIo.EntityManager.World;
		}

		protected Entity CreateElement(int2 position)
		{
			var entity = EntityManager.CreateEntity();
			EntityManager.AddComponent<Element.Tag>(entity);
			EntityManager.AddComponentData(entity, new Board.Position {
				Value = position
			});

			return entity;
		}
		
		protected Entity CreateDynamicElement(int2 position)
		{
			var entity = CreateElement(position);
			EntityManager.AddComponent<Element.Is.Dynamic>(entity);

			return entity;
		}
		
		protected void SetBoardSize(int width, int height)
		{
			EntityManager.SetComponentData(BoardEntity, new Board.Size {
				Width = width,
				Height = height
			});

			var buffer = EntityManager.GetBuffer<Board.CellState>(BoardEntity);
			buffer.ResizeUninitialized(width * height);
		}

		protected void SetGravity(int2 gravity)
		{
			EntityManager.SetComponentData(BoardEntity, new Board.Gravity {
				Value = gravity
			});
		}
	}

}