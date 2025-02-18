using System.Collections.Generic;
using System.Linq;
using Timberborn.BatchControl;
using Timberborn.CoreUI;
using Timberborn.DistributionSystem;
using Timberborn.EntitySystem;
using Timberborn.Goods;
using Timberborn.InputSystemUI;
using Timberborn.InventorySystem;
using Timberborn.Workshops;
using Timberborn.Yielding;
using UnityEngine;

namespace GoodTracing.BatchControl {
  public class OutputGoodTracingBatchControlTab : GoodTracingBatchControlTab {
    
    readonly HashSet<string> _yields = new();

    public OutputGoodTracingBatchControlTab(VisualElementLoader visualElementLoader,
                                            BatchControlDistrict batchControlDistrict,
                                            IGoodService goodService,
                                            BatchControlRowGroupFactory batchControlRowGroupFactory,
                                            GoodTracingBatchControlRowFactory
                                                goodTracingBatchControlRowFactory,
                                            BindableToggleFactory bindableToggleFactory) :
        base(visualElementLoader, batchControlDistrict, goodService, batchControlRowGroupFactory,
             goodTracingBatchControlRowFactory, bindableToggleFactory) {
    }

    public override string TabNameLocKey => "sp1um.GoodTracing.OutputBatchControlTabName";
    public override string TabImage => "Workplaces"; //TODO use correct image
    public override string BindingKey => "OutputGoodTracingTab";

    protected override bool ShouldDisplayEntity(EntityComponent entity) {
      // TODO decide how to deal with district crossings
      return !entity.GetComponentFast<DistrictCrossing>();
    }

    protected override IEnumerable<string> GetGoods(Inventory inventory) {
      return inventory.OutputGoods._set;
    }

    protected override void AddRows(EntityComponent entity, IEnumerable<string> goods,
                                    IDictionary<string, BatchControlRowGroup> rowGroups,
                                    GoodTracingBatchControlRowFactory rowFactory) {
      var manufactory = entity.GetComponentFast<Manufactory>();
      var inRangeYielders = entity.GetComponentFast<InRangeYielders>();

      foreach (var good in goods) {

        // if the entity is a manufactory, only show goods being produced by the current recipe
        var isGoodBeingProduced = manufactory == null || IsGoodBeingProduced(manufactory, good);
        // if the entity is a yield removing building, only show goods that are actually allowed and in range
        var isGoodObtainable = inRangeYielders == null || IsGoodObtainable(inRangeYielders, good);

        if (isGoodBeingProduced && isGoodObtainable) {
          if (rowGroups.TryGetValue(good, out var group)) {
            group.AddRow(rowFactory.Create(entity));
          } else {
            Debug.LogWarningFormat("[GoodTracing] Unknown good: {0}", good);
          }
        }
      }
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