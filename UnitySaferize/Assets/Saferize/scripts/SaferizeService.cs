using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using SaferizeSDK;

public class SaferizeService : MonoBehaviour
{
    private SaferizeData _saferizeData;
    private static SaferizeService instance;

    //For initializing the SDK
    private Saferize _saferize;
    [TextArea(10, 100)]
    public string saferizePrivateKey;
    public string saferizeUrl;
    public string saferizeWebsocketUrl;
    public string saferizeApiKey;

    //Event Handlers

    // Parent Events
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

    public static SaferizeService Instance()
    {
        if (!instance)
        {
            instance = FindObjectOfType(typeof(SaferizeService)) as SaferizeService;
        }
        return instance;
    }

    void Awake()
    {

        _saferize = new Saferize(saferizePrivateKey.Trim(), saferizeUrl.Trim(), saferizeWebsocketUrl.Trim(), saferizeApiKey.Trim());

        if (instance == null)
        {
            instance = this;
            RegisterEventHandlers();
            loadSaferizeDataIntoMemory();
            createAppUserSession();
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        while (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();
        }
    }

    void OnApplicationPause(bool applicationWentIntoBackground)
    {
        if (applicationWentIntoBackground == false)
        {
            createAppUserSession();
        }
        else
        {
            _saferize.DisconnectUser();
        }
    }

    private void loadSaferizeDataIntoMemory()
    {
        var saferizeFileName = Application.persistentDataPath + "/saferize.dat";

        if (File.Exists(saferizeFileName))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream filestream = File.Open(saferizeFileName, FileMode.Open))
            {
                _saferizeData = binaryFormatter.Deserialize(filestream) as SaferizeData;
                filestream.Dispose();
            }
        }
    }

    private void createAppUserSession()
    {
        if (_saferizeData != null)
        {
            AppUsageSession appUsageSession = _saferize.ConnectUser(_saferizeData.token);

            _saferizeData.approval = appUsageSession.Approval;
            _saferizeData.appFeatures = appUsageSession.Features;

            _saferizeData.lastLogin = DateTime.UtcNow;
            SaveFile(_saferizeData);
        }
    }

    private void SaveFile(SaferizeData data)
    {
        var saferizeFileName = Application.persistentDataPath + "/saferize.dat";

        using (FileStream filestream = File.Open(saferizeFileName, FileMode.Create))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(filestream, data);

            filestream.Dispose();
        }
    }


    private void RegisterEventHandlers()
    {
        _saferize.OnOfflineStart += delegate {
            ExecuteOnMainThread.Enqueue(() =>
            {
                if (OpenEventPanel != null) Destroy(OpenEventPanel);
                OpenEventPanel = Instantiate(PINInputPanel);
                OpenEventPanel.gameObject.GetComponent<ISaferizeOnOfflineStart>().OnOfflineStart();
            });
        };

        _saferize.OnOfflineEnd += delegate {

            if (OpenEventPanel != null && OpenEventPanel.GetType() == PINInputPanel.GetType())
            {
                OpenEventPanel.gameObject.GetComponent<ISaferizeOnOfflineEnd>().OnOfflineEnd();
            }
        };

        _saferize.OnPause += delegate {
            ExecuteOnMainThread.Enqueue(() => {
                if (OpenEventPanel != null) Destroy(OpenEventPanel);
                OpenEventPanel = Instantiate(OnPause);
                OpenEventPanel.gameObject.GetComponent<ISaferizeOnPause>().OnPause();
            });
        };

        _saferize.OnRevoke += delegate {
            ExecuteOnMainThread.Enqueue(() => {
                if (OpenEventPanel != null) Destroy(OpenEventPanel);
                OpenEventPanel = Instantiate(OnRevoke);
                OpenEventPanel.gameObject.GetComponent<ISaferizeOnRevoke>().OnRevoke();
            });
        };

        _saferize.OnResume += delegate {
            ExecuteOnMainThread.Enqueue(() => {
                if (OpenEventPanel != null) Destroy(OpenEventPanel);
                OpenEventPanel = Instantiate(OnResume);
                OpenEventPanel.gameObject.GetComponent<ISaferizeOnResume>().OnResume();
            });
        };

        _saferize.OnTimeIsUp += delegate {
            ExecuteOnMainThread.Enqueue(() => {
                if (OpenEventPanel != null) Destroy(OpenEventPanel);
                OpenEventPanel = Instantiate(OnTimeIsUp);
                OpenEventPanel.gameObject.GetComponent<ISaferizeOnTimeIsUp>().OnTimeIsUp();
            });
        };

        _saferize.OnPINChange += ((string pinHash) =>
        {
            _saferizeData.PINhash = pinHash;
            SaveFile(_saferizeData);
        });

        _saferize.OnAppFeaturesChange += ((AppFeature[] updatedAppFeatures) => {
            ExecuteOnMainThread.Enqueue(() =>
            {
                _saferizeData.appFeatures = updatedAppFeatures;
                SaveFile(_saferizeData);
            });
        });
    }

    public AppFeature.AppFeatureStatus CheckAppFeatureState(AppFeature.AppFeatureNameEnum appFeature)
    {
		if(_saferizeData == null || _saferizeData.appFeatures == null) return AppFeature.AppFeatureStatus.ENABLED;
        
		AppFeature[] appFeatures = _saferizeData.appFeatures;

        foreach (AppFeature feature in appFeatures)
        {
            if (feature.appFeature == appFeature)
            {
                return feature.enabled ? AppFeature.AppFeatureStatus.ENABLED : AppFeature.AppFeatureStatus.DISABLED;
            }
        }

        return AppFeature.AppFeatureStatus.ENABLED;
    }

    public void SignUp(string parentEmail, string token)
    {

        try
        {
            Approval approval = _saferize.Signup(parentEmail, token);

            if (approval == null)
            {
                return;
            }
            else
            {
                SaferizeData data = new SaferizeData();

                data.lastLogin = DateTime.UtcNow;
                data.token = approval.AppUser.Token;
                data.parentEmail = approval.ParentEmail;
                data.approval = approval;

                _saferizeData = data;

                SaveFile(data);
            }
        }
        catch (Exception e)
        {
            Debug.Log("error for sign up was: " + e);
        }
    }

    public void OpenSaferizeParents()
    {

        if (_saferizeData != null)
        {
            OpenCurrentUserPanel();
        }
        else
        {
            OpenSignUpPanel();
        }
    }

    public void OpenCurrentUserPanel()
    {
        if (OpenParentInteractionPanel != null) Destroy(OpenParentInteractionPanel);
        OpenParentInteractionPanel = Instantiate(CurrentUserPanel);
        Time.timeScale = 0;
    }

    public void OpenSignUpPanel()
    {
        if (OpenParentInteractionPanel != null) Destroy(OpenParentInteractionPanel);
        OpenParentInteractionPanel = Instantiate(SignUpPanel);
        Time.timeScale = 0;
    }

    public void CloseSaferizeParents()
    {
        Destroy(OpenParentInteractionPanel);
        Time.timeScale = 1;
    }

    public void ClearSaferizeData()
    {
        var saferizeFileName = Application.persistentDataPath + "/saferize.dat";

        if (File.Exists(saferizeFileName))
        {
            File.Delete(saferizeFileName);
            Debug.Log("deleted saferize data file");
        }
    }

    public SaferizeData getSaferizeData()
    {
        return _saferizeData;
    }
}
