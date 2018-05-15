using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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