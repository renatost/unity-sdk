using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaferizeSDK;
// key parent events
public interface ISaferizeOnPause
{
	void OnPause();
}

public interface ISaferizeOnResume
{
	void OnResume();
}

public interface ISaferizeOnTimeIsUp
{
	void OnTimeIsUp();
}

public interface ISaferizeOnRevoke
{
	void OnRevoke();
}

public interface ISaferizeOnConnect
{
	void OnConnect();
}

// Online/offline trigger
public interface ISaferizeOnOfflineStart
{
	void OnOfflineStart();
}

public interface ISaferizeOnOfflineEnd
{
	void OnOfflineEnd();
}