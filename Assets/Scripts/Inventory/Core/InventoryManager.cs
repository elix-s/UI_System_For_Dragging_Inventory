using UnityEngine;
using System.Collections.Generic;
using System; 

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Configuration for default layout")]
    [SerializeField] private int _inventoryRows = 10;
    [SerializeField] private int _inventoryColumns = 10;

    [Header("References")]
    [SerializeField] private InventoryViewUI _inventoryViewUI; 
    [SerializeField] private ItemsViewUI _itemsViewUI;

    [Header("Type")] 
    [SerializeField] private bool _defaultInventory;

    private InventorySlot[,] _inventoryGrid;
    private Dictionary<ItemData, Vector2Int> _placedItems = new Dictionary<ItemData, Vector2Int>();
    
    public event Action<ItemData, Vector2Int> OnItemPlacedInInventory; 
    public event Action<ItemData, Vector2Int> OnItemRemovedFromInventory; 
    
    public InventoryViewUI InventoryViewUI => _inventoryViewUI;
    public ItemsViewUI ItemsViewUI => _itemsViewUI;

    private void Awake()
    {
        _itemsViewUI.Init(this);
        _inventoryViewUI.Init(this);

        if (_defaultInventory)
        {
            InitializeGrid(_inventoryRows, _inventoryColumns, (r, c) => true);
        }
        else
        {
            //example of initialization of non-standard shaped inventory
            int[] shape = new int[] {
                1, 1, 1, 1, 1, 1,1,1,1,
                0, 1, 0, 0, 1, 1,1,1,1,
                0, 1, 0, 0, 0, 1,1,1,1,
                0, 1, 0, 0, 0, 1,1,1,1,
                0, 1, 0, 0, 0, 1,1,1,1,
                0, 1, 1, 0, 1, 1,1,1,1,
                0, 1, 1, 0, 1, 1,1,1,1,
                0, 1, 1, 0, 1, 1,1,1,1,
            };

            InitializeCustomGrid(8,9,shape);
        }
    }

    private void InitializeGrid(int rows, int cols, Func<int, int, bool> displayCellCondition)
    {
        _inventoryRows = rows;
        _inventoryColumns = cols;
        _inventoryGrid = new InventorySlot[rows, cols];
        _placedItems.Clear();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                _inventoryGrid[r, c] = new InventorySlot(displayCellCondition(r, c));
            }
        }

        _inventoryViewUI?.FillInventory(rows, cols, displayCellCondition);
    }
    
    private void InitializeCustomGrid(int rows, int cols, int[] shapeArray)
    {
        if (shapeArray == null || shapeArray.Length != rows * cols)
        {
            Debug.LogError($"Size of shapeArray ({shapeArray?.Length}) does not match rows*cols ({rows * cols}).");
            return;
        }

        Func<int, int, bool> displayCellCondition = (r, c) =>
        {
            int index = r * cols + c; 
            return shapeArray[index] == 1;
        };

        InitializeGrid(rows, cols, displayCellCondition);
    }
    
    public List<Vector2Int> GetWorldCellsForShape(ItemData item, Vector2Int anchorGridPos)
    {
        var worldCells = new List<Vector2Int>();
        if (item == null) return worldCells;

        foreach (var offset in item.GetShapeOffsets())
        {
            worldCells.Add(anchorGridPos + offset);
        }
        
        return worldCells;
    }

    public bool CanPlaceItem(ItemData item, Vector2Int anchorGridPos)
    {
        if (item == null) return false;
        List<Vector2Int> cellsToOccupy = GetWorldCellsForShape(item, anchorGridPos);

        if (cellsToOccupy.Count == 0) return false; 

        foreach (var cellPos in cellsToOccupy)
        {
            if (cellPos.x < 0 || cellPos.x >= _inventoryColumns ||
                cellPos.y < 0 || cellPos.y >= _inventoryRows)
            {
                return false; 
            }
            
            InventorySlot slot = _inventoryGrid[cellPos.y, cellPos.x];
            
            if (!slot.IsDisplayable || (slot.IsOccupied && slot.OccupyingItem != item))
            {
                return false; 
            }
        }
        
        return true;
    }
    
    public bool TryPlaceItem(ItemData item, Vector2Int anchorGridPos)
    {
        if (!CanPlaceItem(item, anchorGridPos))
        {
            return false;
        }
        
        if (_placedItems.ContainsKey(item))
        {
            RemoveItem(item, false); 
        }

        List<Vector2Int> cellsToOccupy = GetWorldCellsForShape(item, anchorGridPos);
        
        foreach (var cellPos in cellsToOccupy)
        {
            _inventoryGrid[cellPos.y, cellPos.x].PlaceItem(item, anchorGridPos);
        }
        
        _placedItems[item] = anchorGridPos;
        OnItemPlacedInInventory?.Invoke(item, anchorGridPos);
        return true;
    }
    
    private ItemData RemoveItem(ItemData item, bool addToItemsViewOnRemove = true)
    {
        if (item == null || !_placedItems.TryGetValue(item, out var anchorPos))
        {
            return null; 
        }

        List<Vector2Int> cellsOccupied = GetWorldCellsForShape(item, anchorPos);
        
        foreach (var cellPos in cellsOccupied)
        {
            if (cellPos.x >= 0 && cellPos.x < _inventoryColumns &&
                cellPos.y >= 0 && cellPos.y < _inventoryRows) 
            {
                _inventoryGrid[cellPos.y, cellPos.x].ClearItem();
            }
        }
        
        _placedItems.Remove(item);
        OnItemRemovedFromInventory?.Invoke(item, anchorPos);

        if (addToItemsViewOnRemove && _itemsViewUI != null)
        {
            _itemsViewUI.AddItem(item);
        }
        
        return item;
    }
    
    public ItemData PickupItemFromInventory(ItemData item)
    {
        return RemoveItem(item, false); 
    }
    
    public InventorySlot GetSlot(int r, int c)
    {
        if (r < 0 || r >= _inventoryRows || c < 0 || c >= _inventoryColumns) return null;
        return _inventoryGrid[r,c];
    }

    public bool IsGridPositionValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _inventoryColumns && pos.y >= 0 && pos.y < _inventoryRows;
    }
}