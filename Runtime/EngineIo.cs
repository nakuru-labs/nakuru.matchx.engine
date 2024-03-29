using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using EntityManager = Unity.Entities.EntityManager;
using IComponentData = Unity.Entities.IComponentData;

namespace MatchX.Engine
{

	public struct EngineIo : IComponentData
	{
		public EntityManager EntityManager { get; private set; }

		internal EngineIo(EntityManager entityManager) => EntityManager = entityManager;
		
		public void InjectOutputGateway(EntityManager gateway)
		{
			var engineOutputGatewayEntity = EntityManager.CreateEntity();
			EntityManager.SetName(engineOutputGatewayEntity, "Engine Output Gateway");
			EntityManager.AddComponentData(engineOutputGatewayEntity, new EngineOutput.Gateway(gateway));
		}

		public void CreateBoard(int width, int height, int2 gravity)
		{
			var ecb = GetBeginSimulationEcb();
			var entity = ecb.CreateEntity();
			ecb.AddComponent<EngineInput.Tag>(entity);
			ecb.AddComponent<EngineInput.CreateBoard>(entity);
			ecb.AddComponent(entity, new Board.Gravity { Value = gravity });
			ecb.AddComponent(entity, new Board.Size {
				Width = width,
				Height = height
			});
		}
		
		public void DestroyBoard()
		{
			var ecb = GetBeginSimulationEcb();
			var entity = ecb.CreateEntity();
			ecb.AddComponent<EngineInput.Tag>(entity);
			ecb.AddComponent<EngineInput.DestroyBoard>(entity);
		}
		
		public void CreateElement(int2 position, NativeArray<uint2> shape)
		{
			var ecb = GetBeginSimulationEcb();
			var entity = ecb.CreateEntity();
			ecb.AddComponent<EngineInput.Tag>(entity);
			ecb.AddComponent<EngineInput.CreateElement>(entity);
			ecb.AddBuffer<Element.Shape>(entity).AddRange(shape.Reinterpret<Element.Shape>());
			ecb.AddComponent(entity, new Board.Position { Value = position });
		}
		
		public void TriggerElement(uint elementId)
		{
			var ecb = GetBeginSimulationEcb();
			var entity = ecb.CreateEntity();
			ecb.AddComponent<EngineInput.Tag>(entity);
			ecb.AddComponent<EngineInput.TriggerElement>(entity);
			ecb.AddComponent(entity, new Element.Id { Value = elementId });
		}
		
		public void KillElementAt(int2 position)
		{
			var ecb = GetBeginSimulationEcb();
			var entity = ecb.CreateEntity();
			ecb.AddComponent<EngineInput.Tag>(entity);
			ecb.AddComponent<EngineInput.KillElement>(entity);
			ecb.AddComponent(entity, new Board.Position { Value = position });
		}

		private EntityCommandBuffer GetBeginSimulationEcb()
		{
			var ecbSingleton = EntityManager.World.GetExistingSystemManaged<BeginSimulationEntityCommandBufferSystem>();
			return ecbSingleton.CreateCommandBuffer();
		}
	}

}