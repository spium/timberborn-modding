using Timberborn.BatchControl;
using Timberborn.InventorySystem;
using UnityEngine.UIElements;

namespace GoodTracing.BatchControl {
  public class GoodTracingInventoryCapacityBatchControlRowItem : 
      IBatchControlRowItem,
      IUpdateableBatchControlRowItem
  {

    readonly Label _capacityAmount, _capacityLimit;
    readonly Inventory _inventory;
    readonly string _goodId;
    
    public VisualElement Root { get; }

    public GoodTracingInventoryCapacityBatchControlRowItem(
        VisualElement root,
        Label capacityAmount, Label capacityLimit, Inventory inventory, string goodId)
    {
      Root = root;
      _capacityAmount = capacityAmount;
      _capacityLimit = capacityLimit;
      _inventory = inventory;
      _goodId = goodId;
    }

    public void UpdateRowItem()
    {
      _capacityAmount.text = _inventory.AmountInStock(_goodId).ToString();
      _capacityLimit.text = _inventory.LimitedAmount(_goodId).ToString();
    }
  }
}