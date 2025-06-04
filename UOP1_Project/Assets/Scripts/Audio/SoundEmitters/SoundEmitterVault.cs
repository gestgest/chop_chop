using System.Collections.Generic;

public class SoundEmitterVault
{
	private int _nextUniqueKey = 0;
	private List<AudioCueKey> _emittersKey;
	private List<SoundEmitter[]> _emittersList;

	public SoundEmitterVault()
	{
		_emittersKey = new List<AudioCueKey>();
		_emittersList = new List<SoundEmitter[]>();
	}

	public AudioCueKey GetKey(AudioCueSO cue)
	{
		return new AudioCueKey(_nextUniqueKey++, cue); //값과 음악리스트를 가지고 있는 AudioCueKey
	}

	public void Add(AudioCueKey key, SoundEmitter[] emitter)
	{
		_emittersKey.Add(key);
		_emittersList.Add(emitter);
	}

	public AudioCueKey Add(AudioCueSO cue, SoundEmitter[] emitter)
	{
		AudioCueKey emitterKey = GetKey(cue);

		_emittersKey.Add(emitterKey);
		_emittersList.Add(emitter);

		return emitterKey;
	}

	//리스트 값이 있는지 없는지 확인 겸 음악 속성 리스트 가져오는 함수
	public bool Get(AudioCueKey key, out SoundEmitter[] emitter)
	{
		//key값과 _emittersKey의 내용물이 같은 경우 index값 반환
		//x == key는 AudioCueKey함수안에 있는 operator 함수다.
		int index = _emittersKey.FindIndex(x => x == key);

		if (index < 0)
		{
			emitter = null;
			return false;
		}

		//음악 속성 리스트를 넣는다.
		emitter = _emittersList[index];
		return true;
	}

	public bool Remove(AudioCueKey key)
	{
		int index = _emittersKey.FindIndex(x => x == key);
		return RemoveAt(index);
	}

	private bool RemoveAt(int index)
	{
		if (index < 0)
		{
			return false;
		}

		_emittersKey.RemoveAt(index);
		_emittersList.RemoveAt(index);

		return true;
	}
}
