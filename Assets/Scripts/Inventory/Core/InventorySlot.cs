using UnityEngine;

public class InventorySlot
{
    public ItemData OccupyingItem { get; private set; }
    public Vector2Int ItemAnchorCoordinate { get; private set; }
    public bool IsOccupied => OccupyingItem != null;
    public bool IsDisplayable { get; } 

    public InventorySlot(bool isDisplayable)
    {
        IsDisplayable = isDisplayable;
        OccupyingItem = null;
    }

    public void PlaceItem(ItemData item, Vector2Int anchorCoordOfItem)
    {
        OccupyingItem = item;
        ItemAnchorCoordinate = anchorCoordOfItem;
    }

    public void ClearItem()
    {
        OccupyingItem = null;
    }
}