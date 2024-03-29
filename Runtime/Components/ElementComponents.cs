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
			public uint2 Index;
			
			public static implicit operator uint2(Shape shape)
			{
				return shape.Index;
			}

			public static implicit operator Shape(uint2 index)
			{
				return new Shape { Index = index };
			}
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