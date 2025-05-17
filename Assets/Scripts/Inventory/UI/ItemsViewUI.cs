using UnityEngine;
using System.Collections.Generic;

public class ItemsViewUI : MonoBehaviour
{
    [Header("Prefabs & Containers")]
    [SerializeField] private GameObject _itemUIPrefab; 
    [SerializeField] private Transform _itemsContainer; 

    [Header("Initial Items")]
    [SerializeField] private List<ItemData> _initialItemsList = new List<ItemData>(); 
    
    private Dictionary<ItemData, ItemUIView> displayedItemViews = new Dictionary<ItemData, ItemUIView>();
    private InventoryManager _inventoryManager;

    public void Init(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager;
        PopulateInitialItems();
    }

    private void PopulateInitialItems()
    {
        foreach (ItemData itemData in _initialItemsList)
        {
            AddItem(itemData);
        }
    }
    
    public void AddItem(ItemData itemData)
    {
        if (itemData == null || displayedItemViews.ContainsKey(itemData))
        {
            return;
        }

        GameObject itemGO = Instantiate(_itemUIPrefab, _itemsContainer);
        ItemUIView itemView = itemGO.GetComponent<ItemUIView>();
        itemView.Init(_inventoryManager, itemData, ItemUIView.DragSource.ItemsView);
        displayedItemViews[itemData] = itemView;
    }

    public void RemoveItem(ItemData itemData, bool destroyObject = true)
    {
        if (itemData != null && displayedItemViews.TryGetValue(itemData, out ItemUIView itemView))
        {
            if (destroyObject && ItemUIView.CurrentlyDraggedItem != itemView) 
            {
                Destroy(itemView.gameObject);
            }
            
            displayedItemViews.Remove(itemData);
        }
    }
}