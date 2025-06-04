using UnityEngine;

/// <summary>
/// Event on which <c>AudioCue</c> components send a message to play SFX and music. <c>AudioManager</c> listens on these events, and actually plays the sound.
/// </summary>
[CreateAssetMenu(menuName = "Events/AudioCue Event Channel")]
public class AudioCueEventChannelSO : DescriptionBaseSO
{
	public AudioCuePlayAction OnAudioCuePlayRequested;
	public AudioCueStopAction OnAudioCueStopRequested;
	public AudioCueFinishAction OnAudioCueFinishRequested;

	//음악 재생. 위치에 따라 들리는 소리가 다르다.
	public AudioCueKey RaisePlayEvent(
		AudioCueSO audioCue,
		AudioConfigurationSO audioConfiguration,
		Vector3 positionInSpace = default
	)
	{
		//AudioCueKey 초기화
		AudioCueKey audioCueKey = AudioCueKey.Invalid;

		//OnAudioCuePlayRequested 이벤트 발동
		if (OnAudioCuePlayRequested != null)
		{
			audioCueKey = OnAudioCuePlayRequested.Invoke(audioCue, audioConfiguration, positionInSpace);
		}
		else
		{
			Debug.LogWarning("An AudioCue play event was requested  for " + audioCue.name +
			                 ", but nobody picked it up. " +
			                 "Check why there is no AudioManager already loaded, " +
			                 "and make sure it's listening on this AudioCue Event channel.");
		}

		return audioCueKey;
	}

	//음악 멈추는 기능 => 멈췄으면 true 출력
	public bool RaiseStopEvent(AudioCueKey audioCueKey)
	{
		bool requestSucceed = false;

		if (OnAudioCueStopRequested != null)
		{
			requestSucceed = OnAudioCueStopRequested.Invoke(audioCueKey);
		}
		else
		{
			Debug.LogWarning("An AudioCue stop event was requested, but nobody picked it up. " +
			                 "Check why there is no AudioManager already loaded, " +
			                 "and make sure it's listening on this AudioCue Event channel.");
		}

		return requestSucceed;
	}

	//음악을 끝내는 기능 => 끝냈으면 true 출력
	public bool RaiseFinishEvent(AudioCueKey audioCueKey)
	{
		bool requestSucceed = false;

		if (OnAudioCueStopRequested != null)
		{
			requestSucceed = OnAudioCueFinishRequested.Invoke(audioCueKey);
		}
		else
		{
			Debug.LogWarning("An AudioCue finish event was requested, but nobody picked it up. " +
			                 "Check why there is no AudioManager already loaded, " +
			                 "and make sure it's listening on this AudioCue Event channel.");
		}

		return requestSucceed;
	}
}

public delegate AudioCueKey AudioCuePlayAction(AudioCueSO audioCue, AudioConfigurationSO audioConfiguration,
	Vector3 positionInSpace);

public delegate bool AudioCueStopAction(AudioCueKey emitterKey);

public delegate bool AudioCueFinishAction(AudioCueKey emitterKey);
