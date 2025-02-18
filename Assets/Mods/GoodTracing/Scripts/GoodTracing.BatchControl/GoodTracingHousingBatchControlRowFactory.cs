using System;
using Timberborn.BatchControl;
using Timberborn.BuildingsUI;
using Timberborn.ConstructionSitesUI;
using Timberborn.CoreUI;
using Timberborn.DwellingSystemUI;
using Timberborn.EntitySystem;
using Timberborn.HaulingUI;
using Timberborn.InventorySystemBatchControl;
using Timberborn.ReproductionUI;
using Timberborn.StatusSystemUI;

namespace GoodTracing.BatchControl {
  /**
   * Just a copy of Timberborn.HousingBatchControl.HousingBatchControlRowFactory that also adds the visibility getter to the rows it creates
   */
  public class GoodTracingHousingBatchControlRowFactory {

    readonly VisualElementLoader _visualElementLoader;
    readonly BuildingBatchControlRowItemFactory _buildingBatchControlRowItemFactory;
    readonly DwellingBatchControlRowItemFactory _dwellingBatchControlRowItemFactory;
    readonly HaulCandidateBatchControlRowItemFactory _haulCandidateBatchControlRowItemFactory;
    readonly BreedingPodBatchControlRowItemFactory _breedingPodBatchControlRowItemFactory;
    readonly StatusBatchControlRowItemFactory _statusBatchControlRowItemFactory;
    readonly ConstructionSitePriorityBatchControlRowItemFactory
        _constructionSitePriorityBatchControlRowItemFactory;
    readonly InventoryCapacityBatchControlRowItemFactory
        _inventoryCapacityBatchControlRowItemFactory;

    public GoodTracingHousingBatchControlRowFactory(
        VisualElementLoader visualElementLoader,
        BuildingBatchControlRowItemFactory buildingBatchControlRowItemFactory,
        DwellingBatchControlRowItemFactory dwellingBatchControlRowItemFactory,
        HaulCandidateBatchControlRowItemFactory haulCandidateBatchControlRowItemFactory,
        BreedingPodBatchControlRowItemFactory breedingPodBatchControlRowItemFactory,
        StatusBatchControlRowItemFactory statusBatchControlRowItemFactory,
        ConstructionSitePriorityBatchControlRowItemFactory
            constructionSitePriorityBatchControlRowItemFactory,
        InventoryCapacityBatchControlRowItemFactory inventoryCapacityBatchControlRowItemFactory) {
      _visualElementLoader = visualElementLoader;
      _buildingBatchControlRowItemFactory = buildingBatchControlRowItemFactory;
      _dwellingBatchControlRowItemFactory = dwellingBatchControlRowItemFactory;
      _haulCandidateBatchControlRowItemFactory = haulCandidateBatchControlRowItemFactory;
      _breedingPodBatchControlRowItemFactory = breedingPodBatchControlRowItemFactory;
      _statusBatchControlRowItemFactory = statusBatchControlRowItemFactory;
      _constructionSitePriorityBatchControlRowItemFactory =
          constructionSitePriorityBatchControlRowItemFactory;
      _inventoryCapacityBatchControlRowItemFactory = inventoryCapacityBatchControlRowItemFactory;
    }

    public BatchControlRow Create(EntityComponent entity, Func<bool> visibilityGetter) {
      return new(_visualElementLoader.LoadVisualElement("Game/BatchControl/BatchControlRow"),
                 entity, visibilityGetter, _buildingBatchControlRowItemFactory.Create(entity),
                 _dwellingBatchControlRowItemFactory.Create(entity),
                 _haulCandidateBatchControlRowItemFactory.Create(entity),
                 _breedingPodBatchControlRowItemFactory.Create(entity),
                 _inventoryCapacityBatchControlRowItemFactory.Create(entity),
                 _constructionSitePriorityBatchControlRowItemFactory.Create(entity),
                 _statusBatchControlRowItemFactory.Create(entity));
    }

  }
}