using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class InventoryViewUI : MonoBehaviour
{
    [Header("Prefabs & Containers")]
    [SerializeField] private GameObject _inventoryCellPrefab;
    [SerializeField] private Transform _cellsContainer; 
    [SerializeField] private GameObject _itemUIPrefab; 
    [SerializeField] private RectTransform _placedItemsContainer; 

    [Header("Layout Settings")]
    [SerializeField] private GridLayoutGroup _gridLayoutGroup; 

    private InventoryCellView[,] cellViews;
    private int currentRows, currentColumns;
    private List<InventoryCellView> _highlightedCells = new List<InventoryCellView>();
    private Dictionary<ItemData, ItemUIView> displayedInventoryItemViews = new Dictionary<ItemData, ItemUIView>();
    private InventoryManager _inventoryManager;

    public void Init(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager;
        
        _inventoryManager.OnItemPlacedInInventory += HandleItemPlaced;
        _inventoryManager.OnItemRemovedFromInventory += HandleItemRemoved;
        
        if (_gridLayoutGroup == null && _cellsContainer != null) 
            _gridLayoutGroup = _cellsContainer.GetComponent<GridLayoutGroup>();
    }

    private void OnDestroy()
    {
        _inventoryManager.OnItemPlacedInInventory -= HandleItemPlaced;
        _inventoryManager.OnItemRemovedFromInventory -= HandleItemRemoved;
    }

    public void FillInventory(int numRows, int numColumns, Func<int, int, bool> displayCellCondition)
    {
        currentRows = numRows;
        currentColumns = numColumns;

        foreach (Transform child in _cellsContainer) Destroy(child.gameObject);
        
        if (_placedItemsContainer != null) 
        {
            foreach (Transform child in _placedItemsContainer) Destroy(child.gameObject);
        }
        
        cellViews = new InventoryCellView[numRows, numColumns];
        _highlightedCells.Clear();
        displayedInventoryItemViews.Clear();

        if (_gridLayoutGroup != null)
        {
            _gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _gridLayoutGroup.constraintCount = numColumns;
        }
        else Debug.LogError("GridLayoutGroup is null.");
        
        for (int r = 0; r < numRows; r++)
        {
            for (int c = 0; c < numColumns; c++)
            {
                GameObject cellGO = Instantiate(_inventoryCellPrefab, _cellsContainer);
                InventoryCellView cellView = cellGO.GetComponent<InventoryCellView>();
                bool display = displayCellCondition(r, c);
                cellView.Initialize(this, r, c, display);
                cellViews[r, c] = cellView;
            }
        }
    }

    private void HandleItemPlaced(ItemData itemData, Vector2Int anchorPosition)
    {
        if (_itemUIPrefab == null)
        {
            Debug.LogError("ItemUIPrefab not assigned in InventoryViewUI.");
            return;
        }
        if (_placedItemsContainer == null)
        {
            Debug.LogError("not assigned in InventoryViewUI.");
            return;
        }

        ItemUIView itemView;
        
        if (!displayedInventoryItemViews.TryGetValue(itemData, out itemView))
        {
            GameObject itemGO = Instantiate(_itemUIPrefab, _placedItemsContainer); 
            itemView = itemGO.GetComponent<ItemUIView>();
            itemView.Init(_inventoryManager, itemData, ItemUIView.DragSource.InventoryGrid, anchorPosition);
            displayedInventoryItemViews[itemData] = itemView;
        }
        
        PositionAndSizeItemView(itemView, itemData, anchorPosition);
    }

    private void PositionAndSizeItemView(ItemUIView itemView, ItemData itemData, Vector2Int anchorPosition)
    {
        if (_gridLayoutGroup == null) {
            Debug.LogError("GridLayoutGroup is null in PositionAndSizeItemView.");
            return;
        }
        if (_placedItemsContainer == null) {
             Debug.LogError("PlacedItemsContainer is null in PositionAndSizeItemView.");
            return;
        }
        if (_cellsContainer == null) { 
            Debug.LogError("CellsContainer is null in PositionAndSizeItemView.");
            return;
        }
        
        RectTransform itemRectTransform = itemView.GetComponent<RectTransform>();
        
        itemRectTransform.anchorMin = new Vector2(0, 1); 
        itemRectTransform.anchorMax = new Vector2(0, 1); 
        itemRectTransform.pivot = new Vector2(0, 1);    

        float cellWidth = _gridLayoutGroup.cellSize.x;
        float cellHeight = _gridLayoutGroup.cellSize.y;
        float spacingX = _gridLayoutGroup.spacing.x;
        float spacingY = _gridLayoutGroup.spacing.y;

        float itemWidth = itemData.Dimensions.x * cellWidth + Mathf.Max(0, itemData.Dimensions.x - 1) * spacingX;
        float itemHeight = itemData.Dimensions.y * cellHeight + Mathf.Max(0, itemData.Dimensions.y - 1) * spacingY;
        itemRectTransform.sizeDelta = new Vector2(itemWidth, itemHeight);
        
        float localXInCellsContainer = _gridLayoutGroup.padding.left + anchorPosition.x * (cellWidth + spacingX);
        float localYInCellsContainer = _gridLayoutGroup.padding.top - anchorPosition.y * (cellHeight + spacingY);
        Vector2 localPosInCellsContainer = new Vector2(localXInCellsContainer, localYInCellsContainer);
        
        Vector3 worldPositionOfItemAnchor = _cellsContainer.TransformPoint(localPosInCellsContainer);
        Vector2 targetAnchoredPositionInPlacedContainer = _placedItemsContainer.InverseTransformPoint(worldPositionOfItemAnchor);
        
        itemRectTransform.anchoredPosition = targetAnchoredPositionInPlacedContainer;
        
        if(itemView.transform.parent != _placedItemsContainer) 
        {
            itemView.transform.SetParent(_placedItemsContainer, false); 
        }
        
        itemView.transform.SetAsLastSibling(); 
    }
    
    private void HandleItemRemoved(ItemData itemData, Vector2Int anchorPosition)
    {
        if (displayedInventoryItemViews.TryGetValue(itemData, out ItemUIView itemView))
        {
            if (ItemUIView.CurrentlyDraggedItem != itemView)
            {
                Destroy(itemView.gameObject);
            }
            
            displayedInventoryItemViews.Remove(itemData);
        }
    }
    
    public void HighlightCells(List<Vector2Int> cellsToHighlightCoords, bool canPlace, Vector2Int anchorCell)
    {
        ClearHighlights(); 

        foreach (var coord in cellsToHighlightCoords)
        {
            if (IsValidCoordinate(coord.y, coord.x))
            {
                InventoryCellView cellView = cellViews[coord.y, coord.x];
                
                if (cellView.IsDisplayable) 
                {
                    cellView.Highlight(canPlace);
                    _highlightedCells.Add(cellView);
                }
            }
        }
    }

    public void ClearHighlights()
    {
        foreach (var cellView in _highlightedCells)
        {
            if (cellView != null) 
            {
                cellView.ResetHighlight();
            }
        }
        
        _highlightedCells.Clear();
    }
    
    public InventoryCellView GetCellView(int r, int c)
    {
        if (!IsValidCoordinate(r,c)) return null;
        return cellViews[r,c];
    }

    private bool IsValidCoordinate(int r, int c)
    {
        return r >= 0 && r < currentRows && c >= 0 && c < currentColumns;
    }
}