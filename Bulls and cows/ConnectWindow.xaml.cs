using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Bulls_and_cows
{
	/// <summary>
	/// Interaction logic for ConnectWindow.xaml
	/// </summary>
	public partial class ConnectWindow : Window
	{
		public string EnteredIP { get; set; }
		public ConnectWindow()
		{
			InitializeComponent();

		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (String.IsNullOrWhiteSpace(textboxIp.Text))
				return;

			EnteredIP = textboxIp.Text;
			DialogResult = true;
			//Close();
		}
	}
}
