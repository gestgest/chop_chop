using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is responsible for starting the game by loading the persistent managers scene 
/// and raising the event to load the Main Menu
/// </summary>

public class InitializationLoader : MonoBehaviour
{
	
	[SerializeField] private GameSceneSO _managersScene = default;

	//맨 처음 게임 메뉴 Scene
	[SerializeField] private GameSceneSO _menuToLoad = default;

	[Header("Broadcasting on")]
	[SerializeField] private AssetReference _menuLoadChannel = default;

	private void Start()
	{
		_managersScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true).Completed += LoadEventChannel;
		//Additive는 로드된 씬에 추가로 넣는 느낌
		//즉 기존 init에 sceneReference를 추가
		//true는 비동기 완료 되면 바로 scene 실행
	}

	// 즉 매니저 실행후[start] => 로드 메뉴 [LoadEventChannelSO 실행]=> 0 Scene 실행 [아마 init]
	private void LoadEventChannel(AsyncOperationHandle<SceneInstance> obj)
	{
		_menuLoadChannel.LoadAssetAsync<LoadEventChannelSO>().Completed += LoadMainMenu;
	}

	//메인 메뉴 실행
	private void LoadMainMenu(AsyncOperationHandle<LoadEventChannelSO> obj)
	{
		//Result는 LoadEventChannelSO다.
		obj.Result.RaiseEvent(_menuToLoad, true);

		SceneManager.UnloadSceneAsync(0); //init 씬 제거
	}

	//시작하면 = LoadEventChannel = LoadMainMenu
	//밑에 두개 함수는 콜백 함수 넘기는 느낌

	//TMI로 매니저가 menu 이벤트를 넣고 돌리는 느낌
}
