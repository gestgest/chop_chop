using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


/// <summary>
/// This class contains the function to call when play button is pressed
/// </summary>
public class StartGame : MonoBehaviour
{
	//게임 시작할 때 나오는 초기 씬
	[SerializeField] private GameSceneSO _locationsToLoad;
	[SerializeField] private SaveSystem _saveSystem = default;
	[SerializeField] private bool _showLoadScreen = default;

	//Listening to => broadcasting on
	[Header("Broadcasting on")]
	[SerializeField] private LoadEventChannelSO _loadLocation = default; //from SceneLoader.cs

	//publisher) 넣는 함수
	[Header("Listening to")]
	[SerializeField] private VoidEventChannelSO _onNewGameButton = default;
	[SerializeField] private VoidEventChannelSO _onContinueButton = default;

	private bool _hasSaveData;

	private void Start()
	{

		_hasSaveData = _saveSystem.LoadSaveDataFromDisk();
		_onNewGameButton.OnEventRaised += StartNewGame;
		_onContinueButton.OnEventRaised += ContinuePreviousGame;
	}

	private void OnDestroy()
	{
		_onNewGameButton.OnEventRaised -= StartNewGame;
		_onContinueButton.OnEventRaised -= ContinuePreviousGame;
	}

	/// <summary>
	/// 새로운 개임 시작
	/// </summary>
	private void StartNewGame()
	{
		_hasSaveData = false; //애초에 새로 시작해서 새로운 데이터가 없음

		_saveSystem.WriteEmptySaveFile(); // todo : 이거는 빼도 되지 않을까?
		_saveSystem.SetNewGameData(); //새롭게 디스크 초기화
		_loadLocation.RaiseEvent(_locationsToLoad, _showLoadScreen); //씬 로드
	}

	//불러오기
	private void ContinuePreviousGame()
	{
		StartCoroutine(LoadSaveGame());
	}

	private void OnResetSaveDataPress()
	{
		_hasSaveData = false;
	}

	private IEnumerator LoadSaveGame()
	{
		//인벤토리 가져오기
		yield return StartCoroutine(_saveSystem.LoadSavedInventory());

		//퀘스트 가져오기
		_saveSystem.LoadSavedQuestlineStatus();
		var locationGuid = _saveSystem.saveData._locationId;

		//위치 비동기로 가져오기
		var asyncOperationHandle = Addressables.LoadAssetAsync<LocationSO>(locationGuid);

		yield return asyncOperationHandle;

		if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
		{
			LocationSO locationSO = asyncOperationHandle.Result;
			_loadLocation.RaiseEvent(locationSO, _showLoadScreen); //씬 불러오기
		}
	}
}
