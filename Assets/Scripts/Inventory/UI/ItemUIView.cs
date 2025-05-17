using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic; 

public class ItemUIView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _transparencyOnDrag = 0.7f;
    
    private Image _itemIcon;
    private RectTransform _rectTransform;
    private Vector3 _originalLocalPosition;
    private ItemData _itemData;
    private DragSource _currentDragSource;
    private InventoryManager _inventoryManager;
    
    public static ItemUIView CurrentlyDraggedItem { get; private set; }
    public enum DragSource { ItemsView, InventoryGrid }
    public Vector2Int OriginalInventoryAnchor { get; private set; }
    
    public void Init(InventoryManager inventoryManager, ItemData data, DragSource source, Vector2Int inventoryAnchorIfFromGrid = default)
    {
        _rectTransform = GetComponent<RectTransform>();
        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        if (_itemIcon == null && transform.childCount > 0) _itemIcon = GetComponentInChildren<Image>(); 
        else if (_itemIcon == null) _itemIcon = GetComponent<Image>(); 
        
        _itemData = data;
        _currentDragSource = source;
        OriginalInventoryAnchor = inventoryAnchorIfFromGrid;
        _inventoryManager = inventoryManager;
        gameObject.name = "ItemUI_" + (data != null ? data.ItemName : "NULL_ITEM") + "_Source_" + source;
        
        var spriteToDisplay = data?.Icon;
        
        if (spriteToDisplay == null)
        {
            if(_itemIcon != null) _itemIcon.enabled = false;
        } 
        else 
        {
             if(_itemIcon != null) 
             {
                _itemIcon.sprite = spriteToDisplay;
                _itemIcon.enabled = true;
             }
        }

        if (source == DragSource.ItemsView)
        {
            if (data != null && data.PreviewPreferredWidthInItemsView > 0 && spriteToDisplay != null)
            {
                float targetWidth = data.PreviewPreferredWidthInItemsView;
                
                float aspectRatio = (float)spriteToDisplay.rect.width / spriteToDisplay.rect.height;
                float targetHeight = targetWidth / aspectRatio;
                
                _rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
                
                if (_itemIcon != null && _itemIcon.transform != this.transform && _itemIcon.TryGetComponent<RectTransform>(out RectTransform iconRect))
                {
                    iconRect.anchorMin = Vector2.zero;
                    iconRect.anchorMax = Vector2.one;
                    iconRect.sizeDelta = Vector2.zero; 
                    iconRect.anchoredPosition = Vector2.zero;
                }
            }
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_itemData == null) return;

        CurrentlyDraggedItem = this; 
        
        transform.SetParent(GetComponentInParent<Canvas>().transform, true);
        transform.SetAsLastSibling();

        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = _transparencyOnDrag;

        if (_currentDragSource == DragSource.InventoryGrid)
        {
            _inventoryManager.PickupItemFromInventory(_itemData);
        }
        else if (_currentDragSource == DragSource.ItemsView)
        {
            _inventoryManager.ItemsViewUI.RemoveItem(_itemData, false); 
        }
        
        _inventoryManager.InventoryViewUI.ClearHighlights();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_itemData == null || CurrentlyDraggedItem != this) return;

        _rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;

        PointerEventData pData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pData, results);

        InventoryCellView targetCell = null;
        
        foreach (var result in results)
        {
            targetCell = result.gameObject.GetComponent<InventoryCellView>();
            if (targetCell != null && targetCell.IsDisplayable) break;
        }
        
        _inventoryManager.InventoryViewUI.ClearHighlights();
        
        if (targetCell != null)
        {
            bool canPlace = _inventoryManager.CanPlaceItem(_itemData, targetCell.GridCoordinate);
            List<Vector2Int> cellsToOccupy = _inventoryManager.GetWorldCellsForShape(_itemData, targetCell.GridCoordinate);
            _inventoryManager.InventoryViewUI.HighlightCells(cellsToOccupy, canPlace, targetCell.GridCoordinate);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_itemData == null || CurrentlyDraggedItem != this) return;

        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 1f;
        _inventoryManager.InventoryViewUI.ClearHighlights();

        PointerEventData pData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pData, results);

        InventoryCellView dropCell = null;
        ItemsViewUI dropItemsViewPanel = null; 

        foreach (var result in results)
        {
            if (dropCell == null) dropCell = result.gameObject.GetComponent<InventoryCellView>();
            if (dropItemsViewPanel == null) dropItemsViewPanel = result.gameObject.GetComponentInParent<ItemsViewUI>();
            
            if (dropCell != null && dropCell.IsDisplayable) break; 
        }

        bool placedSuccessfullyInInventory = false;
        bool returnedToItemsView = false;
        
        if (dropCell != null && dropCell.IsDisplayable)
        {
            if (_inventoryManager.TryPlaceItem(_itemData, dropCell.GridCoordinate))
            {
                placedSuccessfullyInInventory = true;
                Destroy(gameObject);
            }
        }
        
        if (!placedSuccessfullyInInventory && dropItemsViewPanel != null)
        {
            _inventoryManager.ItemsViewUI.AddItem(_itemData);
            returnedToItemsView = true;
            Destroy(gameObject);
        }
        
        if (!placedSuccessfullyInInventory && !returnedToItemsView)
        {
            _inventoryManager.ItemsViewUI.AddItem(_itemData);
            Destroy(gameObject);
        }
        
        CurrentlyDraggedItem = null; 
    }
}