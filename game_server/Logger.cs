using System;

namespace game_server
{
	public static class Logger
	{
		public static void Log(string message)
		{
			Console.WriteLine(DateTime.Now + " : " + message);
		}
	}
}
