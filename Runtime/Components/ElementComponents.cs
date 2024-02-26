using Unity.Entities;

namespace MatchX.Engine
{

	public struct Element
	{
		public struct Tag : IComponentData
		{ }

		public struct Id : IComponentData
		{
			public uint Value;
		}

		public struct Is
		{
			public struct Dynamic : IComponentData
			{ }
		}
	}

}