using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Bulls_and_cows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		bool isStarted = false;
		string answerNumber;
		public MainWindow()
		{
			InitializeComponent();
		}
		// PreviewTextInput event to make numeric textbox
		private void textbox_OnlyNumeric(object sender, TextCompositionEventArgs e)
		{
			e.Handled = IsStringNumeric(e.Text);
		}

		// check string has numbers
		bool IsStringNumeric(string str)
		{
			return Regex.IsMatch(str, "[^0-9]+");
		}

		// PreviewKeyDown event to restrict space key because of PreviewTextInput doesn't catch space
		private void textbox_restrictSpace(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space)
				e.Handled = true;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if(!isStarted)
				Start();
			else
				Try();
		}
		int tries = 0;
		int bulls;
		int cows;
		private void Try()
		{
			bulls = 0;
			cows = 0;
			for (int i = 0; i < textboxNumber.Text.Length; i++)
			{
				if (answerNumber[i] == textboxNumber.Text[i])
					bulls++;
				else if (answerNumber.Contains(textboxNumber.Text[i]))
					cows++;
			}
			tries++;
			playerDataGrid.Items.Add(new { tries, textboxNumber.Text, bulls, cows });
			playerDataGrid.ScrollIntoView(playerDataGrid.Items[playerDataGrid.Items.Count-1]);

			if(bulls == answerNumber.Length)
			{
				MessageBox.Show("Congratilations! You win!", "You win!");
				isStarted = false;
				btnStart.Content = "Start";
			}
		}

		private void Start()
		{
			answerNumber = "";
			tries = 0;
			playerDataGrid.Items.Clear();
			generateNumber();
			isStarted = true;
			btnStart.Content = "Try";
		}

		private void generateNumber()
		{
			Random random = new Random();
			char randomed;
			for (int i = 0; i < 4; i++)
			{
				randomed = random.Next(0, 9).ToString()[0];
				if (i > 0 && answerNumber.Contains(randomed))
					i--;
				else
					answerNumber += randomed;
			}
		}
	}
}
