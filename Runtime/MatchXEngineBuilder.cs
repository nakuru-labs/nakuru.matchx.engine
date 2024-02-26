using Nakuru.Unity.Ecs.Utilities;
using Unity.Entities;

namespace MatchX.Engine
{

	public static class MatchXEngineBuilder
	{
		public static EngineIo Build(bool appendToPlayerLoop)
		{
			var world = WorldBuilder.NewWorld("Engine World", WorldFlags.Simulation)
			            .WithInitializePhase()
			            .WithSimulationPhase()
			            .WithUnityCoreSystems()
			            // .WithExcludingSystem<VariableRateSimulationSystemGroup>()
			            // .WithExcludingSystem<FixedStepSimulationSystemGroup>()
			            .WithExcludingNameFilter("CompanionGameObjectUpdateSystem")
			            .WithExcludingNameFilter("CompanionGameObjectUpdateTransformSystem")
			            .WithInitializePhaseManagedSystem<EngineInitializationSystemsGroup>()
			            .WithSimulationPhaseManagedSystem<EngineSimulationSystemsGroup>()
			            .Build(appendToPlayerLoop);

			return new EngineIo(world.EntityManager);
		}
	}

}