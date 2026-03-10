using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.UI;

[System.Serializable]
public enum SettingFieldType
{
	Language,
	Volume_SFx,
	Volume_Music,
	Resolution,
	FullScreen,
	ShadowDistance,
	AntiAliasing,
	ShadowQuality,
	Volume_Master,

}
[System.Serializable]
public class SettingTab
{
	public SettingsType settingTabsType;
	public LocalizedString title;
}

[System.Serializable]
public class SettingField
{
	public SettingsType settingTabsType;
	public SettingFieldType settingFieldType;
	public LocalizedString title;
}

public enum SettingsType
{
	Language,
	Graphics,
	Audio,
}

//setting panel
public class UISettingsController : MonoBehaviour
{
	//Panel안에 있는 세팅 UI
	[SerializeField] private UISettingsLanguageComponent _languageComponent;
	[SerializeField] private UISettingsGraphicsComponent _graphicsComponent;
	[SerializeField] private UISettingsAudioComponent _audioComponent;
	[SerializeField] private UISettingTabsFiller _settingTabFiller = default;

	[SerializeField] private SettingsSO _currentSettings = default;
	[SerializeField] private List<SettingsType> _settingTabsList = new List<SettingsType>();

	private SettingsType _selectedTab = SettingsType.Audio;
	[SerializeField] private InputReader _inputReader = default;
	[SerializeField] private VoidEventChannelSO SaveSettingsEvent = default;

	//닫는 함수 : 대체적으로 전에 있던 화면으로 되돌아간다.
	public UnityAction Closed;
	private void OnEnable()
	{
		//save 함수 저장 : 대체로 SettingSO에 있는 값을 가져옴
		_languageComponent._save += SaveLaguageSettings;
		_audioComponent._save += SaveAudioSettings;
		_graphicsComponent._save += SaveGraphicsSettings;

		_inputReader.MenuCloseEvent += CloseScreen; //esc?
		_inputReader.TabSwitched += SwitchTab; //tab?

		_settingTabFiller.FillTabs(_settingTabsList);
		_settingTabFiller.ChooseTab += OpenSetting; //탭 변환할때 기존 탭 비활성화

		//디폴트는 오디오 탭
		OpenSetting(SettingsType.Audio);
	}
	private void OnDisable()
	{
		_inputReader.MenuCloseEvent -= CloseScreen;
		_inputReader.TabSwitched -= SwitchTab;

		_languageComponent._save -= SaveLaguageSettings;
		_audioComponent._save -= SaveAudioSettings;
		_graphicsComponent._save -= SaveGraphicsSettings;
	}
	public void CloseScreen()
	{
		Closed.Invoke();
	}


	void OpenSetting(SettingsType settingType)
	{
		_selectedTab = settingType;
		switch (settingType)
		{
			case SettingsType.Language:
				_currentSettings.SaveLanguageSettings(_currentSettings.CurrentLocale);
				break;
			case SettingsType.Graphics:
				_graphicsComponent.Setup();
				break;
			case SettingsType.Audio:
				_audioComponent.Setup(_currentSettings.MusicVolume, _currentSettings.SfxVolume, _currentSettings.MasterVolume);
				break;
			default:
				break;
		}

		_languageComponent.gameObject.SetActive(settingType == SettingsType.Language);
		_graphicsComponent.gameObject.SetActive((settingType == SettingsType.Graphics));
		_audioComponent.gameObject.SetActive(settingType == SettingsType.Audio);
		_settingTabFiller.SelectTab(settingType);
	}

	// 유추 : 게임패드 한정 탭 스위칭
	void SwitchTab(float orientation)
	{
		if (orientation != 0)
		{
			Debug.Log("엄준식 : " + orientation);
			bool isLeft = orientation < 0;
			int initialIndex = _settingTabsList.FindIndex(o => o == _selectedTab);
			if (initialIndex != -1)
			{
				if (isLeft)
				{
					initialIndex--;
				}
				else
				{
					initialIndex++;
				}

				initialIndex = Mathf.Clamp(initialIndex, 0, _settingTabsList.Count - 1);
			}

			OpenSetting(_settingTabsList[initialIndex]);
		}
	}

	//저장 함수 라인

	/// <summary>
	/// 언어 저장
	/// </summary>
	/// <param name="local">국적</param>
	public void SaveLaguageSettings(Locale local)
	{
		_currentSettings.SaveLanguageSettings(local);
		SaveSettingsEvent.RaiseEvent();
	}
	public void SaveGraphicsSettings(int newResolutionsIndex, int newAntiAliasingIndex, float newShadowDistance, bool fullscreenState)
	{
		_currentSettings.SaveGraphicsSettings(newResolutionsIndex, newAntiAliasingIndex, newShadowDistance, fullscreenState);
		SaveSettingsEvent.RaiseEvent();
	}
	void SaveAudioSettings(float musicVolume, float sfxVolume, float masterVolume)
	{
		_currentSettings.SaveAudioSettings(musicVolume, sfxVolume, masterVolume);

		SaveSettingsEvent.RaiseEvent();
	}

}
