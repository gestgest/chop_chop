using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

/// <summary>
/// This class manages the scene loading and unloading.
/// 씬의 로드와 언로드를 담당
/// </summary>
public class SceneLoader : MonoBehaviour
{
	[SerializeField] private GameSceneSO _gameplayScene = default;
	[SerializeField] private InputReader _inputReader = default;


	//publisher
	[Header("Listening to")]
	[SerializeField] private LoadEventChannelSO _loadLocation = default;
	[SerializeField] private LoadEventChannelSO _loadMenu = default;
	[SerializeField] private LoadEventChannelSO _coldStartupLocation = default;

	[Header("Broadcasting on")]
	[SerializeField] private BoolEventChannelSO _toggleLoadingScreen = default;
	[SerializeField] private VoidEventChannelSO _onSceneReady = default; //picked up by the SpawnSystem
	[SerializeField] private FadeChannelSO _fadeRequestChannel = default;

	private AsyncOperationHandle<SceneInstance> _loadingOperationHandle;
	private AsyncOperationHandle<SceneInstance> _gameplayManagerLoadingOpHandle;

	//Parameters coming from scene loading requests
	private GameSceneSO _sceneToLoad;
	private GameSceneSO _currentlyLoadedScene;
	private bool _showLoadingScreen;

	private SceneInstance _gameplayManagerSceneInstance = new SceneInstance();
	private float _fadeDuration = .5f;
	private bool _isLoading = false; //To prevent a new loading request while already loading a new scene

	private void OnEnable()
	{
		_loadLocation.OnLoadingRequested += LoadLocation;
		_loadMenu.OnLoadingRequested += LoadMenu;
#if UNITY_EDITOR
		_coldStartupLocation.OnLoadingRequested += LocationColdStartup;
#endif
	}

	private void OnDisable()
	{
		_loadLocation.OnLoadingRequested -= LoadLocation;
		_loadMenu.OnLoadingRequested -= LoadMenu;
#if UNITY_EDITOR
		_coldStartupLocation.OnLoadingRequested -= LocationColdStartup;
#endif
	}

#if UNITY_EDITOR
	/// <summary>
	/// This special loading function is only used in the editor, when the developer presses Play in a Location scene, without passing by Initialisation.
	/// </summary>
	private void LocationColdStartup(GameSceneSO currentlyOpenedLocation, bool showLoadingScreen, bool fadeScreen)
	{
		_currentlyLoadedScene = currentlyOpenedLocation;

		if (_currentlyLoadedScene.sceneType == GameSceneSO.GameSceneType.Location)
		{
			//Gameplay managers is loaded synchronously
			_gameplayManagerLoadingOpHandle = _gameplayScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
			_gameplayManagerLoadingOpHandle.WaitForCompletion();
			_gameplayManagerSceneInstance = _gameplayManagerLoadingOpHandle.Result;

			StartGameplay();
		}
	}
#endif

	/// <summary>
	/// This function loads the location scenes passed as array parameter
	/// </summary>
	private void LoadLocation(GameSceneSO locationToLoad, bool showLoadingScreen, bool fadeScreen)
	{
		//Prevent a double-loading, for situations where the player falls in two Exit colliders in one frame
		if (_isLoading)
			return;

		_sceneToLoad = locationToLoad;
		_showLoadingScreen = showLoadingScreen;
		_isLoading = true;

		//In case we are coming from the main menu, we need to load the Gameplay manager scene first
		//한마디로 게임메니저가 null이면 게임메니저를 먼저 생성, 아니면 그냥 UnloadPreviousScene 실행
		//로딩중이면 else
		if (_gameplayManagerSceneInstance.Scene == null
			|| !_gameplayManagerSceneInstance.Scene.isLoaded)
		{

			_gameplayManagerLoadingOpHandle = _gameplayScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
			_gameplayManagerLoadingOpHandle.Completed += OnGameplayManagersLoaded;
			//씬 로딩 과정 [완료되면 그 결과물을 삽입]
			//결국 이놈도 한번의 실행으로 StartCoroutine(UnloadPreviousScene()) 실행
		}
		//애초에 이거는 잘 안나옴 [왜냐하면 게임 관련 유용한 씬이 이미 로딩됐는데 있는데 호출되는 경우임]
		else
		{
			StartCoroutine(UnloadPreviousScene());
		}
	}

	//TMI. 주소값 전달만 실행 초반에 발동되지만. 정작 이 함수 실행자체는 new game 버튼 누르고 실행된다.
	private void OnGameplayManagersLoaded(AsyncOperationHandle<SceneInstance> obj)
	{
		//씬이 완료되면 instance에게 삽입 [그 안에는 완료된 씬이 있다.]
		_gameplayManagerSceneInstance = _gameplayManagerLoadingOpHandle.Result;
		StartCoroutine(UnloadPreviousScene());
	}

	/// <summary>
	/// Prepares to load the main menu scene, first removing the Gameplay scene in case the game is coming back from gameplay to menus.
	/// </summary>
	private void LoadMenu(GameSceneSO menuToLoad, bool showLoadingScreen, bool fadeScreen)
	{
		//Prevent a double-loading, for situations where the player falls in two Exit colliders in one frame
		if (_isLoading)
			return;

		_sceneToLoad = menuToLoad;
		_showLoadingScreen = showLoadingScreen;
		_isLoading = true;

		//In case we are coming from a Location back to the main menu, we need to get rid of the persistent Gameplay manager scene
		if (_gameplayManagerSceneInstance.Scene != null
			&& _gameplayManagerSceneInstance.Scene.isLoaded)
			Addressables.UnloadSceneAsync(_gameplayManagerLoadingOpHandle, true);

		StartCoroutine(UnloadPreviousScene());
	}

	/// <summary>
	/// In both Location and Menu loading, this function takes care of removing previously loaded scenes.
	/// 이전 씬 제거
	/// </summary>
	private IEnumerator UnloadPreviousScene()
	{
		//입력 함수 비활성화
		_inputReader.DisableAllInput();
		//여기까지 함
		_fadeRequestChannel.FadeOut(_fadeDuration);

		//5초후에 스레드식 제거
		yield return new WaitForSeconds(_fadeDuration);

		//한마디로 처음에는 _currentlyLoadedScene에게 값을 안줘서 null이 뜰 수 밖에 없다.
		//왜냐하면 씬을 제거하는 함수니까. [처음에는 씬이 없다.]
		if (_currentlyLoadedScene != null)
		{
			//처음 제외하고 씬 전환을 할때에는 무조건 이 라인을 거친다 []
			//유니티 피셜, 만약 이 씬을 참조하는 애들이 없다면 isValid가 true가 된다고 한다.
			// 즉 안전한 해제법 방식이다.
			if (_currentlyLoadedScene.sceneReference.OperationHandle.IsValid())
			{
				//씬을 지워라. [unload]
				//Unload the scene through its AssetReference, i.e. through the Addressable system
				_currentlyLoadedScene.sceneReference.UnLoadScene();
			}
#if UNITY_EDITOR
			else
			{
				//Only used when, after a "cold start", the player moves to a new scene
				//Since the AsyncOperationHandle has not been used (the scene was already open in the editor),
				//the scene needs to be unloaded using regular SceneManager instead of as an Addressable
				SceneManager.UnloadSceneAsync(_currentlyLoadedScene.sceneReference.editorAsset.name);
			}
#endif
		}

		LoadNewScene();
	}

	/// <summary>
	/// Kicks off the asynchronous loading of a scene, either menu or Location.
	/// </summary>
	private void LoadNewScene()
	{

		//대충 로딩 씬 보여준다는 내용
		if (_showLoadingScreen)
		{
			_toggleLoadingScreen.RaiseEvent(true);
		}

		_loadingOperationHandle = _sceneToLoad.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true, 0);
		_loadingOperationHandle.Completed += OnNewSceneLoaded; // 씬 로드가 되면 OnNewSceneLoaded함수 추가
	}

	private void OnNewSceneLoaded(AsyncOperationHandle<SceneInstance> obj)
	{
		//Save loaded scenes (to be unloaded at next load request)
		_currentlyLoadedScene = _sceneToLoad;

		Scene s = obj.Result.Scene;
		SceneManager.SetActiveScene(s);
		LightProbes.TetrahedralizeAsync(); //씬의 모든 장면 사면체 테셀레이션 강제 비동기 업데이트

		_isLoading = false;

		//로딩 화면 끄기
		if (_showLoadingScreen)
			_toggleLoadingScreen.RaiseEvent(false);

		_fadeRequestChannel.FadeIn(_fadeDuration);

		StartGameplay();
	}

	private void StartGameplay()
	{
		_onSceneReady.RaiseEvent(); //Spawn system will spawn the PigChef in a gameplay scene
	}

	private void ExitGame()
	{
		Application.Quit();
		Debug.Log("Exit!");
	}
}
