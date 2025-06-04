using System.Collections.Generic;
using UnityEngine;

// Created with collaboration with:
// https://forum.unity.com/threads/inventory-system.980646/

[CreateAssetMenu(fileName = "Inventory", menuName = "Inventory/Inventory")]
public class InventorySO : ScriptableObject
{
	[Tooltip("The collection of items and their quantities.")]
	[SerializeField] private List<ItemStack> _items = new List<ItemStack>();
	[SerializeField] private List<ItemStack> _defaultItems = new List<ItemStack>();
	
	public List<ItemStack> Items => _items;

	public void Init()
	{
		if (_items == null)
		{
			_items = new List<ItemStack>();
		}
		_items.Clear();
		foreach (ItemStack item in _defaultItems)
		{
			_items.Add(new ItemStack(item));
		}
	}

	//인벤토리에 아이템 추가, 기본적으로 아이템은 한 개를 받으니 count의 디폴트 값은 1이다.
	public void Add(ItemSO item, int count = 1)
	{
		//아이템
		if (count <= 0)
			return;

		for (int i = 0; i < _items.Count; i++)
		{
			ItemStack currentItemStack = _items[i];

			//만약 아이템을 가지고 있다면. => 개수만 추가
			if (item == currentItemStack.Item)
			{
				//소비아이템만 갯수가 있음
				if (currentItemStack.Item.ItemType.ActionType == ItemInventoryActionType.Use)
				{
					currentItemStack.Amount += count;
				}

				return;
			}
		}

		//else, 만약 아이템이 없다면 아이템 새로 추가
		_items.Add(new ItemStack(item, count));
	}

	
	public void Remove(ItemSO item, int count = 1)
	{
		if (count <= 0)
			return;

		for (int i = 0; i < _items.Count; i++)
		{
			ItemStack currentItemStack = _items[i];

			if (currentItemStack.Item == item)
			{
				currentItemStack.Amount -= count;

				if (currentItemStack.Amount <= 0)
					_items.Remove(currentItemStack);

				return;
			}
		}
	}

	//포함되어 있는가
	public bool Contains(ItemSO item)
	{
		for (int i = 0; i < _items.Count; i++)
		{
			if (item == _items[i].Item)
			{
				return true;
			}
		}

		return false;
	}

	//아이템의 갯수 리턴
	public int Count(ItemSO item)
	{
		for (int i = 0; i < _items.Count; i++)
		{
			ItemStack currentItemStack = _items[i];
			if (item == currentItemStack.Item)
			{
				return currentItemStack.Amount;
			}
		}

		return 0;
	}

	//재료로 쓸 수 있는지,
	//예를 들어 당근2개 포도 1개가 필요한데 당근 포도 1개만 있다면
	//false, true 출력
	public bool[] IngredientsAvailability(List<ItemStack> ingredients)
	{
		if (ingredients == null)
			return null;
		bool[] availabilityArray = new bool[ingredients.Count];

		for (int i = 0; i < ingredients.Count; i++)
		{
			availabilityArray[i] = _items.Exists(o => o.Item == ingredients[i].Item && o.Amount >= ingredients[i].Amount);

		}
		return availabilityArray;
	}

	//하나라도 재료가 부족하다면 => false
	public bool hasIngredients(List<ItemStack> ingredients)
	{

		bool hasIngredients = !ingredients.Exists(j => !_items.Exists(o => o.Item == j.Item && o.Amount >= j.Amount));

		return hasIngredients;


	}
}
