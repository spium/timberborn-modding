using Timberborn.BatchControl;
using Timberborn.EntitySystem;
using Timberborn.GoodsUI;
using Timberborn.InventorySystem;
using Timberborn.InventorySystemBatchControl;
using UnityEngine.UIElements;

namespace GoodTracing.BatchControl {
  public class GoodTracingInventoryCapacityBatchControlRowItemFactory {

    readonly InventoryCapacityBatchControlRowItemFactory
        _inventoryCapacityBatchControlRowItemFactory;
    readonly GoodDescriber _goodDescriber;

    public GoodTracingInventoryCapacityBatchControlRowItemFactory(
        InventoryCapacityBatchControlRowItemFactory inventoryCapacityBatchControlRowItemFactory,
        GoodDescriber goodDescriber) {
      _inventoryCapacityBatchControlRowItemFactory = inventoryCapacityBatchControlRowItemFactory;
      _goodDescriber = goodDescriber;
    }

    public IBatchControlRowItem Create(EntityComponent entity, Inventory inventory, string goodId) {
      var root = _inventoryCapacityBatchControlRowItemFactory.CreateRoot();
      var inventoryWrapper = root.Q<VisualElement>("InventoryWrapper");
      var describedGood = _goodDescriber.GetDescribedGood(goodId);
      var goodElement = _inventoryCapacityBatchControlRowItemFactory.CreateGoodElement();
      InventoryCapacityBatchControlRowItemFactory.InitializeIcon(goodElement, describedGood);
      _inventoryCapacityBatchControlRowItemFactory.InitializeTooltip(goodElement, describedGood);
      inventoryWrapper.Add(goodElement);

      var capacityAmountLabel = goodElement.Q<Label>("CapacityAmount");
      var capacityLimitLabel = goodElement.Q<Label>("CapacityLimit");
      return new GoodTracingInventoryCapacityBatchControlRowItem(
          root, capacityAmountLabel, capacityLimitLabel, inventory, goodId);
    }

  }
}