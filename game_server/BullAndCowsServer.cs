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
		List<PlayerObject> ClientsQueue;

		public BullAndCowsServer()
		{
			ClientsQueue = new List<PlayerObject>();
		}

		public void Start()
		{
			try
			{
				listener = new TcpListener(IPAddress.Any, port);
				listener.Start();
				Console.WriteLine("Ожидание подключений...");
				IPEndPoint iPEndPoint;

				while (true)
				{
					TcpClient client = listener.AcceptTcpClient();

					iPEndPoint = (client.Client.RemoteEndPoint as IPEndPoint);
					Console.WriteLine($"Подключен клиент {iPEndPoint.Address}:{iPEndPoint.Port}...");

					// при подключении клиента создавать обьект класса игрока
					PlayerObject clientObject = new PlayerObject(client);
					clientObject.ClientDisconnected += ClientObject_ClientDisconnected;

					// проверка первого в очереди клиента
					if (ClientsQueue.First()?.client.Connected == false)
					{
						ClientsQueue.RemoveAt(0);
						Console.WriteLine("1-й клиент из очереди не отвечает. Отключаем...");
					}

					// из очереди присваиваем игроку соперника - другого игрока, или добавляем в очередь при отсутствии
					if (ClientsQueue.Count > 0)
					{
						PlayerObject playerTwo = ClientsQueue.First(); // получаем первый
						ClientsQueue.Remove(playerTwo); // удаляем из очереди
						clientObject.opponent = playerTwo;
						playerTwo.opponent = clientObject;

						iPEndPoint = (playerTwo.client.Client.RemoteEndPoint as IPEndPoint);
						Console.WriteLine($"Спарен с {iPEndPoint.Address}:{iPEndPoint.Port}...");
					}
					else
					{
						ClientsQueue.Add(clientObject);
						Console.WriteLine("Отправлен в очередь");
					}

					// создаем новый поток для обслуживания нового игрока
					Task.Run(() => clientObject.Process());
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("/n----- EXCEPTION -----");
				Console.WriteLine(ex.ToString());
				Console.ReadKey();
			}
			finally
			{
				if (listener != null)
					listener.Stop();
			}
		}

		private void ClientObject_ClientDisconnected(PlayerObject playerObject)
		{
			// TODO fix
			ClientsQueue.Remove(playerObject);
			Console.WriteLine($"Отключен {playerObject.HostName}");
		}
	}
}