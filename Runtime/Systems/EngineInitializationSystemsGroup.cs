using Unity.Entities;

namespace MatchX.Engine
{

	public partial class EngineInitializationSystemsGroup : ComponentSystemGroup
	{
		protected override void OnCreate()
		{
			base.OnCreate();
			AddSystemToUpdateList(World.CreateSystem<InitializeEngineSystem>());
		}
	}

}