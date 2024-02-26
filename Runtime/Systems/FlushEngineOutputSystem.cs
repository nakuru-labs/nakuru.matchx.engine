using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace MatchX.Engine
{
	
	public partial struct FlushEngineOutputSystem : ISystem
	{
		private EntityQuery _outputsQuery;
		private EntityQuery _gatewaysQuery;
		
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			_outputsQuery = SystemAPI.QueryBuilder().WithAll<EngineOutput.Tag>().Build();
			_gatewaysQuery = SystemAPI.QueryBuilder().WithAll<EngineOutput.Gateway>().Build();
			
			state.RequireForUpdate(_outputsQuery);
			state.RequireForUpdate(_gatewaysQuery);
		}
		
		public void OnUpdate(ref SystemState state)
		{
			var gateways = _gatewaysQuery.ToComponentDataArray<EngineOutput.Gateway>(Allocator.Temp);
			
			foreach (var gateway in gateways) {
				gateway.FlushOutputs(state.EntityManager, _outputsQuery);
			}
		}
	}

}