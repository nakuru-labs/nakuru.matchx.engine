using Unity.Entities;
using Unity.Mathematics;

namespace MatchX.Engine
{
	public struct Board
	{
		public struct Tag : IComponentData
		{ }
		
		public struct Size : IComponentData
		{
			public int Width;
			public int Height;
		}
		
		public struct Gravity : IComponentData
		{
			public int2 Value;
		}
		
		public struct Slots : IBufferElementData
		{
			public Entity Value;
		}
	
		public struct Position : IComponentData
		{
			public int2 Value;
		}
		
		public struct CellState : IBufferElementData
		{
			public bool Value;
		}
	}

}