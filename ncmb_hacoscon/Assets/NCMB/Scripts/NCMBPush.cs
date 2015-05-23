/*******
 Copyright 2014 NIFTY Corporation All Rights Reserved.
 
 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at
 
 http://www.apache.org/licenses/LICENSE-2.0
 
 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
 **********/

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

using System.Collections.Generic;
using MiniJSON;

namespace NCMB
{
	/// <summary>
	/// プッシュ操作を扱います。
	/// </summary>
	public class NCMBPush
	{
		private IDictionary<string, object> pushData = new Dictionary<string, object> ();
		
		#if UNITY_ANDROID
		static AndroidJavaClass m_AJClass;
		#elif UNITY_IOS
		[DllImport ("__Internal")]
		private static extern void initialize(string applicationId, string clientId);
		[DllImport ("__Internal")]
		private static extern void registerNotification();
		[DllImport ("__Internal")]
		private static extern void registerNotificationWithLocation();
		[DllImport ("__Internal")]
		private static extern void sendPush(string json, string message, int delayByMilliseconds, bool dialog);
		[DllImport ("__Internal")]
		private static extern void clearAll();
		#endif
		
		
		/*** 初期化 ***/
		
		static NCMBPush()
		{
			#if UNITY_ANDROID && !UNITY_EDITOR
			m_AJClass = new AndroidJavaClass("com.nifty.cloud.mb.NCMBPushProxy");
			#endif
		}
		
		/// <summary>
		/// NCMBクラスでプッシュの初期化を行う
		/// </summary>
		/// <param name="applicationId">アプリケーションキー</param>
		/// <param name="clientId">クライアントキー</param>
		internal static void Init(string applicationId, string clientId)
		{
			#if UNITY_EDITOR
			#elif UNITY_ANDROID
			m_AJClass.CallStatic("initialize", applicationId, clientId);
			#elif UNITY_IOS
			initialize(applicationId, clientId);
			#endif
		}
		
		/// <summary>
		/// コンストラクター。<br/>
		///プッシュの作成を行います。
		/// </summary>
		public NCMBPush ()
		{
			//プッシュ設定を保持する
			this.pushData = new Dictionary<string, object> ();
			#if UNITY_ANDROID && !UNITY_EDITOR
			m_AJClass = new AndroidJavaClass("com.nifty.cloud.mb.NCMBPushProxy");
			#endif
		}
		
		#if UNITY_ANDROID
		public static void Register(string senderId)
		{
			if (!string.IsNullOrEmpty(senderId))
			{
				#if !UNITY_EDITOR
				m_AJClass.CallStatic("registerNotification", senderId);
				#endif
			}
		}
		#elif UNITY_IOS
		public static void Register()
		{
			#if !UNITY_EDITOR
			registerNotification();
			#endif
		}
		#endif
		
		#if UNITY_ANDROID
		public static void RegisterWithLocation(string senderId)
		{
			if (!string.IsNullOrEmpty(senderId))
			{
				#if !UNITY_EDITOR
				m_AJClass.CallStatic("registerNotificationWithLocation", senderId);
				#endif
			}
		}
		#elif UNITY_IOS
		/// <summary>
		/// Register for receiving remote notifications (with current location).
		/// </summary>
		internal static void RegisterWithLocation()
		{
			#if !UNITY_EDITOR
			registerNotificationWithLocation();
			#endif
		}
		#endif
		
		// <summary>
		// Subscribe the specified channel.
		// </summary>
		// <param name="channel">Channel.</param>
		//public static void Subscribe(string channel)
		//{
		//    if (!NCMBManager.Inited)
		//        return;
		
		//    #if UNITY_EDITOR
		//    #elif UNITY_ANDROID
		//    m_AJClass.CallStatic("subscribe", channel);
		//    #endif
		//}
		
		
		
		/*** Push設定 ***/
		
		/// <summary>
		/// メッセージの取得、または設定を行います。
		/// </summary>
		public string Message {
			get
			{
				string message = null;
				if (pushData.ContainsKey("message")) {
					message = (string)this.pushData ["message"];
				};
				return message;
			}
			set {this.pushData ["message"] = value;}
		}
		
		/// <summary>
		/// 配信時間(ミリ秒)の取得、または設定を行います。
		/// </summary>
		public int DelayByMilliseconds {
			get 
			{
				int delayByMilliseconds = 0;
				if (pushData.ContainsKey("delayByMilliseconds")) {
					delayByMilliseconds = (int)this.pushData ["delayByMilliseconds"];
				};
				return delayByMilliseconds;
			}
			set {this.pushData ["delayByMilliseconds"] = value;}
		}
		
		/// <summary>
		/// タイトルの取得、または設定を行います(Androidのみ)。
		/// </summary>
		public string Title {
			get
			{
				string title = null;
				if (pushData.ContainsKey("title")) {
					title = (string)this.pushData ["title"];
				};
				return title;
			}
			set {this.pushData ["title"] = value;}
		}
		
		/// <summary>
		/// iOS端末へ送信フラグの取得、または設定を行います。<br/>
		/// target = [ios, android] または target = [ios]の時にfalseを返します。
		/// </summary>
		public bool PushToIOS {
			get
			{
				bool pushToIOSFlag = false;
				if (pushData.ContainsKey("target")) {
					string[] target = (string[])this.pushData ["target"];
					foreach(string value in target){
						if(value == "ios"){
							pushToIOSFlag = true;
						}
					}
				};
				return pushToIOSFlag;
			}
			set 
			{
				bool pushToAndroidFlag = this.PushToAndroid;
				if(value == true && pushToAndroidFlag == false){
					this.pushData ["target"] = new string[]{"ios"};
				}else if(value == false && pushToAndroidFlag == true){
					this.pushData ["target"] = new string[]{"android"};
				}else{
					//[true,true] or [false,false]
					//this.pushData ["target"] = new string[]{"ios","android"};
					if (pushData.ContainsKey ("target")) {
						pushData.Remove ("target");
					}
				}
			}
		}
		
		/// <summary>
		/// Android端末へ送信フラグの取得、または設定を行います。<br/>
		/// target = [ios, android] または target = [android]の時にfalseを返します。
		/// </summary>
		public bool PushToAndroid {
			get
			{
				bool PushToAndroidFlag = false;
				if (pushData.ContainsKey("target")) {
					string[] target = (string[])this.pushData ["target"];
					foreach(string value in target){
						if(value == "android"){
							PushToAndroidFlag = true;
						}
					}
				};
				return PushToAndroidFlag;
			}
			set 
			{
				bool pushToIOSFlag = this.PushToIOS;
				if(value == true && pushToIOSFlag == false){
					this.pushData ["target"] = new string[]{"android"};
				}else if(value == false && pushToIOSFlag == true){
					this.pushData ["target"] = new string[]{"ios"};
				}else{
					//[true,true] or [false,false]
					//this.pushData ["target"] = new string[]{"ios","android"};
					if (pushData.ContainsKey ("target")) {
						pushData.Remove ("target");
					}
				}
			}
		}
		
		/*
		/// <summary>
		/// アクションの取得、または設定を行います(Androidのみ)。
		/// </summary>
		public string Action {
			get
			{
				string action = null;
				if (pushData.ContainsKey("action")) {
					action = (string)this.pushData ["action"];
				};
				return action;
			}
			set {this.pushData ["action"] = value;}
		}

		/// <summary>
		/// バッジ数の取得、または設定を行います(iOSのみ)。
		/// </summary>
		public int? Badge {
			get
			{
				int? Badge = null;
				if (pushData.ContainsKey("badgeSetting")) {
					Badge = (int)this.pushData ["badgeSetting"];
				};
				return Badge;
			}
			set {this.pushData ["badgeSetting"] = value;}
		}
		
		/// <summary>
		/// バッジ増加フラグの取得、または設定を行います。
		/// </summary>
		public bool BadgeIncrementFlag {
			get
			{
				bool badgeIncrementFlag = true;
				if (pushData.ContainsKey("badge")) {
					badgeIncrementFlag = (bool)this.pushData ["badge"];
				};
				return badgeIncrementFlag;
			}
			set {this.pushData ["badge"] = value;}
		}

		/// <summary>
		/// リッチプッシュURLの取得、または設定を行います。
		/// </summary>
		public string RichUrl {
			get
			{
				string title = null;
				if (pushData.ContainsKey("richUrl")) {
					title = (string)this.pushData ["richUrl"];
				};
				return title;
			}
			set {this.pushData ["richUrl"] = value;}
		}

		/// <summary>
		/// ダイアログの取得、または設定を行います。
		/// </summary>
		public bool Dialog {
			get
			{
				bool dialog = false;
				if (pushData.ContainsKey("dialog")) {
					dialog = (bool)this.pushData ["dialog"];
				};
				return dialog;
			}
			set {this.pushData ["dialog"] = value;}
		}
		*/
		
		
		/*** Push送信 ***/
		
		/// <summary>
		/// プッシュの送信を行います。
		/// </summary>
		public void SendPush()
		{
			//本文設定
			string message = this.Message;
			if(message != null){
				pushData.Remove ("message");
			}
			//配信時間設定
			int delayByMilliseconds = this.DelayByMilliseconds;
			if (pushData.ContainsKey ("delayByMilliseconds")) {
				pushData.Remove ("delayByMilliseconds");
			}
			
			//その他プッシュ設定
			string json = Json.Serialize (this.pushData);
			SendPush(json, message, delayByMilliseconds );
			
			//プッシュ設定クリア
			this.pushData = new Dictionary<string, object> ();
		}
		
		/// <summary>
		/// iOSとAndroidのネイティブコードにデータを送信する
		/// </summary>
		/// <param name="json">JSONData</param>
		/// <param name="message">メッセージ</param>
		/// <param name="delayByMilliseconds">配信時間</param>
		/// <param name="dialog">ダイアログ</param>
		private static void SendPush(string json, string message, int delayByMilliseconds = 0, bool dialog = false)
		{
			//if (!NCMBManager.Inited)
			//	return;
			
			#if UNITY_EDITOR
			#elif UNITY_ANDROID
			m_AJClass.CallStatic("sendPush", json, message, delayByMilliseconds, dialog);
			#elif UNITY_IOS
			sendPush(json, message, delayByMilliseconds, dialog);
			#endif
		}
		
		#if UNITY_IOS
		// Clears all notifications.
		public static void ClearAll()
		{
			NotificationServices.ClearRemoteNotifications();
			
			#if !UNITY_EDITOR
			clearAll();
			#endif
		}
		#endif
	}
}
