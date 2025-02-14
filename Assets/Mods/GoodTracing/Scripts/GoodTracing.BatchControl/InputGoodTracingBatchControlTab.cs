using System.Collections.Generic;
using System.Linq;
using Timberborn.BatchControl;
using Timberborn.Buildings;
using Timberborn.ConstructionSites;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Stockpiles;
using Timberborn.Workshops;
using UnityEngine;

namespace GoodTracing.BatchControl {
  public class InputGoodTracingBatchControlTab : BatchControlTab {

    readonly IGoodService _goodService;
    readonly BatchControlRowGroupFactory _batchControlRowGroupFactory;
    readonly GoodTracingBatchControlRowFactory _goodTracingBatchControlRowFactory;

    readonly SortedSet<GoodAmount> _constructionRequiredGoods = new(new GoodAmountComparer());

    public InputGoodTracingBatchControlTab(VisualElementLoader visualElementLoader,
                                           BatchControlDistrict batchControlDistrict,
                                           IGoodService goodService,
                                           BatchControlRowGroupFactory batchControlRowGroupFactory,
                                           GoodTracingBatchControlRowFactory
                                               goodTracingBatchControlRowFactory
    ) :
        base(visualElementLoader, batchControlDistrict) {
      _goodService = goodService;
      _batchControlRowGroupFactory = batchControlRowGroupFactory;
      _goodTracingBatchControlRowFactory = goodTracingBatchControlRowFactory;
    }

    public override string TabNameLocKey => "sp1um.GoodTracing.InputBatchControlTabName";
    public override string TabImage => "Workplaces"; //TODO use correct image
    public override string BindingKey => "InputGoodTracingTab";
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
                   .Where(e => e.GetComponentFast<Inventories>()?.HasEnabledInventories ?? false)) {
        var inventories = entity.GetComponentFast<Inventories>();
        var manufactory = entity.GetComponentFast<Manufactory>();
        var constructionSite = entity.GetComponentFast<ConstructionSite>();

        foreach (var good in inventories.EnabledInventories
                     .SelectMany(i => i.InputGoods._set).Distinct()) {
          // if the entity is a manufactory, only show goods being consumed by the current recipe
          var isGoodBeingConsumed =
              manufactory == null || IsGoodBeingConsumed(manufactory, good);

          var isGoodUsedInConstruction = constructionSite == null
                                         || IsGoodRequiredByConstructionSite(
                                             constructionSite, good);
          
          if (isGoodBeingConsumed || isGoodUsedInConstruction) {
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

    static bool IsGoodBeingConsumed(Manufactory manufactory, string goodId) {
      if (!manufactory.HasCurrentRecipe) {
        return false;
      }

      var isConsumed = false;
      if (manufactory.CurrentRecipe.ConsumesFuel) {
        var usesFuel = manufactory.CurrentRecipe.Fuel.Id == goodId;
        isConsumed = isConsumed || usesFuel;
      }

      if (manufactory.CurrentRecipe.ConsumesIngredients) {
        var usesIngredient = manufactory.CurrentRecipe.Ingredients.Any(i => i.GoodId == goodId);
        isConsumed = isConsumed || usesIngredient;
      }

      return isConsumed;
    }

    bool IsGoodRequiredByConstructionSite(ConstructionSite constructionSite, string goodId) {
      _constructionRequiredGoods.Clear();
      // TODO not quite sure this should be like this... As soon as capacity is reserved for a good, it would disappear from the list
      // even though the good has not yet been put into the building. Maybe I should just display the required goods (regardless if they've
      // already been put in)?
      constructionSite.RemainingRequiredGoods(_constructionRequiredGoods);
      var isRequired = _constructionRequiredGoods.Any(g => g.GoodId == goodId);
      _constructionRequiredGoods.Clear();
      return isRequired;
    }
  }
}