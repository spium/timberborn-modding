using System.Collections.Generic;
using System.Linq;
using Timberborn.BatchControl;
using Timberborn.ConstructionSites;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Workshops;
using UnityEngine;

namespace GoodTracing.BatchControl {
  public class InputGoodTracingBatchControlTab : GoodTracingBatchControlTab {

    readonly SortedSet<GoodAmount> _constructionRequiredGoods = new(new GoodAmountComparer());

    public InputGoodTracingBatchControlTab(VisualElementLoader visualElementLoader,
                                           BatchControlDistrict batchControlDistrict,
                                           IGoodService goodService,
                                           BatchControlRowGroupFactory batchControlRowGroupFactory,
                                           GoodTracingBatchControlRowFactory
                                               goodTracingBatchControlRowFactory
    ) :
        base(visualElementLoader, batchControlDistrict, goodService, batchControlRowGroupFactory,
             goodTracingBatchControlRowFactory) {
    }

    public override string TabNameLocKey => "sp1um.GoodTracing.InputBatchControlTabName";
    public override string TabImage => "Workplaces"; //TODO use correct image
    public override string BindingKey => "InputGoodTracingTab";

    protected override IEnumerable<string> GetGoods(Inventory inventory) {
      return inventory.InputGoods._set;
    }

    protected override void AddRows(EntityComponent entity, IEnumerable<string> goods, IDictionary<string, BatchControlRowGroup> rowGroups,
                                    GoodTracingBatchControlRowFactory rowFactory) {
      
      var manufactory = entity.GetComponentFast<Manufactory>();
      var constructionSite = entity.GetComponentFast<ConstructionSite>();

      foreach (var good in goods) {
        // if the entity is a manufactory, only show goods being consumed by the current recipe
        var isGoodBeingConsumed =
            manufactory == null || IsGoodBeingConsumed(manufactory, good);

        var isGoodUsedInConstruction = constructionSite == null
                                       || IsGoodRequiredByConstructionSite(
                                           constructionSite, good);
          
        if (isGoodBeingConsumed || isGoodUsedInConstruction) {
          if (rowGroups.TryGetValue(good, out var group)) {
            group.AddRow(rowFactory.Create(entity));
          } else {
            Debug.LogWarningFormat("[GoodTracing] Unknown good: {0}", good);
          }
        }
      }
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