using Timberborn.BatchControl;
using Timberborn.Common;
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
      goodElement.Q<Label>("CapacityAmount").text = inventory.AmountInStock(goodId).ToString();
      // do not use LimitedAmount() to initialize capacity limit, because when the row is being created it's possible the good
      // is being disallowed, which results in LimitedAmount() to return 0.
      goodElement.Q<Label>("CapacityLimit").text =
          inventory._allowedGoods.GetAmount(goodId).ToString();
      _inventoryCapacityBatchControlRowItemFactory.InitializeTooltip(goodElement, describedGood);
      inventoryWrapper.Add(goodElement);

      var goodItem =
          new InventoryCapacityBatchControlGood(goodElement.Q<Label>("CapacityAmount"), inventory,
                                                goodId);

      return new InventoryCapacityBatchControlRowItem(root, Enumerables.One(goodItem));
    }

  }
}