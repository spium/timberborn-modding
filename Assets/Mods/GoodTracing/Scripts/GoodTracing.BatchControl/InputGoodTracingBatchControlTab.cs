using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.BatchControl;
using Timberborn.ConstructionSites;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.Goods;
using Timberborn.InputSystemUI;
using Timberborn.InventorySystem;
using Timberborn.Workshops;

namespace GoodTracing.BatchControl {
  public class InputGoodTracingBatchControlTab : GoodTracingBatchControlTab {

    readonly SortedSet<GoodAmount> _constructionRequiredGoods = new(new GoodAmountComparer());

    public InputGoodTracingBatchControlTab(VisualElementLoader visualElementLoader,
                                           BatchControlDistrict batchControlDistrict,
                                           IGoodService goodService,
                                           BatchControlRowGroupFactory batchControlRowGroupFactory,
                                           GoodTracingBatchControlRowFactory
                                               goodTracingBatchControlRowFactory,
                                           BindableToggleFactory bindableToggleFactory
    ) :
        base(visualElementLoader, batchControlDistrict, goodService, batchControlRowGroupFactory,
             goodTracingBatchControlRowFactory, bindableToggleFactory) {
      
    }

    public override string TabNameLocKey => "sp1um.GoodTracing.InputBatchControlTabName";
    public override string TabImage => "InputBatchControlTab";
    public override string BindingKey => "InputGoodTracingTab";

    protected override IEnumerable<string> GetGoods(Inventory inventory) {
      return inventory.InputGoods._set;
    }
    
    protected override bool IsRowVisible(EntityComponent entity, string goodId) {
      var manufactory = entity.GetComponentFast<Manufactory>();
      var isGoodBeingConsumed = manufactory == null || IsGoodBeingConsumed(manufactory, goodId);
      
      var constructionSite = entity.GetComponentFast<ConstructionSite>();
      var isGoodUsedInConstruction = constructionSite != null
                                     && IsGoodRequiredByConstructionSite(
                                         constructionSite, goodId);

      return isGoodBeingConsumed || isGoodUsedInConstruction;
    }

    protected override void RegisterListenersToRefreshRowVisibility(EntityComponent entity) {
      var manufactory = entity.GetComponentFast<Manufactory>();
      if (manufactory) {
        manufactory.ProductionRecipeChanged += OnManufactoryProductionRecipeChanged;
      }
      var constructionSite = entity.GetComponentFast<ConstructionSite>();
      if (constructionSite) {
        constructionSite.Inventory.InventoryCapacityReservationChanged += OnConstructionSiteInventoryCapacityReservationChanged;
      }
    }
    
    protected override void UnregisterListenersToRefreshRowVisibility(EntityComponent entity) {
      var manufactory = entity.GetComponentFast<Manufactory>();
      if (manufactory) {
        manufactory.ProductionRecipeChanged -= OnManufactoryProductionRecipeChanged;
      }
      var constructionSite = entity.GetComponentFast<ConstructionSite>();
      if (constructionSite) {
        constructionSite.Inventory.InventoryCapacityReservationChanged -= OnConstructionSiteInventoryCapacityReservationChanged;
      }
    }

    void OnManufactoryProductionRecipeChanged(object sender, EventArgs args) {
      UpdateRowsVisibility();
    }
    
    void OnConstructionSiteInventoryCapacityReservationChanged(object sender, InventoryAmountChangedEventArgs e) {
      UpdateRowsVisibility();
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