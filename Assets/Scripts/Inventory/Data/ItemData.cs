using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New ItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("General Info")]
    public int ItemID;
    public string ItemName = "New Item";
    public Sprite Icon; 

    [Header("Inventory Shape")]
    public Vector2Int Dimensions = new Vector2Int(1, 1); // Width, Height in cells

    // One-dimensional array for storing the shape matrix
    [SerializeField]
    private bool[] ShapeMatrixData = new bool[1] { true }; 
    
    public float PreviewPreferredWidthInItemsView = 0f; 
    
    private bool GetCellState(int localX, int localY)
    {
        if (localX < 0 || localX >= Dimensions.x || localY < 0 || localY >= Dimensions.y)
        {
            return false; 
        }
       
        if (ShapeMatrixData == null || ShapeMatrixData.Length != Dimensions.x * Dimensions.y)
        {
            OnValidate(); 
        }
        
        return ShapeMatrixData != null && ShapeMatrixData[localY * Dimensions.x + localX];
    }
    
    public void SetCellState(int localX, int localY, bool state)
    {
        if (localX < 0 || localX >= Dimensions.x || localY < 0 || localY >= Dimensions.y) return;
        
        if (ShapeMatrixData.Length != Dimensions.x * Dimensions.y)
        {
            System.Array.Resize(ref ShapeMatrixData, Dimensions.x * Dimensions.y);
        }
        
        ShapeMatrixData[localY * Dimensions.x + localX] = state;
    }
    
    public List<Vector2Int> GetShapeOffsets()
    {
        var offsets = new List<Vector2Int>();
        
        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                if (GetCellState(x, y))
                {
                    offsets.Add(new Vector2Int(x, y));
                }
            }
        }
        
        return offsets;
    }
    
    private void OnValidate()
    {
        if (Dimensions.x < 1) Dimensions.x = 1;
        if (Dimensions.y < 1) Dimensions.y = 1;

        int requiredSize = Dimensions.x * Dimensions.y;
        if (ShapeMatrixData == null || ShapeMatrixData.Length != requiredSize)
        {
            bool[] newShape = new bool[requiredSize];
           
            if (ShapeMatrixData != null)
            {
                int oldWidth = (ShapeMatrixData.Length > 0 && Dimensions.y > 0 && ShapeMatrixData.Length % Dimensions.y == 0) ? ShapeMatrixData.Length / Dimensions.y : Dimensions.x;
                if (ShapeMatrixData.Length / Dimensions.y == 0 && Dimensions.x > 0) oldWidth = ShapeMatrixData.Length / Dimensions.x; // crude guess if height was 1
                
                for (int y = 0; y < Dimensions.y; y++)
                {
                    for (int x = 0; x < Dimensions.x; x++)
                    {
                        if (y < (ShapeMatrixData.Length / oldWidth) && x < oldWidth) 
                        {
                             if (y * oldWidth + x < ShapeMatrixData.Length)
                                newShape[y * Dimensions.x + x] = ShapeMatrixData[y * oldWidth + x];
                             else  newShape[y * Dimensions.x + x] = true; 
                        }
                        else
                        {
                            newShape[y * Dimensions.x + x] = true; 
                        }
                    }
                }
            }
            else
            {
                 for(int i=0; i < newShape.Length; ++i) newShape[i] = true; 
            }
            
            ShapeMatrixData = newShape;
        }
    }
}
