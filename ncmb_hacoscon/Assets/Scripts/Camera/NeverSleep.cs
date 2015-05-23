using UnityEngine;
using System.Collections;

public class NeverSleep : MonoBehaviour {
	//スリープさせないためのプログラム
	void Awake () {
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}
}