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
using Timberborn.SingletonSystem;
using Timberborn.Workshops;

namespace GoodTracing.BatchControl {
  public class InputGoodTracingBatchControlTab : GoodTracingBatchControlTab {
    public InputGoodTracingBatchControlTab(VisualElementLoader visualElementLoader,
                                           BatchControlDistrict batchControlDistrict,
                                           IGoodService goodService,
                                           BatchControlRowGroupFactory batchControlRowGroupFactory,
                                           GoodTracingBatchControlRowFactory
                                               goodTracingBatchControlRowFactory,
                                           BindableToggleFactory bindableToggleFactory,
                                           EventBus eventBus
    ) :
        base(visualElementLoader, batchControlDistrict, goodService, batchControlRowGroupFactory,
             goodTracingBatchControlRowFactory, bindableToggleFactory, eventBus) {
      
    }

    public override string TabNameLocKey => "sp1um.GoodTracing.InputBatchControlTabName";
    public override string TabImage => "InputBatchControlTab";
    public override string BindingKey => "InputGoodTracingTab";

    protected override IEnumerable<string> GetGoods(Inventory inventory) {
      return inventory.InputGoods._set;
    }
    
    protected override bool IsRowVisible(EntityComponent entity, string goodId) {
      var inventories = entity.GetComponentFast<Inventories>();
      if (!inventories.EnabledInventories.Any(i => i.Takes(goodId))) {
        return false;
      }
      
      var constructionSite = entity.GetComponentFast<ConstructionSite>();
      var isGoodUsedInConstruction = constructionSite != null
                                     && IsGoodRequiredByConstructionSite(
                                         constructionSite, goodId);
      // early exit if construction is still in progress
      if (isGoodUsedInConstruction) {
        return true;
      }
      
      var manufactory = entity.GetComponentFast<Manufactory>();
      var isGoodBeingConsumed = manufactory == null || IsGoodBeingConsumed(manufactory, goodId);

      return isGoodBeingConsumed;
    }

    protected override void RegisterListenersToRefreshRowVisibility(EntityComponent entity) {
      var manufactory = entity.GetComponentFast<Manufactory>();
      if (manufactory) {
        manufactory.ProductionRecipeChanged += OnManufactoryProductionRecipeChanged;
      }
      var constructionSite = entity.GetComponentFast<ConstructionSite>();
      if (constructionSite) {
        constructionSite.OnConstructionSiteProgressed += OnConstructionSiteProgressed;
        constructionSite.Inventory.InventoryStockChanged += OnConstructionSiteInventoryStockChanged;
      }
    }

    protected override void UnregisterListenersToRefreshRowVisibility(EntityComponent entity) {
      var manufactory = entity.GetComponentFast<Manufactory>();
      if (manufactory) {
        manufactory.ProductionRecipeChanged -= OnManufactoryProductionRecipeChanged;
      }
      var constructionSite = entity.GetComponentFast<ConstructionSite>();
      if (constructionSite) {
        constructionSite.OnConstructionSiteProgressed -= OnConstructionSiteProgressed;
        constructionSite.Inventory.InventoryStockChanged -= OnConstructionSiteInventoryStockChanged;
      }
    }

    void OnManufactoryProductionRecipeChanged(object sender, EventArgs args) {
      UpdateRowsVisibility();
    }
    
    void OnConstructionSiteInventoryStockChanged(object sender, InventoryAmountChangedEventArgs e) {
      var inventory = (Inventory) sender;
      if (inventory.AmountInStock(e.GoodAmount.GoodId) >= inventory.LimitedAmount(e.GoodAmount.GoodId)) {
        UpdateRowsVisibility();
      }
    }
    
    void OnConstructionSiteProgressed(object sender, EventArgs e) {
      var constructionSite = (ConstructionSite) sender;
      if (constructionSite.BuildTimeProgress >= 1f) {
        UpdateRowsVisibility();
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

    static bool IsGoodRequiredByConstructionSite(ConstructionSite constructionSite, string goodId) {
      if (constructionSite.BuildTimeProgress >= 1f) {
        return false;
      }
      if (!constructionSite.Inventory.Takes(goodId)) {
        return false;
      }

      var needed = constructionSite.Inventory.LimitedAmount(goodId);
      var obtained = constructionSite.Inventory.AmountInStock(goodId);
      return obtained < needed;
    }
  }
}