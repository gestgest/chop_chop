using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	[Header("SoundEmitters pool")]
	[SerializeField] private SoundEmitterPoolSO _pool = default;
	[SerializeField] private int _initialSize = 10;

	[Header("Listening on channels")]
	[Tooltip("The SoundManager listens to this event, fired by objects in any scene, to play SFXs")]
	[SerializeField] private AudioCueEventChannelSO _SFXEventChannel = default;
	[Tooltip("The SoundManager listens to this event, fired by objects in any scene, to play Music")]
	[SerializeField] private AudioCueEventChannelSO _musicEventChannel = default;
	[Tooltip("The SoundManager listens to this event, fired by objects in any scene, to change SFXs volume")]
	[SerializeField] private FloatEventChannelSO _SFXVolumeEventChannel = default;
	[Tooltip("The SoundManager listens to this event, fired by objects in any scene, to change Music volume")]
	[SerializeField] private FloatEventChannelSO _musicVolumeEventChannel = default;
	[Tooltip("The SoundManager listens to this event, fired by objects in any scene, to change Master volume")]
	[SerializeField] private FloatEventChannelSO _masterVolumeEventChannel = default;


	[Header("Audio control")]
	[SerializeField] private AudioMixer audioMixer = default;
	[Range(0f, 1f)]
	[SerializeField] private float _masterVolume = 1f;
	[Range(0f, 1f)]
	[SerializeField] private float _musicVolume = 1f;
	[Range(0f, 1f)]
	[SerializeField] private float _sfxVolume = 1f;

	private SoundEmitterVault _soundEmitterVault;
	private SoundEmitter _musicSoundEmitter; //뮤직 오디오 소스

	private void Awake()
	{
		//TODO: Get the initial volume levels from the settings
		_soundEmitterVault = new SoundEmitterVault();

		_pool.Prewarm(_initialSize);
		_pool.SetParent(this.transform);
	}

	private void OnEnable()
	{
		//SFX 이벤트(함수) 설정 [시작, 멈출때, 끝낼때]
		//delegate, 즉 _SFXEventChannel의 함수에 PlayAudioCue함수 내용을 넣는다.
		_SFXEventChannel.OnAudioCuePlayRequested += PlayAudioCue;
		_SFXEventChannel.OnAudioCueStopRequested += StopAudioCue;
		_SFXEventChannel.OnAudioCueFinishRequested += FinishAudioCue;

		//Music 이벤트 설정
		_musicEventChannel.OnAudioCuePlayRequested += PlayMusicTrack;
		_musicEventChannel.OnAudioCueStopRequested += StopMusic;

		_masterVolumeEventChannel.OnEventRaised += ChangeMasterVolume;
		_musicVolumeEventChannel.OnEventRaised += ChangeMusicVolume;
		_SFXVolumeEventChannel.OnEventRaised += ChangeSFXVolume;
	}

	private void OnDestroy()
	{
		_SFXEventChannel.OnAudioCuePlayRequested -= PlayAudioCue;
		_SFXEventChannel.OnAudioCueStopRequested -= StopAudioCue;

		_SFXEventChannel.OnAudioCueFinishRequested -= FinishAudioCue;
		_musicEventChannel.OnAudioCuePlayRequested -= PlayMusicTrack;

		_musicVolumeEventChannel.OnEventRaised -= ChangeMusicVolume;
		_SFXVolumeEventChannel.OnEventRaised -= ChangeSFXVolume;
		_masterVolumeEventChannel.OnEventRaised -= ChangeMasterVolume;
	}

	/// <summary>
	/// This is only used in the Editor, to debug volumes.
	/// It is called when any of the variables is changed, and will directly change the value of the volumes on the AudioMixer.
	/// 유니티 에디터상[inspector]에서 값이 바뀐 경우에 실행
	/// </summary>
	void OnValidate()
	{
		if (Application.isPlaying)
		{
			SetGroupVolume("MasterVolume", _masterVolume);
			SetGroupVolume("MusicVolume", _musicVolume);
			SetGroupVolume("SFXVolume", _sfxVolume);
		}
	}
	void ChangeMasterVolume(float newVolume)
	{
		_masterVolume = newVolume;
		SetGroupVolume("MasterVolume", _masterVolume);
	}
	void ChangeMusicVolume(float newVolume)
	{
		_musicVolume = newVolume;
		SetGroupVolume("MusicVolume", _musicVolume);
	}
	void ChangeSFXVolume(float newVolume)
	{
		_sfxVolume = newVolume;
		SetGroupVolume("SFXVolume", _sfxVolume);
	}
	public void SetGroupVolume(string parameterName, float normalizedVolume)
	{
		bool volumeSet = audioMixer.SetFloat(parameterName, NormalizedToMixerValue(normalizedVolume));
		if (!volumeSet)
			Debug.LogError("The AudioMixer parameter was not found");
	}

	public float GetGroupVolume(string parameterName)
	{
		if (audioMixer.GetFloat(parameterName, out float rawVolume))
		{
			return MixerValueToNormalized(rawVolume);
		}
		else
		{
			Debug.LogError("The AudioMixer parameter was not found");
			return 0f;
		}
	}

	// Both MixerValueNormalized and NormalizedToMixerValue functions are used for easier transformations
	/// when using UI sliders normalized format
	private float MixerValueToNormalized(float mixerValue)
	{
		// We're assuming the range [-80dB to 0dB] becomes [0 to 1]
		return 1f + (mixerValue / 80f);
	}
	private float NormalizedToMixerValue(float normalizedValue)
	{
		// We're assuming the range [0 to 1] becomes [-80dB to 0dB]
		// This doesn't allow values over 0dB
		return (normalizedValue - 1f) * 80f;
	}

	//음악 틀어주는 함수
	private AudioCueKey PlayMusicTrack(AudioCueSO audioCue, AudioConfigurationSO audioConfiguration, Vector3 positionInSpace)
	{
		float fadeDuration = 2f;
		float startTime = 0f;

		if (_musicSoundEmitter != null && _musicSoundEmitter.IsPlaying())
		{
			AudioClip songToPlay = audioCue.GetClips()[0];

			//만약 플레이 하려는 음악이 지금 재생하려는 음악이랑 같다면.
			if (_musicSoundEmitter.GetClip() == songToPlay)
				return AudioCueKey.Invalid; //그냥 무시

			//Music is already playing, need to fade it out
			startTime = _musicSoundEmitter.FadeMusicOut(fadeDuration);
			//_musicSoundEmitter.FadeMusicOut(fadeDuration);
		}

		_musicSoundEmitter = _pool.Request();
		_musicSoundEmitter.FadeMusicIn(audioCue.GetClips()[0], audioConfiguration, 1f, startTime);
		_musicSoundEmitter.OnSoundFinishedPlaying += StopMusicEmitter;

		return AudioCueKey.Invalid; //No need to return a valid key for music
	}

	private bool StopMusic(AudioCueKey key)
	{
		if (_musicSoundEmitter != null && _musicSoundEmitter.IsPlaying())
		{
			_musicSoundEmitter.Stop();
			return true;
		}
		else
			return false;
	}

	/// <summary>
	/// Only used by the timeline to stop the gameplay music during cutscenes.
	/// Called by the SignalReceiver present on this same GameObject.
	/// </summary>
	public void TimelineInterruptsMusic()
	{
		StopMusic(AudioCueKey.Invalid);
	}

	/// <summary>
	/// Plays an AudioCue by requesting the appropriate number of SoundEmitters from the pool.
	/// </summary>
	public AudioCueKey PlayAudioCue(AudioCueSO audioCue, AudioConfigurationSO settings, Vector3 position = default)
	{
		AudioClip[] clipsToPlay = audioCue.GetClips(); //오디오 시리즈를 가져온다.
		SoundEmitter[] soundEmitterArray = new SoundEmitter[clipsToPlay.Length];

		int nOfClips = clipsToPlay.Length;
		for (int i = 0; i < nOfClips; i++)
		{
			soundEmitterArray[i] = _pool.Request();
			if (soundEmitterArray[i] != null)
			{
				//사운드 재생
				soundEmitterArray[i].PlayAudioClip(clipsToPlay[i], settings, audioCue.looping, position);

				//만약 반복재생이 아니라면 끝날때 함수 추가.
				if (!audioCue.looping)
					soundEmitterArray[i].OnSoundFinishedPlaying += OnSoundEmitterFinishedPlaying;
			}
		}

		return _soundEmitterVault.Add(audioCue, soundEmitterArray);
	}

	public bool FinishAudioCue(AudioCueKey audioCueKey)
	{
		//리스트 값이 있는지 없는지 확인 겸 음악 제어 리스트 가져오는 함수
		bool isFound = _soundEmitterVault.Get(audioCueKey, out SoundEmitter[] soundEmitters);

		if (isFound)
		{
			for (int i = 0; i < soundEmitters.Length; i++)
			{
				soundEmitters[i].Finish();
				soundEmitters[i].OnSoundFinishedPlaying += OnSoundEmitterFinishedPlaying;
			}
		}
		else
		{
			Debug.LogWarning("Finishing an AudioCue was requested, but the AudioCue was not found.");
		}

		return isFound;
	}

	public bool StopAudioCue(AudioCueKey audioCueKey)
	{
		//리스트 값이 있는지 없는지 확인 겸 음악 제어 리스트 가져오는 함수
		bool isFound = _soundEmitterVault.Get(audioCueKey, out SoundEmitter[] soundEmitters);

		//만약 sound가 존재한다면
		if (isFound)
		{
			for (int i = 0; i < soundEmitters.Length; i++)
			{
				StopAndCleanEmitter(soundEmitters[i]);
			}

			_soundEmitterVault.Remove(audioCueKey);
		}

		return isFound;
	}

	private void OnSoundEmitterFinishedPlaying(SoundEmitter soundEmitter)
	{
		StopAndCleanEmitter(soundEmitter);
	}

	private void StopAndCleanEmitter(SoundEmitter soundEmitter)
	{
		if (!soundEmitter.IsLooping())
			soundEmitter.OnSoundFinishedPlaying -= OnSoundEmitterFinishedPlaying;

		soundEmitter.Stop();
		_pool.Return(soundEmitter);

		//TODO: is the above enough?
		//_soundEmitterVault.Remove(audioCueKey); is never called if StopAndClean is called after a Finish event
		//How is the key removed from the vault?
	}

	private void StopMusicEmitter(SoundEmitter soundEmitter)
	{
		soundEmitter.OnSoundFinishedPlaying -= StopMusicEmitter;
		_pool.Return(soundEmitter);
	}
}
