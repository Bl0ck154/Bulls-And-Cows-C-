using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_and_cows
{
	public static class Config
	{
		public const string LocalIP = "127.0.0.1";
		public const string RemoteIP = "127.0.0.1";
		public const int LocalPort = 7766;
		public const int RemotePort = 7766;
		public static string LocalIPPort { get { return LocalIP + ":" + LocalPort; } }
		public static string RemoteIPPort { get { return RemoteIP + ":" + RemotePort; } }
	}
}
