using NUnit.Framework;
using Unity.Entities;

namespace MatchX.Engine.Tests.Common
{

	[TestFixture]
	public abstract class BaseEcsTestFixture
	{
		protected World World { get; private set; }
		protected EntityManager EntityManager { get; private set; }

		private World _previousWorld;

		[SetUp]
		public virtual void Setup()
		{
			_previousWorld = World.DefaultGameObjectInjectionWorld;
			World = InitializeWorld();
			EntityManager = World.EntityManager;
		}

		protected abstract World InitializeWorld();

		protected void Update()
		{
			if (World is not { IsCreated: true })
				return;
			
			World.Update();
		}

		[TearDown]
		public virtual void TearDown()
		{
			if (World is not { IsCreated: true })
				return;
			
			// Note that World.Dispose() already completes all jobs. But some tests may leave tests running when
			// they return, but we can't safely run an internal consistency check with jobs running, so we
			// explicitly complete them here as well.
			World.EntityManager.CompleteAllTrackedJobs();
			
			EntityManager.DestroyAndResetAllEntities();
			
			World.DestroyAllSystemsAndLogException();

			World.Dispose();
			World = null;

			World.DefaultGameObjectInjectionWorld = _previousWorld;
			
			_previousWorld = null;
			EntityManager = default;
		}
	}

}