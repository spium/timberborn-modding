using System.Collections.Generic;
using System.Linq;
using Timberborn.BatchControl;
using Timberborn.Buildings;
using Timberborn.CoreUI;
using Timberborn.DistributionSystem;
using Timberborn.EntitySystem;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Stockpiles;
using Timberborn.Workshops;
using Timberborn.Yielding;
using UnityEngine;

namespace GoodTracing.BatchControl {
  public class OutputGoodTracingBatchControlTab : BatchControlTab {

    readonly IGoodService _goodService;
    readonly BatchControlRowGroupFactory _batchControlRowGroupFactory;
    readonly GoodTracingBatchControlRowFactory _goodTracingBatchControlRowFactory;
    readonly HashSet<string> _yields = new();

    public OutputGoodTracingBatchControlTab(VisualElementLoader visualElementLoader,
                                            BatchControlDistrict batchControlDistrict,
                                            IGoodService goodService,
                                            BatchControlRowGroupFactory batchControlRowGroupFactory,
                                            GoodTracingBatchControlRowFactory
                                                goodTracingBatchControlRowFactory) :
        base(visualElementLoader, batchControlDistrict) {
      _goodService = goodService;
      _batchControlRowGroupFactory = batchControlRowGroupFactory;
      _goodTracingBatchControlRowFactory = goodTracingBatchControlRowFactory;
    }

    public override string TabNameLocKey => "sp1um.GoodTracing.OutputBatchControlTabName";
    public override string TabImage => "Workplaces"; //TODO use correct image
    public override string BindingKey => "OutputGoodTracingTab";
    public override bool RemoveEmptyRowGroups => true;

    public override IEnumerable<BatchControlRowGroup> GetRowGroups(
        IEnumerable<EntityComponent> entities) {
      
      var groups = _goodService.Goods.ToDictionary(goodId => goodId,
                                                   goodId => {
                                                     var good = _goodService.GetGood(goodId);
                                                     return _batchControlRowGroupFactory
                                                         .Create(good.PluralDisplayName,
                                                                 good.PluralDisplayName);
                                                   });

      foreach (var entity in entities.Where(e => e.GetComponentFast<Building>())
                   .Where(e => !e.GetComponentFast<Stockpile>())
                   // TODO decide how to deal with district crossings
                   .Where(e => !e.GetComponentFast<DistrictCrossing>())
                   .Where(e => e.GetComponentFast<Inventories>()?.HasEnabledInventories ?? false)) {
        var inventories = entity.GetComponentFast<Inventories>();
        var manufactory = entity.GetComponentFast<Manufactory>();
        var inRangeYielders = entity.GetComponentFast<InRangeYielders>();
        

        foreach (var good in inventories.EnabledInventories
                     .SelectMany(i => i.OutputGoods._set).Distinct()) {
          
          // if the entity is a manufactory, only show goods being produced by the current recipe
          var isGoodBeingProduced = manufactory == null || IsGoodBeingProduced(manufactory, good);
          // if the entity is a yield removing building, only show goods that are actually allowed and in range
          var isGoodObtainable = inRangeYielders == null || IsGoodObtainable(inRangeYielders, good);
          
          if (isGoodBeingProduced && isGoodObtainable) {
            if (groups.TryGetValue(good, out var group)) {
              group.AddRow(_goodTracingBatchControlRowFactory.Create(entity));
            } else {
              Debug.LogWarningFormat("[GoodTracing] Unknown good: {0}", good);
            }
          }
        }
      }

      return groups.Values;
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