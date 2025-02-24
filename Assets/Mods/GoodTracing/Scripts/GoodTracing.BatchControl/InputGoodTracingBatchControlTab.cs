using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.BatchControl;
using Timberborn.BlockSystem;
using Timberborn.ConstructionSites;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.SingletonSystem;
using Timberborn.Stockpiles;
using Timberborn.Workshops;
using UnityEngine.UIElements;

namespace GoodTracing.BatchControl {
  public class InputGoodTracingBatchControlTab : GoodTracingBatchControlTab {
    readonly ISpecificStockpileDetector _specificStockpileDetector;

    bool _showConstruction;
    
    public InputGoodTracingBatchControlTab(VisualElementLoader visualElementLoader,
                                           BatchControlDistrict batchControlDistrict,
                                           IGoodService goodService,
                                           BatchControlRowGroupFactory batchControlRowGroupFactory,
                                           GoodTracingBatchControlRowFactory
                                               goodTracingBatchControlRowFactory,
                                           EventBus eventBus,
                                           ISpecificStockpileDetector specificStockpileDetector
    ) :
        base(visualElementLoader, batchControlDistrict, goodService, batchControlRowGroupFactory,
             goodTracingBatchControlRowFactory, eventBus) {
      _specificStockpileDetector = specificStockpileDetector;
    }

    public override string TabNameLocKey => "sp1um.GoodTracing.InputBatchControlTabName";
    public override string TabImage => "InputBatchControlTab";
    public override string BindingKey => "InputGoodTracingTab";

    public override VisualElement GetHeader() {
      var header = base.GetHeader();
      var showConstructionToggle = header.Q<Toggle>("ShowConstruction");
      showConstructionToggle.value = _showConstruction;
      showConstructionToggle.RegisterValueChangedCallback(ShowConstructionSitesToggled);
      showConstructionToggle.ToggleDisplayStyle(true);
      return header;
    }

    protected override IEnumerable<string> GetGoods(Inventory inventory) {
      return inventory.InputGoods._set;
    }

    protected override bool ShouldAddToRowGroups(EntityComponent entity) {
      var blockState = entity.GetComponentFast<BlockObjectState>();
      if (!blockState) {
        return false;
      }
      
      if (!blockState.IsFinished) {
        // always add buildings under construction, as long as they still require some goods
        var constructionSite = entity.GetEnabledComponent<ConstructionSite>();
        if (constructionSite && !constructionSite.Inventory.IsFull) {
          return true;
        }
      }

      // otherwise don't display stockpiles
      var stockpile = entity.GetComponentFast<Stockpile>();
      // nor SpecificStockpiles (from Pantry mod)
      var hasSpecificStockpile = _specificStockpileDetector.HasSpecificStockpile(entity);
      return !stockpile && !hasSpecificStockpile;
    }

    protected override bool IsRowVisible(EntityComponent entity, string goodId) {
      var inventories = entity.GetComponentFast<Inventories>();
      if (!inventories.EnabledInventories.Any(i => i.Takes(goodId))) {
        return false;
      }

      var blockState = entity.GetComponentFast<BlockObjectState>();
      if (!blockState.IsFinished) {
        if (!_showConstruction) {
          return false;
        }
        // if it's still under construction, show the row only if good is being used for construction
        var constructionSite = entity.GetEnabledComponent<ConstructionSite>();
        return constructionSite != null && IsGoodRequiredByConstructionSite(constructionSite, goodId);
      }
      
      var manufactory = entity.GetEnabledComponent<Manufactory>();
      return manufactory == null || IsGoodBeingConsumed(manufactory, goodId);
    }

    protected override void RegisterListenersToRefreshRowVisibility(EntityComponent entity) {
      var manufactory = entity.GetComponentFast<Manufactory>();
      if (manufactory) {
        manufactory.ProductionRecipeChanged += OnManufactoryProductionRecipeChanged;
      }
      var constructionSite = entity.GetComponentFast<ConstructionSite>();
      if (constructionSite) {
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
        constructionSite.Inventory.InventoryStockChanged -= OnConstructionSiteInventoryStockChanged;
      }
    }

    void OnManufactoryProductionRecipeChanged(object sender, EventArgs args) {
      UpdateRowsVisibility();
    }

    void OnConstructionSiteInventoryStockChanged(object sender, InventoryAmountChangedEventArgs e) {
      // refresh when construction inventory for a specific good has been completely filled (the row should disappear as the good is no longer consumed)
      var inventory = (Inventory) sender;
      var inStock = inventory.AmountInStock(e.GoodAmount.GoodId);
      var needed = inventory.LimitedAmount(e.GoodAmount.GoodId);
      if (inStock >= needed) {
        UpdateRowsVisibility();
      }
    }
    
    void ShowConstructionSitesToggled(ChangeEvent<bool> evt) {
      _showConstruction = evt.newValue;
      UpdateRowsVisibility();
    }

    static bool IsGoodBeingConsumed(Manufactory manufactory, string goodId) {
      if (!manufactory.HasCurrentRecipe) {
        return false;
      }

      var isConsumed = false;
      if (manufactory.CurrentRecipe.ConsumesFuel) {
        var usesFuel = manufactory.CurrentRecipe.Fuel == goodId;
        isConsumed = isConsumed || usesFuel;
      }

      if (manufactory.CurrentRecipe.ConsumesIngredients) {
        var usesIngredient = manufactory.CurrentRecipe.Ingredients.Any(i => i.Id == goodId);
        isConsumed = isConsumed || usesIngredient;
      }

      return isConsumed;
    }

    static bool IsGoodRequiredByConstructionSite(ConstructionSite constructionSite, string goodId) {
      if (constructionSite.Inventory.IsFull) {
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