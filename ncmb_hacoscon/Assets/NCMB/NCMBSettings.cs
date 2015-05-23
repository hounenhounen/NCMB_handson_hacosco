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

using System.Collections;
using System;
using UnityEngine;
using NCMB.Internal;

namespace NCMB
{
	/// <summary>
	/// 初期設定を扱います。
	/// </summary>
	public class NCMBSettings : MonoBehaviour
	{
		//アプリケションキー
		private static string _applicationKey = "";
		//クライアントキー
		private static string _clientKey = "";
		//ANDROID SENDER キー
		private static string _androidSenderId = "";
		//シグネチャチェックフラグ
		internal static bool _responseValidationFlag = false;
		//初回のみ実行フラグ
		private static bool _isInitialized = false;

		//static NG
		[SerializeField]
		internal string
		applicationKey = "";
		[SerializeField]
		internal string
		clientKey = "";
		[SerializeField]
		internal bool
		usePush = false;
		[SerializeField]
		internal string
		androidSenderId = "";
		//[SerializeField]
		//internal bool
		//getLocation = false;
		[SerializeField]
		internal bool
		responseValidation = false;

		//Current user
		private static string _currentUser = null;
		internal static string filePath;

		/// <summary>
		/// Current userの取得、または設定を行います。 
		/// </summary>
		internal static string CurrentUser {
			get {
				return _currentUser;
			}
			set {
				_currentUser = value;
			}
		}

		/// <summary>
		/// アプリケションキーの取得、または設定を行います。 
		/// </summary>
		public static string ApplicationKey {
			get {
				return _applicationKey;
			}
			set {
				_applicationKey = value;
			}
		}

		/// <summary>
		/// クライアントキーの取得、または設定を行います。 
		/// </summary>
		public static string ClientKey {
			get {
				return _clientKey;
			}
			set {
				_clientKey = value;
			}
		}

		/// <summary>
		/// Android Sender Id
		/// </summary>
		public static string AndroidSenderId {
			get {
				return _androidSenderId;
			}
			set {
				_androidSenderId = value;
			}
		}

		/// <summary>
		/// コンストラクター
		/// </summary>
		public NCMBSettings ()
		{
		}

		/// <summary>
		/// 初期設定を行います。
		/// </summary>
		/// <param name="applicationKey">アプリケーションキー</param>
		/// <param name="clientKey">クライアントキー</param>
		private static void Initialize (String applicationKey, String clientKey)
		{
			// アプリケーションキーを設定
			_applicationKey = applicationKey;
			// クライアントキーを設定
			_clientKey = clientKey;
			// Native Initialize
			NCMBPush.Init(applicationKey, clientKey);
		}

		/// <summary>
		/// Registers the push.
		/// </summary>
		/// <param name="usePush">If set to <c>true</c> use push.</param>
		/// <param name="androidSenderId">Android sender identifier.</param>
		/// <param name="getLocation">If set to <c>true</c> get location.</param>
		private static void RegisterPush(bool usePush, String androidSenderId, bool getLocation = false)
		{
			// Sender idを設定
			_androidSenderId = androidSenderId;

			// Register
			if (usePush)
			{
				if (!getLocation)
				{
					#if UNITY_ANDROID
					NCMBPush.Register(androidSenderId);
					#elif UNITY_IOS
					NCMBPush.Register();
					#endif
				}
				else
				{
					#if UNITY_ANDROID
					NCMBPush.RegisterWithLocation(androidSenderId);
					#elif UNITY_IOS
					NCMBPush.RegisterWithLocation();
					#endif
				}
			}
		}

		/// <summary>
		/// レスポンスが改ざんされていないか判定する機能を有効にします。<br/>
		/// デフォルトは無効です。
		/// </summary>
		/// <param name="checkFlag">true:有効　false:無効</param>
		public static void EnableResponseValidation (bool checkFlag)
		{
			_responseValidationFlag = checkFlag;
		}

		/// <summary>
		/// 初期設定を行います。
		/// </summary>
		public virtual void Awake ()
		{
			if (!NCMBSettings._isInitialized) {
				NCMBSettings._isInitialized = true;
				_responseValidationFlag = responseValidation;
				DontDestroyOnLoad (base.gameObject);
				NCMBSettings.Initialize (this.applicationKey, this.clientKey);
				//NCMBSettings.RegisterPush(this.usePush, this.androidSenderId, this.getLocation);
				NCMBSettings.RegisterPush(this.usePush, this.androidSenderId, false);
				filePath = Application.persistentDataPath;
				base.StartCoroutine (Platform.RunLoop ());
			}
		}
	}
}