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
		public bool isConnected { get { return tcpClientOpponent.Connected; } }

		public MainWindow()
		{
			InitializeComponent();

			tryList = new BindingList<Try>();
			playerDataGrid.ItemsSource = tryList;
			btnStart.Focus();
		}
		// PreviewTextInput event to make numeric textbox
		private void textbox_OnlyNumeric(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !IsStringNumeric(e.Text);
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
			else if (e.Key == Key.Enter)
			{
				btnStart.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
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
				Start();
			else
				Try();
		}
		
		int tries = 0;
		private void Try()
		{
			string textboxNumber = getTextboxNumberValue();
			if (!IsStringNumeric(textboxNumber))
			{
				MessageBox.Show("Number format error", "Error");
				return;
			}

			if(isConnected)
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
					Num = tries,
					Number = textboxNumber,
					Bulls = data[0],
					Cows = data[1]
				});

				if(data[0] == 4) // TOREDO
				{
					congratilations();
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

		private void Start()
		{
			tryList.Clear();
			if(isConnected)
			{
				string number = getTextboxNumberValue();
				if(number.Length < 4)
				{
					MessageBox.Show("Incorrect number");
					return;
				}

				answerNumber = new HiddenNumber(number);
			}
			else
			{
				generateNumber();
			}
			isStarted = true;
			btnStart.Content = "Try";
			startTime = DateTime.Now;
			dispatcherTimer = new DispatcherTimer();
			dispatcherTimer.Tick += DispatcherTimer_Tick;
			dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
			dispatcherTimer.Start();
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
			Start();
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
					response.Append(Encoding.UTF8.GetString(data, 0, bytes));
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

		void waitForConnect()
		{
			TcpListener server = new TcpListener(IPAddress.Any, Config.RemotePort);

			// запуск слушателя
			server.Start();

			while (true)
			{
				Console.WriteLine("Ожидание подключений... ");

				// получаем входящее подключение
				TcpClient client = server.AcceptTcpClient();
				Console.WriteLine("Подключен клиент. Выполнение запроса...");

				//
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
			}
			
			return true;
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

					do
					{
						int bytes = stream.Read(data, 0, data.Length);
						response.Append(Encoding.UTF8.GetString(data, 0, bytes));
					}
					while (stream.DataAvailable);

					answerNumber.CheckMatches(response.ToString());

					data = new byte[] { (byte)answerNumber.Bulls, (byte)answerNumber.Cows };
					stream.Write(data, 0, data.Length);
					stream.Close();

					opponentDataGrid.Items.Add(new Try
					{
						Num = answerNumber.Attempts,
						Number = response.ToString(),
						Bulls = answerNumber.Bulls,
						Cows = answerNumber.Cows
					});
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
			finally
			{

			}
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
			ConnectWindow connectWindow = new ConnectWindow();
			bool connectResult = false;

			if (connectWindow.ShowDialog() == true)
			{
				connectResult = connectByIp(connectWindow.EnteredIP);
			}

		}

		void waitWindow()
		{
			new WaitWindow().ShowDialog();
		}
	}
}
