using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.BatchControl;
using Timberborn.CoreUI;
using Timberborn.DistributionSystem;
using Timberborn.EntitySystem;
using Timberborn.Goods;
using Timberborn.InputSystemUI;
using Timberborn.InventorySystem;
using Timberborn.SingletonSystem;
using Timberborn.Workshops;
using Timberborn.Yielding;

namespace GoodTracing.BatchControl {
  public class OutputGoodTracingBatchControlTab : GoodTracingBatchControlTab {
    
    readonly HashSet<string> _yields = new();

    public OutputGoodTracingBatchControlTab(VisualElementLoader visualElementLoader,
                                            BatchControlDistrict batchControlDistrict,
                                            IGoodService goodService,
                                            BatchControlRowGroupFactory batchControlRowGroupFactory,
                                            GoodTracingBatchControlRowFactory
                                                goodTracingBatchControlRowFactory,
                                            BindableToggleFactory bindableToggleFactory,
                                            EventBus eventBus) :
        base(visualElementLoader, batchControlDistrict, goodService, batchControlRowGroupFactory,
             goodTracingBatchControlRowFactory, bindableToggleFactory, eventBus) {
    }

    public override string TabNameLocKey => "sp1um.GoodTracing.OutputBatchControlTabName";
    public override string TabImage => "OutputBatchControlTab";
    public override string BindingKey => "OutputGoodTracingTab";

    protected override bool ShouldDisplayEntity(EntityComponent entity) {
      // TODO decide how to deal with district crossings
      return !entity.GetComponentFast<DistrictCrossing>();
    }

    protected override IEnumerable<string> GetGoods(Inventory inventory) {
      return inventory.OutputGoods._set;
    }
    
    protected override bool IsRowVisible(EntityComponent entity, string goodId) {
      var manufactory = entity.GetComponentFast<Manufactory>();
      var isGoodBeingProduced = manufactory == null || IsGoodBeingProduced(manufactory, goodId);
      
      var inRangeYielders = entity.GetComponentFast<InRangeYielders>();
      var isGoodObtainable = inRangeYielders == null || IsGoodObtainable(inRangeYielders, goodId);
      return isGoodBeingProduced && isGoodObtainable;
    }

    protected override void RegisterListenersToRefreshRowVisibility(EntityComponent entity) {
      var manufactory = entity.GetComponentFast<Manufactory>();
      if (manufactory) {
        manufactory.ProductionRecipeChanged += OnManufactoryProductionRecipeChanged;
      }
    }
    
    protected override void UnregisterListenersToRefreshRowVisibility(EntityComponent entity) {
      var manufactory = entity.GetComponentFast<Manufactory>();
      if (manufactory) {
        manufactory.ProductionRecipeChanged -= OnManufactoryProductionRecipeChanged;
      }
    }
    
    void OnManufactoryProductionRecipeChanged(object sender, EventArgs e) {
      UpdateRowsVisibility();
    }

    static bool IsGoodBeingProduced(Manufactory manufactory, string goodId) {
      if (!manufactory.HasCurrentRecipe) {
        return false;
      }

      return manufactory.CurrentRecipe.ProducesProducts
             && manufactory.CurrentRecipe.Products.Any(i => i.GoodId == goodId);
    }

    bool IsGoodObtainable(InRangeYielders inRangeYielders, string goodId) {
      if (!inRangeYielders._yieldRemovingBuilding.GetAllowedGoods().Contains(goodId)) {
        return false;
      }
      _yields.Clear();
      inRangeYielders.GetYields(_yields);
      var inRange = _yields.Contains(goodId);
      _yields.Clear();
      return inRange;
    }

  }
}