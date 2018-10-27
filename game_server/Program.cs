using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace game_server
{
	class Program
	{
		const int port = 5678; // порт для прослушивания подключений
		static void Main(string[] args)
		{
			TcpListener server = null;
			Queue<string> ClientsQueue = new Queue<string>();
			try
			{
				IPAddress localAddr = IPAddress.Parse("127.0.0.1");
				server = new TcpListener(localAddr, port);

				// запуск слушателя
				server.Start();

				while (true)
				{
					Console.WriteLine("Ожидание подключений... ");

					// получаем входящее подключение
					TcpClient client = server.AcceptTcpClient();
					Console.WriteLine("Подключен клиент. Выполнение запроса...");

					if (ClientsQueue.Count == 0)
					{
						IPEndPoint iPEndPoint = client.Client.LocalEndPoint as IPEndPoint;
						ClientsQueue.Enqueue($"{iPEndPoint.Address}:{iPEndPoint.Port}");
					}
					else
					{
						// получаем сетевой поток для чтения и записи
						NetworkStream stream = client.GetStream();

						byte[] data = Encoding.UTF8.GetBytes(ClientsQueue.Dequeue());

						// отправка сообщения
						stream.Write(data, 0, data.Length);
						stream.Close();
					}
					// закрываем подключение
					client.Close();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
			finally
			{
				if (server != null)
					server.Stop();
			}
		}
	}
}
