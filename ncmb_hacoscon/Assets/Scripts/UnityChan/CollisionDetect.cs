using UnityEngine;
using System.Collections;

public class CollisionDetect : MonoBehaviour {
	public bool callbackActive = true;
	
	void OnTriggerStay (Collider col){
		if(callbackActive == false){
			return;
		}
		
		//UnityちゃんがGoalについたら書くスクリプトのOnGoalを作動させる
		if(col.name == "Goal"){
			gameObject.SendMessage("OnGoal");
			return;
		}
	}
	
	void OnDead() {
		callbackActive = false;
	}
	
	void OnGoal() {
		callbackActive = false;
	}
	
}