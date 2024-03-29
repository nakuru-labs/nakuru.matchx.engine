using Unity.Entities;
using Unity.Mathematics;

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
		
		public struct Shape : IBufferElementData
		{
			public int2 Index;
		}
		
		public struct Size : IComponentData
		{
			public uint2 Value;
		}

		public struct Is
		{
			public struct Dynamic : IComponentData
			{ }
		}
	}

}