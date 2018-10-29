using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_and_cows
{
	class Attempt
	{
		public int Num { get; set; }
		public string Number { get; set; }
		public int Bulls { get; set; }
		public int Cows { get; set; }
		public override string ToString()
		{
			return Num + ".  " + Number + " - " + Bulls + " - " + Cows;
		}
	}
}
