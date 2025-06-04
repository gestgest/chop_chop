using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{
	private AudioSource _audioSource;

	public event UnityAction<SoundEmitter> OnSoundFinishedPlaying;

	private void Awake()
	{
		_audioSource = this.GetComponent<AudioSource>();
		_audioSource.playOnAwake = false;
		//씬이 실행되는 순간 바로 오디오 재생 [true라면]
	}

	/// <summary>
	/// Instructs the AudioSource to play a single clip, with optional looping, in a position in 3D space.
	/// </summary>
	/// <param name="clip"></param>
	/// <param name="settings"></param>
	/// <param name="hasToLoop"></param>
	/// <param name="position">포지션</param>
	public void PlayAudioClip(AudioClip clip, AudioConfigurationSO settings, bool hasToLoop, Vector3 position = default)
	{
		_audioSource.clip = clip;
		settings.ApplyTo(_audioSource); //세부속성 적용
		_audioSource.transform.position = position;
		_audioSource.loop = hasToLoop;
		_audioSource.time = 0f; //Reset in case this AudioSource is being reused for a short SFX after being used for a long music track
		_audioSource.Play();

		if (!hasToLoop)
		{
			//clip.length 초? 만큼 기다리고 끝 날때 나는 오디오 재생
			StartCoroutine(FinishedPlaying(clip.length));
		}
	}

	public void FadeMusicIn(AudioClip musicClip, AudioConfigurationSO settings, float duration, float startTime = 0f)
	{
		PlayAudioClip(musicClip, settings, true); //반복하는 브금 재생
		_audioSource.volume = 0f;

		//Start the clip at the same time the previous one left, if length allows
		//TODO: 사라지는 소리를 동기화 하는 방법을 찾자  
		//저장된 startTime에서 시작
		if (startTime <= _audioSource.clip.length)
			_audioSource.time = startTime;

		_audioSource.DOFade(settings.Volume, duration); // 서서히 볼륨이 커짐
	}

	public float FadeMusicOut(float duration)
	{
		_audioSource.DOFade(0f, duration).onComplete += OnFadeOutComplete;

		return _audioSource.time;
	}

	private void OnFadeOutComplete()
	{
		NotifyBeingDone();
	}

	/// <summary>
	/// Used to check which music track is being played.
	/// </summary>
	public AudioClip GetClip()
	{
		return _audioSource.clip;
	}


	/// <summary>
	/// Used when the game is unpaused, to pick up SFX from where they left.
	/// </summary>
	public void Resume()
	{
		_audioSource.Play();
	}

	/// <summary>
	/// Used when the game is paused.
	/// </summary>
	public void Pause()
	{
		_audioSource.Pause();
	}

	public void Stop()
	{
		_audioSource.Stop();
	}

public void Finish()
{
	if (_audioSource.loop)
	{
		_audioSource.loop = false;
		float timeRemaining = _audioSource.clip.length - _audioSource.time; //남은 시간
		StartCoroutine(FinishedPlaying(timeRemaining));
	}
}

	public bool IsPlaying()
	{
		return _audioSource.isPlaying;
	}

	public bool IsLooping()
	{
		return _audioSource.loop;
	}

	IEnumerator FinishedPlaying(float clipLength)
	{
		yield return new WaitForSeconds(clipLength);

		NotifyBeingDone();
	}

	private void NotifyBeingDone()
	{
		OnSoundFinishedPlaying.Invoke(this); // The AudioManager will pick this up
	}
}
