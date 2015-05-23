using UnityEngine;
using System.Collections;

namespace NCMB
{
	
	public class Rankers
	{
		
		public string time   { get; set; }
		
		// コンストラクタ -----------------------------------
		public Rankers(string _time)
		{
			time = _time;
		}
		
		// ランキングで表示するために文字列を整形 -----------
		public string print()
		{
			return time;
		}
	}
	
}