using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] private Button _showDefaultInventory;
    [SerializeField] private Button _showCustomInventory;
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private GameObject _customInventoryPanel;

    private void Awake()
    {
        _inventoryPanel.SetActive(true);
        _customInventoryPanel.SetActive(false);
        
        _showDefaultInventory.onClick.AddListener(()=>{_inventoryPanel.SetActive(true); 
            _customInventoryPanel.SetActive(false);});
        _showCustomInventory.onClick.AddListener(()=>{_inventoryPanel.SetActive(false); 
            _customInventoryPanel.SetActive(true); });
    }
}
