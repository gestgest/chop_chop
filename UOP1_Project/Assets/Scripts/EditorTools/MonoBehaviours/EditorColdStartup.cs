using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

//Allows a "cold start" in the editor, when pressing Play and not passing from the Initialisation scene.
/// <summary>
/// Initialisation씬에서 시작해서 한게 아닌 자체적으로 씬에서 시작한 경우를 방지하기 위한 클래스
/// </summary>
public class EditorColdStartup : MonoBehaviour
{
#if UNITY_EDITOR
	[SerializeField] private GameSceneSO _thisSceneSO = default;
	[SerializeField] private GameSceneSO _persistentManagersSO = default;
	[SerializeField] private AssetReference _notifyColdStartupChannel = default;
	[SerializeField] private VoidEventChannelSO _onSceneReadyChannel = default;
	[SerializeField] private PathStorageSO _pathStorage = default;
	[SerializeField] private SaveSystem _saveSystem = default;

	private bool isColdStart = false;
	private void Awake()
	{
		//presistentManager씬이 없으면
		if (!SceneManager.GetSceneByName(_persistentManagersSO.sceneReference.editorAsset.name).isLoaded)
		{
			isColdStart = true; //없으니까 씬 로드해라 => Start

			//Reset the path taken, so the character will spawn in this location's default spawn point
			//리셋시켜서 기본 생성 지점에 생성
			_pathStorage.lastPathTaken = null;
		}
		CreateSaveFileIfNotPresent();
	}

	private void Start()
	{
		//presistentManager씬이 없으면 비동기 씬 로드
		if (isColdStart)
		{
			//씬 로딩이 완료되면 씬을 실행해라
			_persistentManagersSO.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true).Completed += LoadEventChannel;

		}
		CreateSaveFileIfNotPresent(); //저장시스템
	}
	private void CreateSaveFileIfNotPresent()
	{
		//파일이 없으면 만들어라
		if (_saveSystem != null && !_saveSystem.LoadSaveDataFromDisk())
		{
			_saveSystem.SetNewGameData();
		}
	}
	private void LoadEventChannel(AsyncOperationHandle<SceneInstance> obj)
	{
		_notifyColdStartupChannel.LoadAssetAsync<LoadEventChannelSO>().Completed += OnNotifyChannelLoaded;
	}

	private void OnNotifyChannelLoaded(AsyncOperationHandle<LoadEventChannelSO> obj)
	{
		if (_thisSceneSO != null)
		{
			obj.Result.RaiseEvent(_thisSceneSO);
		}
		else
		{
			//Raise a fake scene ready event, so the player is spawned
			//가짜 장면을 만들어서 플레이어를 생성한다.
			_onSceneReadyChannel.RaiseEvent();
			//When this happens, the player won't be able to move between scenes because the SceneLoader has no conception of which scene we are in
			//이 이벤트가 발생하면 SceneLoader가 어디에 있는지 알 수 없다.
			//그래서 플레이어는 장면을 이동할 수 없다
		}
	}


#endif
}
