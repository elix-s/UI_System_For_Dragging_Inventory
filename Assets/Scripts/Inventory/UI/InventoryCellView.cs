using UnityEngine;
using UnityEngine.UI;

public class InventoryCellView : MonoBehaviour
{
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _itemIconDisplay; 

    [Header("Highlight Colors")]
    [SerializeField] private Color _defaultColor = Color.blue;
    [SerializeField] private Color _highlightValidColor = Color.green;
    [SerializeField] private Color _highlightInvalidColor = Color.red;
    [SerializeField] private Color _placeholderColor = new Color(0.3f, 0.3f, 0.3f, 0.5f); 

    public Vector2Int GridCoordinate { get; private set; }
    public bool IsDisplayable { get; private set; } 
    
    public void Initialize(InventoryViewUI owner, int r, int c, bool displayInInventory)
    {
        if (_backgroundImage == null) _backgroundImage = GetComponent<Image>();
        if (_itemIconDisplay != null) _itemIconDisplay.enabled = false; 
        
        GridCoordinate = new Vector2Int(c, r); 
        IsDisplayable = displayInInventory;
        gameObject.name = $"Cell_{r}_{c} ({(IsDisplayable ? "Active" : "Placeholder")})";

        if (_backgroundImage != null)
        {
            _backgroundImage.color = IsDisplayable ? _defaultColor : _placeholderColor;
        }
        
        var canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null && !IsDisplayable) {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        if (!IsDisplayable && canvasGroup != null)
        {
            canvasGroup.alpha = 0.3f; 
            canvasGroup.blocksRaycasts = false; 
        }
    }

    public void SetDisplayedItem(ItemData itemData)
    {
        if (_itemIconDisplay != null && IsDisplayable)
        {
            if (itemData != null && itemData.Icon != null)
            {
                _itemIconDisplay.sprite = itemData.Icon;
                _itemIconDisplay.enabled = true;
            }
            else
            {
                _itemIconDisplay.enabled = false;
            }
        }
    }

    public void ClearDisplayedItem()
    {
        if (_itemIconDisplay != null)
        {
            _itemIconDisplay.sprite = null;
            _itemIconDisplay.enabled = false;
        }
    }

    public void Highlight(bool isValidPlacement)
    {
        if (!IsDisplayable || _backgroundImage == null) return;
        _backgroundImage.color = isValidPlacement ? _highlightValidColor : _highlightInvalidColor;
    }

    public void ResetHighlight()
    {
        if (!IsDisplayable || _backgroundImage == null) return;
        _backgroundImage.color = _defaultColor;
    }
}