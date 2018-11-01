using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace game_server
{
	class ConnectServer
	{
		const int port = 7766;
		public ConnectServer()
		{

		}
		public void Start()
		{
			TcpListener server = null;
			Queue<string> ClientsQueue = new Queue<string>();
			try
			{
				IPAddress localAddr = IPAddress.Any;
				server = new TcpListener(localAddr, port);

				// запуск слушателя
				server.Start();

				while (true)
				{
					Console.WriteLine("Ожидание подключений... ");

					// получаем входящее подключение
					TcpClient client = server.AcceptTcpClient();

					IPEndPoint iPEndPoint = client.Client.LocalEndPoint as IPEndPoint;
					string clientIPPort = $"{iPEndPoint.Address}:{iPEndPoint.Port}";
					Console.WriteLine($"Подключен клиент {clientIPPort}. Выполнение запроса...");

					if (ClientsQueue.Count == 0)
					{
						ClientsQueue.Enqueue(clientIPPort);
					}
					else
					{
						// получаем сетевой поток для чтения и записи
						NetworkStream stream = client.GetStream();

						byte[] data = Encoding.Unicode.GetBytes(ClientsQueue.Dequeue());

						// отправка сообщения
						stream.Write(data, 0, data.Length);
						stream.Close();
					}
					// закрываем подключение
					client.Close();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			finally
			{
				if (server != null)
					server.Stop();
			}
		}
	}
}
