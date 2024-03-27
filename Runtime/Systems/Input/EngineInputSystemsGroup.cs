using Nakuru.Unity.Ecs.Utilities;
using Unity.Entities;

namespace MatchX.Engine
{

	public partial class EngineInputSystemsGroup : StrictOrderSystemsGroup
	{
		protected override void OnCreate()
		{
			base.OnCreate();
			
			AddSystemToUpdateList(World.CreateSystem<CreateBoardInputSystem>());
			AddSystemToUpdateList(World.CreateSystem<CreateElementInputSystem>());
			AddSystemToUpdateList(World.CreateSystem<TriggerElementInputSystem>());
			AddSystemToUpdateList(World.CreateSystem<DestroyBoardInputSystem>());
		}
	}

}