namespace BB
{
	public static class DiServices
	{
		public static IBinder Root { get; private set; }
		public static IBinder Game { get; private set; }
		public static void SetRoot(IBinder binder) => Root = binder;
		public static void SetGame(IBinder binder) => Game = binder;
		public static void PublishRootEvent<T>(T msg) => Publish(Root, msg, "Root Resolver has not been set. Can't publish events.");
		public static void PublishGameEvent<T>(T msg) => Publish(Root, msg, "Game Resolver has not been set. Can't publish events.");
		static void Publish<T>(IBinder binder, T msg, string errorMsg)
		{
			if (Log.AssertNotNull(binder, errorMsg))
				binder.Publish(msg);
		}
	}
}