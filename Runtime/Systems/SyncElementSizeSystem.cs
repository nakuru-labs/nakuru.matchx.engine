using MatchX.Engine;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems
{

	[RequireMatchingQueriesForUpdate]
	public partial struct SyncElementSizeSystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			foreach (var (shapeBuffer, sizeRw) in SystemAPI.Query<DynamicBuffer<Element.Shape>, RefRW<Element.Size>>()
			                                               .WithAll<Element.Tag>()) {
				var indices = shapeBuffer.Reinterpret<uint2>();

				foreach (var index in indices) {
					if (index.x > sizeRw.ValueRW.Value.x)
						sizeRw.ValueRW.Value.x = index.x;
					
					if (index.y > sizeRw.ValueRW.Value.y)
						sizeRw.ValueRW.Value.y = index.y;
				}
			}
		}
	}

}