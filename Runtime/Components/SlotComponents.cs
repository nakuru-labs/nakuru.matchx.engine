using Unity.Entities;

namespace MatchX.Engine
{

	public struct Slot
	{
		public struct Tag : IComponentData
		{ }
			
		public struct Elements : IBufferElementData
		{
			public Entity Value;
		}
	}

}