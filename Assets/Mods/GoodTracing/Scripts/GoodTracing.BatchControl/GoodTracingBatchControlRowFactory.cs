using System;
using Timberborn.Attractions;
using Timberborn.BatchControl;
using Timberborn.BuildingsUI;
using Timberborn.CoreUI;
using Timberborn.DwellingSystem;
using Timberborn.EntitySystem;
using Timberborn.InventorySystem;
using Timberborn.Reproduction;
using Timberborn.WorkSystem;

namespace GoodTracing.BatchControl {
  public class GoodTracingBatchControlRowFactory {

    readonly VisualElementLoader _visualElementLoader;
    readonly GoodTracingWorkplacesBatchControlRowFactory _workplacesBatchControlRowFactory;
    readonly GoodTracingAttractionsBatchControlRowFactory _attractionsBatchControlRowFactory;
    readonly GoodTracingHousingBatchControlRowFactory _housingBatchControlRowFactory;
    readonly BuildingBatchControlRowItemFactory _buildingBatchControlRowItemFactory;
    readonly GoodTracingInventoryCapacityBatchControlRowItemFactory
        _inventoryCapacityBatchControlRowItemFactory;

    public GoodTracingBatchControlRowFactory(VisualElementLoader visualElementLoader,
                                             GoodTracingWorkplacesBatchControlRowFactory
                                                 workplacesBatchControlRowFactory,
                                             GoodTracingAttractionsBatchControlRowFactory
                                                 attractionsBatchControlRowFactory,
                                             GoodTracingHousingBatchControlRowFactory
                                                 housingBatchControlRowFactory,
                                             BuildingBatchControlRowItemFactory
                                                 buildingBatchControlRowItemFactory,
                                             GoodTracingInventoryCapacityBatchControlRowItemFactory
                                                 inventoryCapacityBatchControlRowItemFactory) {
      _visualElementLoader = visualElementLoader;
      _workplacesBatchControlRowFactory = workplacesBatchControlRowFactory;
      _attractionsBatchControlRowFactory = attractionsBatchControlRowFactory;
      _housingBatchControlRowFactory = housingBatchControlRowFactory;
      _buildingBatchControlRowItemFactory = buildingBatchControlRowItemFactory;
      _inventoryCapacityBatchControlRowItemFactory = inventoryCapacityBatchControlRowItemFactory;
    }

    public BatchControlRow Create(EntityComponent entity, Inventory inventory, string goodId,
                                  Func<EntityComponent, string, bool> visibilityGetter) {
      Func<bool> isVisible = () => inventory.enabled && visibilityGetter(entity, goodId);

      if (entity.GetComponentFast<Workplace>()) {
        return _workplacesBatchControlRowFactory.Create(entity, inventory, goodId, isVisible);
      }

      if (entity.GetComponentFast<Attraction>()) {
        return _attractionsBatchControlRowFactory.Create(entity, inventory, goodId, isVisible);
      }

      if (entity.GetComponentFast<Dwelling>()
          || entity.GetComponentFast<BreedingPod>()) {
        return _housingBatchControlRowFactory.Create(entity, inventory, goodId, isVisible);
      }

      // TODO maybe improve?
      return new(_visualElementLoader.LoadVisualElement("Game/BatchControl/BatchControlRow"),
                 entity, isVisible,
                 _buildingBatchControlRowItemFactory.Create(entity),
                 _inventoryCapacityBatchControlRowItemFactory.Create(entity, inventory, goodId));
    }

  }
}