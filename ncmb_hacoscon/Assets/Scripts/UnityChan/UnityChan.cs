using UnityEngine;
using System.Collections;

public class UnityChan : MonoBehaviour {
	private GUIStyle labelStyle;
	float initpositionx = -9;
	float initpositiony = 0.3f;
	float initpositionz = -21;
	Quaternion gyro;
	bool goal = false;
	private Vector3 acceleration;
	int count = 0;
	
	void Update () {
		if (goal == false) {
			Input.gyro.enabled = true;
			if (Input.gyro.enabled) {
				Quaternion gyro = Input.gyro.attitude;
				Quaternion action_gyro = Quaternion.Euler (90, 0, 0) * (new Quaternion (-gyro.x, -gyro.y, gyro.z, gyro.w));
				Vector3 p = new Vector3 (0, 0, -50f);
				GetComponent<Rigidbody> ().AddForce (p);
				Vector3 c = new Vector3 (transform.position.x - 0.1f*action_gyro.z, initpositiony, transform.position.z);
				transform.position = c;
				Vector3 r = new Vector3 (0, 180 + action_gyro.z * 180 / Mathf.PI, 0);
				transform.rotation = Quaternion.Euler (r);
			}else{
				//シュミレーター上で動かすための、キーボードの入力を受け付ける
				if(Input.GetKey ("up")){
					Vector3 up = new Vector3 (transform.position.x, transform.position.y, transform.position.z - 0.03f);
					transform.position = up;
				}
				
				if(Input.GetKey ("down")){
					Vector3 down = new Vector3 (transform.position.x, transform.position.y, transform.position.z + 0.03f);
					transform.position = down;
				}
				
				if(Input.GetKey ("right")){
					Vector3 right = new Vector3 (transform.position.x - 0.03f, transform.position.y, transform.position.z);
					transform.position = right;
				}
				
				if(Input.GetKey ("left")){
					Vector3 left = new Vector3 (transform.position.x + 0.03f, transform.position.y, transform.position.z);
					transform.position = left;
				}
				
			}
		} else {
			//ゴール時にUnityちゃんの回転を止める
			Vector3 R = new Vector3 (0, 180, 0);
			transform.rotation = Quaternion.Euler (R);
		}
	}
	
	//Goal接触時にアニメーションを切り替える
	//Unityちゃんの挙動をゴールモードにする
	void OnGoal() {
		GetComponent<Animator>().SetTrigger("GOAL");
		goal = true;
	}
	
	
}