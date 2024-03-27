using Unity.Entities;

namespace MatchX.Engine
{

	public struct EngineInput
	{
		public struct Tag : IComponentData
		{ }
			
		public struct CreateBoard : IComponentData
		{ }
		
		public struct DestroyBoard : IComponentData
		{ }
			
		public struct CreateElement : IComponentData
		{ }
		
		public struct TriggerElement : IComponentData
		{ }
			
		public struct KillElement : IComponentData
		{ }
	}

}