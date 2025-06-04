using UnityEngine;
using UnityEngine.AddressableAssets;

//This class is a base class which contains what is common to all game scenes (Locations, Menus, Managers)
/// <summary>
/// 이 클래스는 게임 씬에 공통적으로 있는 속성을 포함하고 있다.
/// </summary>
public class GameSceneSO : DescriptionBaseSO
{
	public GameSceneType sceneType;
	public AssetReference sceneReference; //Used at runtime to load the scene from the right AssetBundle
	public AudioCueSO musicTrack;

	/// <summary>
	/// Used by the SceneSelector tool to discern what type of scene it needs to load
	/// GameSceneType는 SceneSelector 클래스가 장면 타입을 식별할 때 쓰인다
	/// </summary>
	public enum GameSceneType
	{
		//주요 게임 플레이씬
		Location, //게임 지역씬
		Menu,

		//Special scenes
		Initialisation,
		PersistentManagers,
		Gameplay,

		//Work in progress scenes that don't need to be played
		Art,
	}
}
