using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

// Created with collaboration from:
// https://forum.unity.com/threads/inventory-system.980646/

[CreateAssetMenu(fileName = "Item", menuName = "Inventory/Item")]
public class ItemSO : SerializableScriptableObject
{

	//언어마다 다른 언어
	[Tooltip("The name of the item")]
	[SerializeField] private LocalizedString _name = default;

	[Tooltip("A preview image for the item")]
	[SerializeField]
	private Sprite _previewImage = default; //이미지

	[Tooltip("A description of the item")]
	[SerializeField]
	private LocalizedString _description = default; //설명

	[Tooltip("A description of the item")]
	[SerializeField]
	private int _healthResorationValue = default; //

	[Tooltip("The type of item")]
	[SerializeField]
	private ItemTypeSO _itemType = default; // 타입

	[Tooltip("A prefab reference for the model of the item")]
	[SerializeField]
	private GameObject _prefab = default; //아이템 모델


	public LocalizedString Name => _name;
	public Sprite PreviewImage => _previewImage;
	public LocalizedString Description => _description;
	public int HealthResorationValue => _healthResorationValue;
	public ItemTypeSO ItemType => _itemType;
	public GameObject Prefab => _prefab;
	public virtual List<ItemStack> IngredientsList { get; }
	public virtual ItemSO ResultingDish { get; }

	public virtual bool IsLocalized { get; }
	public virtual LocalizedSprite LocalizePreviewImage { get; }

}
