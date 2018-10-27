using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Bulls_and_cows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		bool isStarted = false;
		HiddenNumber answerNumber;
		BindingList<Try> tryList;
		DateTime startTime;
		DispatcherTimer dispatcherTimer;
		TcpClient tcpClientOpponent;
		public bool isConnected { get { return (tcpClientOpponent!= null && tcpClientOpponent.Connected); } }
		bool opponentIsReady = false;

		public MainWindow()
		{
			InitializeComponent();

			tryList = new BindingList<Try>();
			playerDataGrid.ItemsSource = tryList;
			btnStart.Focus();
			
		}
		
		private void textbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			// numeric textbox || avoid to repeating digits
			e.Handled = !IsStringNumeric(e.Text) || checkForRepeats(sender as TextBox, e.Text);
		}

		bool checkForRepeats(TextBox sender, string text)
		{
			// TODO better way
			return getTextboxNumberValue().Contains(text);
		}

		// check string has numbers
		bool IsStringNumeric(string str)
		{
			return Regex.IsMatch(str, "^[0-9]+$");
		}

		// PreviewKeyDown event to restrict space key because of PreviewTextInput doesn't catch space
		private void textbox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space)
			{
				e.Handled = true;
			}
			else if (e.Key == Key.Back && (sender as TextBox).Text == "" && !e.IsRepeat)
			{
				myMoveFocus(FocusNavigationDirection.Previous);
				e.Handled = true;
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (!isStarted)
				BtnStart();
			else
				Try();
		}
		const byte readyPacket = 234;
		private void BtnStart()
		{
			tryList.Clear();
			if (isConnected)
			{
				string number = getTextboxNumberValue();
				if (number.Length < 4)
				{
					MessageBox.Show("Incorrect number");
					return;
				}

				answerNumber = new HiddenNumber(number);

				//  одновременный старт
				NetworkStream networkStream = tcpClientOpponent.GetStream();
				networkStream.Write(new byte[] { readyPacket }, 0, 1);
			//	networkStream.Close();

				if (opponentIsReady)
					StartGame();
			}
			else
			{
				SingleGameStart();
			}

		}

		void SingleGameStart()
		{
			generateNumber();
			StartGame();
		}

		void StartGame()
		{
			isStarted = true;
			btnStart.Content = "Try";
			startTime = DateTime.Now;
			dispatcherTimer = new DispatcherTimer();
			dispatcherTimer.Tick += DispatcherTimer_Tick;
			dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
			dispatcherTimer.Start();
		}

		int tries = 0;
		private void Try()
		{
			string textboxNumber = getTextboxNumberValue();
			if (!IsStringNumeric(textboxNumber) || textboxNumber.Length < answerNumber.Length)
			{
				MessageBox.Show("Number format error", "Error");
				return;
			}

			if(isConnected)
			{
				try
				{
					NetworkStream networkStream = tcpClientOpponent.GetStream();
					byte[] data = Encoding.Unicode.GetBytes(textboxNumber);
					networkStream.Write(data, 0, data.Length);

					data = new byte[256];
					int bytes = 0;
					do
					{
						bytes = networkStream.Read(data, 0, data.Length);
					} while (networkStream.DataAvailable);

					tryList.Add(new Try
					{
						Num = ++tries,
						Number = textboxNumber,
						Bulls = data[0],
						Cows = data[1]
					});

					if (data[0] == 4) // TODO
					{
						congratilations();
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
				}
			}
			else
			{
				answerNumber.CheckMatches(textboxNumber);
				tryList.Add(new Try
				{
					Num = answerNumber.Attempts,
					Number = textboxNumber,
					Bulls = answerNumber.Bulls,
					Cows = answerNumber.Cows
				});
				//	playerDataGrid.ScrollIntoView(playerDataGrid.Items[playerDataGrid.Items.Count-1]);
				
				if (answerNumber.Bulls == answerNumber.Length)
				{
					congratilations();
				}
			}
			focusOnFirst();
		}

		void congratilations()
		{
			MessageBox.Show("Congratilations! You win!", "You win!");
			isStarted = false;
			btnStart.Content = "Start";
			dispatcherTimer.Stop();
		}

		string getTextboxNumberValue()
		{
			return textboxNum1.Text + textboxNum2.Text + textboxNum3.Text + textboxNum4.Text;
		}

		private void DispatcherTimer_Tick(object sender, EventArgs e)
		{
			textTimer.Text = (DateTime.Now - startTime).ToString(@"mm\:ss");
		}

		private void generateNumber()
		{
			Random random = new Random();
			char randomed;
			string generatedNum = "";
			for (int i = 0; i < 4; i++)
			{
				randomed = random.Next(0, 9).ToString()[0];
				if (i > 0 && generatedNum.Contains(randomed))
					i--;
				else
					generatedNum += randomed;
			}
			answerNumber = new HiddenNumber(generatedNum);
		}

		private void MenuItemExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void textbox_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox textBox = (sender as TextBox);
			if (textBox.Text != "" && !textBox.Name.Contains('4'))
			{
				myMoveFocus(FocusNavigationDirection.Next);
				setTextSelection(getFocusedControl() as TextBox);
			}
			else if (textBox.Text == "" && !textBox.Name.Contains('1'))
			{
				myMoveFocus(FocusNavigationDirection.Previous);
				setTextSelection(getFocusedControl() as TextBox);
			}
		}
		void myMoveFocus(FocusNavigationDirection fnd)
		{
			TraversalRequest tRequest = new TraversalRequest(fnd);
			UIElement keyboardFocus = getFocusedControl();

			if (keyboardFocus != null)
			{
				keyboardFocus.MoveFocus(tRequest);
			}
		}
		UIElement getFocusedControl()
		{
			return Keyboard.FocusedElement as UIElement;
		}

		void focusOnFirst()
		{
			textboxNum1.Focus();
			setTextSelection(textboxNum1);
		}
		void setTextSelection(TextBox control)
		{
			control.SelectAll();
		}

		private void textbox_GotMouseCapture(object sender, MouseEventArgs e)
		{
			// TODO fix
			setTextSelection(sender as TextBox);
		}

		private void textbox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			setTextSelection(sender as TextBox);
		}

		private void textbox_GotFocus(object sender, RoutedEventArgs e)
		{
			(sender as TextBox).CaptureMouse();
		}

		private void MenuItemNewGame_Click(object sender, RoutedEventArgs e)
		{
			SingleGameStart();
		}

		private void MenuItemFind_Click(object sender, RoutedEventArgs e)
		{
			const int port = 8888;
			const string server = "127.0.0.1"; // TODO

			try
			{
				TcpClient client = new TcpClient();
				client.Connect(server, port);

				byte[] data = new byte[256];
				StringBuilder response = new StringBuilder();
				NetworkStream stream = client.GetStream();

				//TODO Listening

				do
				{
					int bytes = stream.Read(data, 0, data.Length);
					response.Append(Encoding.Unicode.GetString(data, 0, bytes));
				}
				while (stream.DataAvailable); // пока данные есть в потоке

				connectByIp(response.ToString());

				// Закрываем потоки
				stream.Close();
				client.Close();
			}
			catch (SocketException ex)
			{
				MessageBox.Show($"{ex}", "SocketException");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		void showOpponentsUIElements()
		{
			opponentDataGrid.IsEnabled = true;
			textOnline.Text = "You playing online with: " + (tcpClientOpponent.Client.RemoteEndPoint as IPEndPoint).Address.ToString();
		}

		void waitForConnect()
		{
			try
			{
				TcpListener server = new TcpListener(IPAddress.Any, Config.RemotePort);

				server.Start();

				WaitWindow waitWindow = new WaitWindow() { Owner = this };
				Task task = new Task(() => {
					while (tcpClientOpponent == null)
					{
						Console.WriteLine("Ожидание подключений... ");

						tcpClientOpponent = server.AcceptTcpClient();
						Console.WriteLine("Подключен клиент. Выполнение запроса...");

						this.Dispatcher.Invoke(() => {
							waitWindow.Close();
							showOpponentsUIElements();
							MessageBox.Show("Соедениние успешно.\nЗагадайте число и нажмите старт");
							Task.Run(() => listenOpponent());
						});
					}
				});
				task.Start();
				waitWindow.ShowDialog(); // TODO stop task when closed

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
	
		public bool connectByIp(string IPPort)
		{
			string[] parts = IPPort.Split(':');
			string Ip = parts[0];
			if (!ValidateIPv4(Ip) || parts.Length < 2)
			{
				MessageBox.Show("Incorrect IP address - " + Ip, "Error");
				return false;
			}
			string port = parts[1];
			if(!IsStringNumeric(port))
			{
				MessageBox.Show("Incorrect port - " + port, "Error");
				return false;
			}

			try
			{
				tcpClientOpponent = new TcpClient();
				tcpClientOpponent.Connect(Ip, Int32.Parse(port));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				return false;
			}

			if (tcpClientOpponent.Connected)
			{
				MessageBox.Show("Соедениние успешно.\nЗагадайте число и нажмите старт");
				Task.Run(()=>listenOpponent());
				showOpponentsUIElements();
			}
			
			return true;
		}

		void opponentReady()
		{
			opponentIsReady = true;
			if(answerNumber != null)
				StartGame();
		}

		private void listenOpponent()
		{
			try
			{
				while (tcpClientOpponent.Connected)
				{
					byte[] data = new byte[256];
					StringBuilder response = new StringBuilder();
					NetworkStream stream = tcpClientOpponent.GetStream();

					bool receivedReady = false;
					int bytes;
					do
					{
						bytes = stream.Read(data, 0, data.Length);

						if(bytes == 1 && data[0] == readyPacket)
							receivedReady = true;

						if(opponentIsReady)
							response.Append(Encoding.Unicode.GetString(data, 0, bytes));
					} while (stream.DataAvailable);


					if (receivedReady)
					{
						this.Dispatcher.Invoke(() => opponentReady());
			//			stream.Close();
						continue;
					}

					if (!opponentIsReady || answerNumber == null)
					{
			//			stream.Close();
						continue;
					}

					answerNumber.CheckMatches(response.ToString());

					data = new byte[] { (byte)answerNumber.Bulls, (byte)answerNumber.Cows };
					stream.Write(data, 0, data.Length);
					//		stream.Close();

					if (bytes == 2) // answer bulls - 0, cows - 1
					{
						continue;
					}

					this.Dispatcher.Invoke(() => opponentDataGrid.Items.Add(new Try
					{
						Num = answerNumber.Attempts,
						Number = response.ToString(),
						Bulls = answerNumber.Bulls,
						Cows = answerNumber.Cows
					}));
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
			finally
			{

				tcpClientOpponent?.Close();
				tcpClientOpponent = null;
				this.Dispatcher.Invoke(() => disableOpponentsUI());
			}
		}

		void disableOpponentsUI()
		{
			textOnline.Text = "";
			opponentDataGrid.IsEnabled = false;
		}

		public bool ValidateIPv4(string ipString)
		{
			if (String.IsNullOrWhiteSpace(ipString))
			{
				return false;
			}

			string[] splitValues = ipString.Split('.');
			if (splitValues.Length != 4)
			{
				return false;
			}

			byte tempForParsing;

			return splitValues.All(r => byte.TryParse(r, out tempForParsing));
		}

		private void MenuItemConnectIP_Click(object sender, RoutedEventArgs e)
		{
			ConnectWindow connectWindow = new ConnectWindow() { Owner = this };
			bool connectResult = false;

			if (connectWindow.ShowDialog() == true)
			{
				connectResult = connectByIp(connectWindow.EnteredIP);
			}

		}

		private void MenuItemWait_Click(object sender, RoutedEventArgs e)
		{
			waitForConnect();
		}
	}
}
