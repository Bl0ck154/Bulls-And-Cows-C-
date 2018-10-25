using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		BindingList<Try> tryList;
		public MainWindow()
		{
			InitializeComponent();
			tryList = new BindingList<Try>();
			playerDataGrid.ItemsSource = tryList;
			focusOnFirst();
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
			string textboxNumber = getTextboxNumberValue();
			if(!IsStringNumeric(textboxNumber))
			{
				MessageBox.Show("Number format error", "Error");
				return;
			}

			for (int i = 0; i < textboxNumber.Length; i++)
			{
				if (answerNumber[i] == textboxNumber[i])
					bulls++;
				else if (answerNumber.Contains(textboxNumber[i]))
					cows++;
			}
			tries++;
			tryList.Add(new Try { Num = tries, Number = textboxNumber, Bulls= bulls,  Cows=cows });
			//	playerDataGrid.ScrollIntoView(playerDataGrid.Items[playerDataGrid.Items.Count-1]);
			focusOnFirst();

			if(bulls == answerNumber.Length)
			{
				MessageBox.Show("Congratilations! You win!", "You win!");
				isStarted = false;
				btnStart.Content = "Start";
			}
		}

		string getTextboxNumberValue()
		{
			return textboxNum1.Text + textboxNum2.Text + textboxNum3.Text + textboxNum4.Text;
		}

		private void Start()
		{
			answerNumber = "";
			tries = 0;
			tryList.Clear();
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
	}
}
