using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace game_server
{
	public class PlayerObject
	{
		public string HostName;
		public TcpClient client;
		public PlayerObject opponent;
		HiddenNumber number;
		public bool isReady;

		public delegate void ClientDisconnect(PlayerObject playerObject);
		public event ClientDisconnect ClientDisconnected;

		const byte readyPacket = 234;

		public PlayerObject(TcpClient tcpClient)
		{
			client = tcpClient;
			IPEndPoint iPEndPoint = tcpClient.Client.RemoteEndPoint as IPEndPoint;
			HostName = $"{iPEndPoint.Address}:{iPEndPoint.Port}";
		}

		public void Process()
		{
			NetworkStream stream = null;
			try
			{
				bool firstReadySent = false;
				stream = client.GetStream();
				byte[] data = new byte[256]; // буфер для получаемых данных
				while (client.Connected)
				{
					// если соперника нет, то ждем его появления
					while (opponent == null)
					{
						Thread.Sleep(300);
						continue;
					}

					// при появлении cоперника отправляем на клиент пакет готовности
					if (!firstReadySent)
					{
						SendReadyPacket(stream); // клиент закрывает окно ожидания и загадывает число
						firstReadySent = true;
					}

					// получаем сообщение
					// первое сообщение должно быть загаданное число
					StringBuilder builder = new StringBuilder();
					int bytes = 0;
					do
					{
						bytes = stream.Read(data, 0, data.Length);
						builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
					}
					while (stream.DataAvailable);
					Console.WriteLine($"> Received from {HostName} : {builder.ToString()}");

					if (number == null)
					{
						// клиент присылает свое загаданное число
						number = new HiddenNumber(builder.ToString());
						isReady = true;

						if (opponent.isReady)
						{
							SendReadyPacket(stream);
							SendReadyPacket(opponent.client.GetStream());
						}

						continue;
					}
					
					// если соперник не готов, то ждем
					if (!opponent.isReady)
						continue;

					// отправить попытку сопернику
					TcpClient opponentTcpClient = opponent.client;
					data = Encoding.Unicode.GetBytes(builder.ToString());
					opponentTcpClient.GetStream().Write(data, 0, data.Length);
					Console.WriteLine($"> Sent data to opponent {opponent.HostName} : {builder.ToString()}");

					// проверка совпадений
					opponent.number.CheckMatches(builder.ToString());

					// отправка ответа
					data = new byte[] { (byte)opponent.number.Bulls, (byte)opponent.number.Cows };
					stream.Write(data, 0, data.Length);
					stream.Write(data, 0, data.Length); // test kostyl' todo fix
					Console.WriteLine($"> Answer to {HostName} : bulls {data[0]} , cows {data[1]}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"\n========= EXCEPTION ON {HostName} ==========");
				Console.WriteLine(ex);
				Console.WriteLine();
			}
			finally
			{
				stream?.Close();
				client?.Close();
				ClientDisconnected(this);
				opponent.client.Close();
				opponent = null;
			}
		}
		void SendReadyPacket(NetworkStream ns)
		{
			ns.Write(new byte[] { readyPacket }, 0, 1);
			Console.WriteLine($"> Sent ready to {HostName}");
		}
	}
}
