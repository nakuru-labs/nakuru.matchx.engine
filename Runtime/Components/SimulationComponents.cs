using Unity.Entities;

namespace MatchX.Engine
{

	public struct Simulation
	{
		public struct Tick : IComponentData
		{
			public int Value;
		}
	}

}