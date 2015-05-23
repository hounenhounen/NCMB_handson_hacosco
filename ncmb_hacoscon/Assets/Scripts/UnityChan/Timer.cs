using UnityEngine;
using System.Collections;
using NCMB;

public class Timer : MonoBehaviour {
	float startTime;
	float lapTime;
	bool goal = false;
	bool ranking = false;
	private LeaderBoard lBoard;
	bool isRankFetched;
	
	
	void Start () {
		startTime = Time.time;
		lBoard = new LeaderBoard();
		// フラグ初期化
		isRankFetched  = false;
	}
	
	
	void Update () {
		if (goal == false) {
			// 現在の経過時間を算出
			lapTime = Time.time - startTime;
		}
	}
	
	void OnGUI(){
		if (goal == false) {
			// 現在の経過時間を表示
			float timer_x = Screen.width * 8 / 10;
			float timer_y = Screen.height / 10;
			float timer_w = Screen.width * 3 / 10;
			float timer_h = Screen.height / 2;
			GUI.Label (new Rect (timer_x, timer_y, timer_w, timer_h), "Time" + lapTime.ToString ());
		} else {
			// ゴールタイムを表示
			float goal_x = Screen.width / 2;
			float goal_y = Screen.height / 5;
			float goal_w = Screen.width * 2/ 10;
			float goal_h = Screen.height / 10;
			if(ranking == false){
				GUI.Label (new Rect (goal_x - goal_w/2, goal_y, goal_w, goal_h), "Your Goal Time!");
				GUI.Label (new Rect (goal_x - goal_w/2, goal_y*2, goal_w, goal_h), lapTime.ToString ());
				if (GUI.Button(new Rect(goal_x - goal_w, goal_y*3, goal_w, goal_h), "ReStart")) {
					Application.LoadLevel ("world");
				}
				
				if(GUI.Button(new Rect(goal_x, goal_y*3, goal_w, goal_h), "Ranking")){
					ranking = true;
				}
				
			}else{
				// 現在の順位の取得が完了したら1度だけ実行
				if( !isRankFetched ){
					lBoard.fetchTopRankers();
					isRankFetched = true;
				}
				
				// ランキングを描画
				if( lBoard.topRankers != null ){
					GUI.Label (new Rect (goal_x-goal_w/2, 15, goal_w, goal_h),"Top Ranking");
					for( int i = 0; i < lBoard.topRankers.Count; ++i) {
						GUI.Label (new Rect (goal_x-goal_w/2, 10 + goal_y*(i+1)/2, goal_w, goal_h),"Time"+ (i+1) + "."+ lBoard.topRankers[i].print());
					}
				}
				
				if (GUI.Button(new Rect(goal_x - goal_w/2, 10 + goal_y*3, goal_w, goal_h), "Back")) {
					ranking = false;
				}
			}
		}
	}
	
	// Goal到着が検知されたとき
	void OnGoal(){
		goal = true;
		NCMBObject timeClass = new NCMBObject("Time");
		timeClass["time"] = lapTime;
		timeClass.SaveAsync();
	}
}
