using Unity.Entities;
using Unity.Mathematics;

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
		
		public void CreateElement(int2 position)
		{
			var ecb = GetBeginSimulationEcb();
			var entity = ecb.CreateEntity();
			ecb.AddComponent<EngineInput.Tag>(entity);
			ecb.AddComponent<EngineInput.CreateElement>(entity);
			ecb.AddComponent(entity, new Board.Position { Value = position });
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