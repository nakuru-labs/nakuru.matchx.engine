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
		
		public struct Direction : IComponentData
		{
			public static readonly Direction Up = new() { Value = new int2(0, 1)};
			public static readonly Direction Down = new() { Value = new int2(0, -1)};
			public static readonly Direction Left = new() { Value = new int2(-1, 0)};
			public static readonly Direction Right = new() { Value = new int2(1, 0)};
			
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
			
			public static implicit operator bool(CellState shape)
			{
				return shape.Value;
			}

			public static implicit operator CellState(bool value)
			{
				return new CellState { Value = value };
			}
		}
	}

}