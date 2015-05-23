using UnityEngine;
using System.Collections;

public class StartCameraController : MonoBehaviour {
	private GUIStyle labelStyle;
	public static Quaternion ini_gyro;
	void Start()
	{
		this.labelStyle = new GUIStyle();
		this.labelStyle.fontSize = Screen.height / 22;
		this.labelStyle.normal.textColor = Color.white;
		
	}
	
	void Update () {
		Input.gyro.enabled = true;
		if (Input.gyro.enabled)
		{
			ini_gyro = Input.gyro.attitude;
			this.transform.localRotation = Quaternion.Euler(90, 0, 0) * (new Quaternion(-ini_gyro.x,-ini_gyro.y, ini_gyro.z, ini_gyro.w)); 
		}
	}
	//ジャイロセンサの値を表示するプログラム
	void OnGUI()
	{
		if (Input.gyro.enabled)
		{
			ini_gyro = Quaternion.Euler(90, 0, 0) * (new Quaternion(-ini_gyro.x, -ini_gyro.y, ini_gyro.z, ini_gyro.w));
			float x = Screen.width / 10;
			float y = 0;
			float w = Screen.width * 8 / 10;
			float h = Screen.height / 20;
			
			for (int i = 0; i < 3; i++)
			{
				y = Screen.height / 10 + h * i;
				string text = string.Empty;
				
				switch (i)
				{
				case 0://X
					text = string.Format("gyro-X:{0}", ini_gyro.x);
					break;
				case 1://Y
					text = string.Format("gyro-Y:{0}", ini_gyro.y);
					break;
				case 2://Z
					text = string.Format("gyro-Z:{0}", ini_gyro.z);
					break;
				default:
					throw new System.InvalidOperationException();
				}
				GUI.Label(new Rect(x, y, w, h), text, this.labelStyle);
			}
			
		}
		//スタートからワールドへのシーン遷移を行う
		if (GUI.Button(new Rect(Screen.width / 2 - Screen.width / 10, Screen.height / 2, Screen.width / 5, Screen.height / 10), "Start")) {
			Application.LoadLevel ("world");
		}
	}
}
