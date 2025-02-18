using System;
using Timberborn.Attractions;
using Timberborn.BatchControl;
using Timberborn.BuildingsUI;
using Timberborn.CoreUI;
using Timberborn.DwellingSystem;
using Timberborn.EntitySystem;
using Timberborn.Reproduction;
using Timberborn.WorkSystem;

namespace GoodTracing.BatchControl {
  public class GoodTracingBatchControlRowFactory {

    readonly VisualElementLoader _visualElementLoader;
    readonly GoodTracingWorkplacesBatchControlRowFactory _workplacesBatchControlRowFactory;
    readonly GoodTracingAttractionsBatchControlRowFactory _attractionsBatchControlRowFactory;
    readonly GoodTracingHousingBatchControlRowFactory _housingBatchControlRowFactory;
    readonly BuildingBatchControlRowItemFactory _buildingBatchControlRowItemFactory;

    public GoodTracingBatchControlRowFactory(VisualElementLoader visualElementLoader,
                                             GoodTracingWorkplacesBatchControlRowFactory
                                                 workplacesBatchControlRowFactory,
                                             GoodTracingAttractionsBatchControlRowFactory
                                                 attractionsBatchControlRowFactory,
                                             GoodTracingHousingBatchControlRowFactory
                                                 housingBatchControlRowFactory,
                                             BuildingBatchControlRowItemFactory
                                                 buildingBatchControlRowItemFactory) {
      _visualElementLoader = visualElementLoader;
      _workplacesBatchControlRowFactory = workplacesBatchControlRowFactory;
      _attractionsBatchControlRowFactory = attractionsBatchControlRowFactory;
      _housingBatchControlRowFactory = housingBatchControlRowFactory;
      _buildingBatchControlRowItemFactory = buildingBatchControlRowItemFactory;
    }

    public BatchControlRow Create(EntityComponent entity, string goodId, Func<EntityComponent, string, bool> visibilityGetter) {
      if (entity.GetComponentFast<Workplace>()) {
        return _workplacesBatchControlRowFactory.Create(entity, () => visibilityGetter(entity, goodId));
      }
      
      if (entity.GetComponentFast<Attraction>()) {
        return _attractionsBatchControlRowFactory.Create(entity, () => visibilityGetter(entity, goodId));
      }

      if (entity.GetComponentFast<Dwelling>()
          || entity.GetComponentFast<BreedingPod>()) {
        return _housingBatchControlRowFactory.Create(entity, () => visibilityGetter(entity, goodId));
      }

      // TODO maybe improve?
      return new(_visualElementLoader.LoadVisualElement("Game/BatchControl/BatchControlRow"),
                 entity, () => visibilityGetter(entity, goodId),
                 _buildingBatchControlRowItemFactory.Create(entity));
    }
  }
}