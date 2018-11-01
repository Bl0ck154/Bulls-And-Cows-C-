using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace game_server
{
	class BullAndCowsServer
	{
		const int port = 7766;
		TcpListener listener;
		Queue<PlayerObject> ClientsQueue;
		public BullAndCowsServer()
		{
			ClientsQueue = new Queue<PlayerObject>();

		}
		public void Start()
		{
			try
			{
				listener = new TcpListener(IPAddress.Any, port);
				listener.Start();
				Console.WriteLine("Ожидание подключений...");

				while (true)
				{
					TcpClient client = listener.AcceptTcpClient();
					PlayerObject clientObject = new PlayerObject(client);

					if (ClientsQueue.Count > 0)
					{
						PlayerObject playerTwo = ClientsQueue.Dequeue();
						clientObject.opponent = playerTwo;
						playerTwo.opponent = clientObject;
					}
					else
						ClientsQueue.Enqueue(clientObject);

					// создаем новый поток для обслуживания нового клиента
					Task.Run(() => clientObject.Process());
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			finally
			{
				if (listener != null)
					listener.Stop();
			}

		}
	}
}
