using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
	[SerializeField] private VoidEventChannelSO _onSceneReady = default;
	[SerializeField] private AudioCueEventChannelSO _playMusicOn = default;
	[SerializeField] private GameSceneSO _thisSceneSO = default;
	[SerializeField] private AudioConfigurationSO _audioConfig = default;

	[Header("Pause menu music")]
	[SerializeField] private AudioCueSO _pauseMusic = default;
	[SerializeField] private BoolEventChannelSO _onPauseOpened = default;

	private void OnEnable()
	{
		_onPauseOpened.OnEventRaised += PlayPauseMusic; //음악 멈추는 이벤트 넣기
		_onSceneReady.OnEventRaised += PlayMusic; //음악 재생 이벤트 넣기
	}

	private void OnDisable()
	{
		_onSceneReady.OnEventRaised -= PlayMusic;
		_onPauseOpened.OnEventRaised -= PlayPauseMusic;
	}

	/// <summary>
	/// 음악 재생시켜주는 함수
	/// </summary>
	private void PlayMusic()
	{
		_playMusicOn.RaisePlayEvent(_thisSceneSO.musicTrack, _audioConfig);
	}

	//open이 true면 느린 음악이 나옴 => esc 누를 경우
	private void PlayPauseMusic(bool open)
	{
		//esc를 누를 경우 => 설정으로 이동
		if (open)
		{
			_playMusicOn.RaisePlayEvent(_pauseMusic, _audioConfig);
		}
		// 설정에서 빠져 나간 경우
		else
		{
			PlayMusic();
		}
	}
}
