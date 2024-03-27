using Unity.Entities;

namespace MatchX.Engine
{

	public struct EngineOutput
	{
		public struct Tag : IComponentData
		{ }
		
		public struct BoardCreated : IComponentData
		{ }
			
		public struct ElementCreated : IComponentData
		{ }
		
		public struct ElementDestroyed : IComponentData
		{ }
		
		public struct ElementMoved : IComponentData
		{ }

		internal struct Gateway : IComponentData
		{
			private EntityManager _outputEntityManager;

			internal Gateway(EntityManager outputEntityManager) => _outputEntityManager = outputEntityManager;
			
			public void FlushOutputs(EntityManager sourceEntityManager, EntityQuery query)
			{
				_outputEntityManager.MoveEntitiesFrom(sourceEntityManager, query);
			}
		}
	}

}