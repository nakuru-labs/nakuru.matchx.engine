using Nakuru.Unity.Ecs.Utilities;
using Unity.Entities;

namespace MatchX.Engine.Tests.Common
{

	public abstract class EcsTestFixture : BaseEcsTestFixture
	{
		protected override World InitializeWorld()
		{
			var builder = WorldBuilder.NewWorld("Test World", WorldFlags.Game, true);
			InitializeSystems(builder);
			return builder.Build();
		}
		
		protected virtual void InitializeSystems(WorldBuilder builder) { }
	}

}