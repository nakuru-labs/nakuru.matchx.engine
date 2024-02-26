using Unity.Entities;

namespace MatchX.Engine
{

	public struct IdGenerator : IComponentData
	{
		private uint _lastId;

		public uint Next => ++_lastId;
	}

}