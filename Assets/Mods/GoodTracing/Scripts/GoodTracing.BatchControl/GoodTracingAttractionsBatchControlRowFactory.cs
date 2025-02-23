using System;
using Timberborn.AttractionsUI;
using Timberborn.BatchControl;
using Timberborn.BuildingsUI;
using Timberborn.ConstructionSitesUI;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.HaulingUI;
using Timberborn.InventorySystem;
using Timberborn.InventorySystemBatchControl;
using Timberborn.StatusSystemUI;

namespace GoodTracing.BatchControl {
  /**
   * Just a copy of Timberborn.AttractionsBatchControl.AttractionsBatchControlRowFactory that also adds the visibility getter to the rows it creates
   */
  public class GoodTracingAttractionsBatchControlRowFactory {

    readonly VisualElementLoader _visualElementLoader;
    readonly BuildingBatchControlRowItemFactory _buildingBatchControlRowItemFactory;
    readonly StatusBatchControlRowItemFactory _statusBatchControlRowItemFactory;
    readonly ConstructionSitePriorityBatchControlRowItemFactory
        _constructionSitePriorityBatchControlRowItemFactory;
    readonly AttractionBatchControlRowItemFactory _attractionBatchControlRowItemFactory;
    readonly AttractionLoadRateBatchControlRowItemFactory
        _attractionLoadRateBatchControlRowItemFactory;
    readonly GoodTracingInventoryCapacityBatchControlRowItemFactory
        _inventoryCapacityBatchControlRowItemFactory;
    readonly HaulCandidateBatchControlRowItemFactory _haulCandidateBatchControlRowItemFactory;

    public GoodTracingAttractionsBatchControlRowFactory(
        VisualElementLoader visualElementLoader,
        BuildingBatchControlRowItemFactory buildingBatchControlRowItemFactory,
        StatusBatchControlRowItemFactory statusBatchControlRowItemFactory,
        ConstructionSitePriorityBatchControlRowItemFactory
            constructionSitePriorityBatchControlRowItemFactory,
        AttractionBatchControlRowItemFactory attractionBatchControlRowItemFactory,
        AttractionLoadRateBatchControlRowItemFactory attractionLoadRateBatchControlRowItemFactory,
        GoodTracingInventoryCapacityBatchControlRowItemFactory
            inventoryCapacityBatchControlRowItemFactory,
        HaulCandidateBatchControlRowItemFactory haulCandidateBatchControlRowItemFactory) {
      _visualElementLoader = visualElementLoader;
      _buildingBatchControlRowItemFactory = buildingBatchControlRowItemFactory;
      _statusBatchControlRowItemFactory = statusBatchControlRowItemFactory;
      _constructionSitePriorityBatchControlRowItemFactory =
          constructionSitePriorityBatchControlRowItemFactory;
      _attractionBatchControlRowItemFactory = attractionBatchControlRowItemFactory;
      _attractionLoadRateBatchControlRowItemFactory = attractionLoadRateBatchControlRowItemFactory;
      _inventoryCapacityBatchControlRowItemFactory = inventoryCapacityBatchControlRowItemFactory;
      _haulCandidateBatchControlRowItemFactory = haulCandidateBatchControlRowItemFactory;
    }

    public BatchControlRow Create(EntityComponent entity, Inventory inventory, string goodId,
                                  Func<bool> visibilityGetter) {
      return new(_visualElementLoader.LoadVisualElement("Game/BatchControl/BatchControlRow"),
                 entity, visibilityGetter, _buildingBatchControlRowItemFactory.Create(entity),
                 _inventoryCapacityBatchControlRowItemFactory.Create(entity, inventory, goodId),
                 _attractionBatchControlRowItemFactory.Create(entity),
                 _attractionLoadRateBatchControlRowItemFactory.Create(entity),
                 _haulCandidateBatchControlRowItemFactory.Create(entity),
                 _constructionSitePriorityBatchControlRowItemFactory.Create(entity),
                 _statusBatchControlRowItemFactory.Create(entity));
    }

  }
}