using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class is used for scene-loading events.
/// Takes a GameSceneSO of the location or menu that needs to be loaded, and a bool to specify if a loading screen needs to display.
/// </summary>
[CreateAssetMenu(menuName = "Events/Load Event Channel")]
public class LoadEventChannelSO : DescriptionBaseSO
{
	public UnityAction<GameSceneSO, bool, bool> OnLoadingRequested;

	//로딩 함수
	public void RaiseEvent(GameSceneSO locationToLoad, bool showLoadingScreen = false, bool fadeScreen = false)
	{
		if (OnLoadingRequested != null)
		{
			//그러니까 대충 OnLoadingRequested = 씬 실행 함수;를 public으로 함 [awake?]
			//
			//씬이 하나 로딩될때마다 이 함수가 실행
			OnLoadingRequested.Invoke(locationToLoad, showLoadingScreen, fadeScreen);
			//Debug.Log(locationToLoad);
		}
		else
		{
			Debug.LogWarning("A Scene loading was requested, but nobody picked it up. " +
				"Check why there is no SceneLoader already present, " +
				"and make sure it's listening on this Load Event channel.");
		}
	}
}
