using MatchX.Common.Systems;
using Nakuru.Unity.Ecs.Utilities;
using Systems;
using Unity.Entities;

namespace MatchX.Engine
{

	public partial class EngineSimulationSystemsGroup : StrictOrderSystemsGroup
	{
		protected override void OnCreate()
		{
			base.OnCreate();
			
			AddSystemToUpdateList(World.CreateSystem<DestroyEntitiesSystem>());
			AddSystemToUpdateList(World.CreateSystem<EngineTickSystem>());
			
			AddSystemToUpdateList(World.CreateSystem<EngineInputSystemsGroup>());
			
			AddSystemToUpdateList(World.CreateSystem<DestroySlotsOfDestroyedBoardSystem>());
			AddSystemToUpdateList(World.CreateSystem<DestroyElementsOfDestroyedSlotSystem>());
	
			AddSystemToUpdateList(World.CreateSystem<SyncElementSizeSystem>());
			
			AddSystemToUpdateList(World.CreateSystem<GravitySystem>());
			AddSystemToUpdateList(World.CreateSystem<SyncBoardStateSystem>());
			
			AddSystemToUpdateList(World.CreateSystem<FlushEngineOutputSystem>());
		}
	}

}