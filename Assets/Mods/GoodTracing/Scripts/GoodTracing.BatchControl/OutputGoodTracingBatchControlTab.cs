using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.BatchControl;
using Timberborn.CoreUI;
using Timberborn.DistributionSystem;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.SingletonSystem;
using Timberborn.Stockpiles;
using Timberborn.Workshops;
using Timberborn.Yielding;

namespace GoodTracing.BatchControl {
  public class OutputGoodTracingBatchControlTab : GoodTracingBatchControlTab {
    
    readonly HashSet<string> _yields = new();
    readonly ISpecificStockpileDetector _specificStockpileDetector;

    public OutputGoodTracingBatchControlTab(VisualElementLoader visualElementLoader,
                                            BatchControlDistrict batchControlDistrict,
                                            IGoodService goodService,
                                            BatchControlRowGroupFactory batchControlRowGroupFactory,
                                            GoodTracingBatchControlRowFactory
                                                goodTracingBatchControlRowFactory,
                                            EventBus eventBus,
                                            ISpecificStockpileDetector specificStockpileDetector) :
        base(visualElementLoader, batchControlDistrict, goodService, batchControlRowGroupFactory,
             goodTracingBatchControlRowFactory, eventBus) {
      _specificStockpileDetector = specificStockpileDetector;
    }

    public override string TabNameLocKey => "sp1um.GoodTracing.OutputBatchControlTabName";
    public override string TabImage => "OutputBatchControlTab";
    public override string BindingKey => "OutputGoodTracingTab";
    
    protected override bool ShouldAddToRowGroups(EntityComponent entity) {
      // never display stockpiles or district center
      var stockpile = entity.GetComponentFast<Stockpile>();
      var districtCenter = entity.GetComponentFast<DistrictCenter>();
      // TODO decide how to deal with district crossings
      var districtCrossing = entity.GetComponentFast<DistrictCrossing>();
      // nor SpecificStockpiles (from Pantry mod)
      var hasSpecificStockpile = _specificStockpileDetector.HasSpecificStockpile(entity);
      return !stockpile && !districtCenter && !districtCrossing && !hasSpecificStockpile;
    }

    protected override IEnumerable<string> GetGoods(Inventory inventory) {
      return inventory.OutputGoods._set;
    }
    
    protected override bool IsRowVisible(EntityComponent entity, string goodId) {
      var inventories = entity.GetComponentFast<Inventories>();
      if (!inventories.EnabledInventories.Any(i => i.Gives(goodId))) {
        return false;
      }
      
      var manufactory = entity.GetEnabledComponent<Manufactory>();
      var isGoodBeingProduced = manufactory == null || IsGoodBeingProduced(manufactory, goodId);
      
      var inRangeYielders = entity.GetEnabledComponent<InRangeYielders>();
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
             && manufactory.CurrentRecipe.Products.Any(i => i.Id == goodId);
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