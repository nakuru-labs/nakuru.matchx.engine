using MatchX.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace MatchX.Engine
{

	public partial struct DestroyBoardInputSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			var query = SystemAPI.QueryBuilder().WithAll<EngineInput.DestroyBoard>().Build();
			state.RequireForUpdate(query);
			state.RequireForUpdate<Board.Tag>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var ecb = new EntityCommandBuffer(Allocator.Temp);
			var boardEntity = SystemAPI.GetSingletonEntity<Board.Tag>();

			foreach (var (_, requestEntity) in SystemAPI.Query<RefRO<EngineInput.DestroyBoard>>()
			                                                     .WithEntityAccess()) {
				ecb.AddComponent<ReadyToDestroy>(requestEntity);
				ecb.AddComponent<ReadyToDestroy>(boardEntity);
			}
			
			ecb.Playback(state.EntityManager);
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state) { }
	}

}