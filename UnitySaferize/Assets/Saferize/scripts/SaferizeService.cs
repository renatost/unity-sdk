using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using SaferizeSDK;

public class SaferizeService : MonoBehaviour {
	private SaferizeData _saferizeData;
	private static SaferizeService instance;
	private string saferizeFileName;

	//For initializing the SDK
	private Saferize _saferize;
	[TextArea(10, 100)]
	public string saferizePrivateKey;
	public string saferizeUrl;
	public string saferizeWebsocketUrl;
	public string saferizeApiKey;

	//Events
	public GameObject OnPause;
	public GameObject OnTimeIsUp;
	public GameObject OnRevoke;
	public GameObject OnResume;
	private GameObject OpenEventPanel;
    
	//Parent Interaction
	public GameObject SignUpPanel;
	public GameObject CurrentUserPanel;
	public GameObject PINInputPanel;
	private GameObject OpenParentInteractionPanel;

	public readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

	public static SaferizeService Instance() {
		if (!instance) {
			instance = FindObjectOfType (typeof(SaferizeService)) as SaferizeService;
		}
		return instance;
	}

	void Awake () {
		_saferize = new Saferize (saferizePrivateKey.Trim (), saferizeUrl.Trim (), saferizeWebsocketUrl.Trim (), saferizeApiKey.Trim ());
		saferizeFileName = Application.persistentDataPath + "/saferize.dat";

		RegisterEventHandlers ();
		createAppUserSession ();
	}

	void Update()
	{
		while (ExecuteOnMainThread.Count > 0)
		{
			ExecuteOnMainThread.Dequeue().Invoke();
		}
	}
		
	void OnApplicationPause(bool applicationWentIntoBackground){
		if (applicationWentIntoBackground == false) {
			createAppUserSession ();
		}
	}

	private void createAppUserSession(){
		if (File.Exists(saferizeFileName)) {
			BinaryFormatter binaryFormatter = new BinaryFormatter();

			using (FileStream filestream = File.Open (saferizeFileName, FileMode.Open)) {
				_saferizeData = binaryFormatter.Deserialize (filestream) as SaferizeData;
				_saferizeData.lastLogin = DateTime.UtcNow;
				filestream.Dispose();

				SaveFile(_saferizeData);
				_saferize.ConnectUser (_saferizeData.token);
			}
		}
	}

	private void SaveFile(SaferizeData data) {
		using (FileStream filestream = File.Open (saferizeFileName, FileMode.Create)) {
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			binaryFormatter.Serialize (filestream, data);

			filestream.Dispose ();
		}
	}

	private void RegisterEventHandlers(){
		_saferize.OnOfflineStart += delegate {
			ExecuteOnMainThread.Enqueue(() =>
			{
				if (OpenEventPanel != null) Destroy(OpenEventPanel);
				OpenEventPanel = Instantiate(PINInputPanel);
				OpenEventPanel.gameObject.GetComponent<ISaferizeOnOfflineStart>().OnOfflineStart();
			});
		};

		_saferize.OnOfflineEnd += delegate {
			if (OpenEventPanel.GetType() == PINInputPanel.GetType()){
				OpenEventPanel.gameObject.GetComponent<ISaferizeOnOfflineEnd>().OnOfflineEnd();
			}
		};

		_saferize.OnPause += delegate {
			ExecuteOnMainThread.Enqueue(() => {
				if(OpenEventPanel != null) Destroy(OpenEventPanel);
				OpenEventPanel = Instantiate(OnPause);
				OpenEventPanel.gameObject.GetComponent<ISaferizeOnPause>().OnPause();
			});
		};

		_saferize.OnRevoke += delegate {
			ExecuteOnMainThread.Enqueue (() => {
				if(OpenEventPanel != null) Destroy(OpenEventPanel);
				OpenEventPanel = Instantiate(OnRevoke);
				OpenEventPanel.gameObject.GetComponent<ISaferizeOnRevoke>().OnRevoke();
			});
		};

		_saferize.OnResume += delegate {
			ExecuteOnMainThread.Enqueue (() => {
				if(OpenEventPanel != null) Destroy(OpenEventPanel);
				OpenEventPanel = Instantiate(OnResume);
				OpenEventPanel.gameObject.GetComponent<ISaferizeOnResume>().OnResume();
			});
		};

		_saferize.OnTimeIsUp += delegate {
			ExecuteOnMainThread.Enqueue(() => {
				if(OpenEventPanel != null) Destroy(OpenEventPanel);
				OpenEventPanel = Instantiate(OnTimeIsUp);
				OpenEventPanel.gameObject.GetComponent<ISaferizeOnTimeIsUp>().OnTimeIsUp();
			});
		};
	}

	public void SignUp (string parentEmail, string token) {
		Approval approval = _saferize.Signup (parentEmail, token);

		SaferizeData data = new SaferizeData ();

		data.lastLogin = DateTime.UtcNow;
		data.token = approval.AppUser.Token;
		data.parentEmail = approval.ParentEmail;

		_saferizeData = data;
		SaveFile (data);
		_saferize.ConnectUser (token);
	}

	public void OpenSaferizeParents(){

		if (_saferizeData != null) {
			OpenParentInteractionPanel = Instantiate (CurrentUserPanel);
		} else {
			OpenParentInteractionPanel = Instantiate (SignUpPanel);
		}

		Time.timeScale = 0;
	}

	public void CloseSaferizeParents(){
		Destroy (OpenParentInteractionPanel);
		Time.timeScale = 1;
	}

	public void ClearSaferizeData(){
		if (File.Exists (saferizeFileName)) {
			File.Delete (saferizeFileName);
			Debug.Log ("deleted saferize data file");
		}
	}

	public SaferizeData getSaferizeData()
	{
		return _saferizeData;
	}
}
