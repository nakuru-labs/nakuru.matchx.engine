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

		protected Entity CreateElement(int2 position, NativeArray<uint2> shape)
		{
			var entity = EntityManager.CreateEntity();
			EntityManager.AddComponent<Element.Tag>(entity);
			EntityManager.AddBuffer<Element.Shape>(entity).AddRange(shape.Reinterpret<Element.Shape>());
			EntityManager.AddComponent<Element.Size>(entity);
			EntityManager.AddComponentData(entity, new Board.Position {
				Value = position
			});

			return entity;
		}

		protected NativeArray<uint2> GetShape1x1()
		{
			var shape = new NativeArray<uint2>(1, Allocator.Temp);
			shape[0] = uint2.zero;
			return shape;
		}
		
		protected NativeArray<uint2> GetShape2x1()
		{
			var shape = new NativeArray<uint2>(2, Allocator.Temp);
			shape[0] = uint2.zero;
			shape[1] = new uint2(1, 0);
			return shape;
		}
		
		protected NativeArray<uint2> GetShape1x2()
		{
			var shape = new NativeArray<uint2>(2, Allocator.Temp);
			shape[0] = uint2.zero;
			shape[1] = new uint2(0, 1);
			return shape;
		}
		
		protected NativeArray<uint2> GetShape2x2()
		{
			var shape = new NativeArray<uint2>(4, Allocator.Temp);
			shape[0] = uint2.zero;
			shape[1] = new uint2(1, 0);
			shape[2] = new uint2(0, 1);
			shape[3] = new uint2(1, 1);
			return shape;
		}
		
		protected Entity CreateDynamicElement(int2 position, NativeArray<uint2> shape)
		{
			var entity = CreateElement(position, shape);
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