using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game_server
{
    class HiddenNumber
	{
		public string Number { get; set; }

		public HiddenNumber() { }
		public HiddenNumber(string num) { Number = num; }
		public HiddenNumber(int num) : this(num.ToString()) { }

		public int Length { get { return Number.Length; } }

		private int attempts;
		public int Attempts { get { return attempts; } } // кол-во попыток угадать число

		private int bulls;
		public int Bulls { get { return bulls; } } // колво быков угаданных в последней попытке

		private int cows;
		public int Cows { get { return cows; } } // колво коров угаданных в последней попытке

		public int CheckMatches(string num) // попытка угадать
		{
			if (Number.Length != num.Length)
				return -1;

			bulls = 0;
			cows = 0;
			for (int i = 0; i < num.Length; i++)
			{
				if (Number[i] == num[i])
					bulls++;
				else if (Number.Contains(num[i]))
					cows++;
			}
			return ++attempts;
		}
    }
}
