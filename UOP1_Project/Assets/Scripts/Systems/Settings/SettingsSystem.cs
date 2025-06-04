using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Rendering.Universal;

public class SettingsSystem : MonoBehaviour
{
	[SerializeField] private VoidEventChannelSO SaveSettingsEvent = default; //UnityAction 담는 SO

	[SerializeField] private SettingsSO _currentSettings = default;
	[SerializeField] private UniversalRenderPipelineAsset _urpAsset = default; //URP
	[SerializeField] private SaveSystem _saveSystem = default;

	[SerializeField] private FloatEventChannelSO _changeMasterVolumeEventChannel = default;
	[SerializeField] private FloatEventChannelSO _changeSFXVolumeEventChannel = default;
	[SerializeField] private FloatEventChannelSO _changeMusicVolumeEventChannel = default;

	private void Awake()
	{
		_saveSystem.LoadSaveDataFromDisk(); //디스크 로드
		_currentSettings.LoadSavedSettings(_saveSystem.saveData); //설정 가져오기
		SetCurrentSettings();
	}
	private void OnEnable()
	{
		SaveSettingsEvent.OnEventRaised += SaveSettings;
	}
	private void OnDisable()
	{
		SaveSettingsEvent.OnEventRaised -= SaveSettings;
	}

	/// <summary>
	/// Set current settings, 설정 가져온 값을 적용하는 함수
	/// </summary>
	void SetCurrentSettings()
	{
		//음악 관련 코드들
		_changeMusicVolumeEventChannel.RaiseEvent(_currentSettings.MusicVolume);//raise event for volume change
		_changeSFXVolumeEventChannel.RaiseEvent(_currentSettings.SfxVolume); //raise event for volume change
		_changeMasterVolumeEventChannel.RaiseEvent(_currentSettings.MasterVolume); //raise event for volume change

		//해상도 관련 코드들 => 해상도 크기, 풀 스크린인지
		Resolution currentResolution = Screen.currentResolution; // get a default resolution in case saved resolution doesn't exist in the resolution List
		if (_currentSettings.ResolutionsIndex < Screen.resolutions.Length)
			currentResolution = Screen.resolutions[_currentSettings.ResolutionsIndex];

		//설정
		Screen.SetResolution(currentResolution.width,
			currentResolution.height,
			_currentSettings.IsFullscreen
		);

		//그림자 거리, 안티 앨리어싱 => 그래픽 처리 기술
		_urpAsset.shadowDistance = _currentSettings.ShadowDistance;
		_urpAsset.msaaSampleCount = _currentSettings.AntiAliasingIndex;

		LocalizationSettings.SelectedLocale = _currentSettings.CurrentLocale; //무슨 언어인지
	}
	void SaveSettings()
	{
		_saveSystem.SaveDataToDisk();
	}





}

