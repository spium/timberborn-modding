using Timberborn.Attractions;
using Timberborn.AttractionsBatchControl;
using Timberborn.BatchControl;
using Timberborn.BuildingsUI;
using Timberborn.CoreUI;
using Timberborn.DwellingSystem;
using Timberborn.EntitySystem;
using Timberborn.HousingBatchControl;
using Timberborn.Reproduction;
using Timberborn.WorkplacesBatchControl;
using Timberborn.WorkSystem;

namespace GoodTracing.BatchControl {
  public class GoodTracingBatchControlRowFactory {

    readonly VisualElementLoader _visualElementLoader;
    readonly WorkplacesBatchControlRowFactory _workplacesBatchControlRowFactory;
    readonly AttractionsBatchControlRowFactory _attractionsBatchControlRowFactory;
    readonly HousingBatchControlRowFactory _housingBatchControlRowFactory;
    readonly BuildingBatchControlRowItemFactory _buildingBatchControlRowItemFactory;

    public GoodTracingBatchControlRowFactory(VisualElementLoader visualElementLoader,
                                             WorkplacesBatchControlRowFactory
                                                 workplacesBatchControlRowFactory,
                                             AttractionsBatchControlRowFactory
                                                 attractionsBatchControlRowFactory,
                                             HousingBatchControlRowFactory
                                                 housingBatchControlRowFactory,
                                             BuildingBatchControlRowItemFactory
                                                 buildingBatchControlRowItemFactory) {
      _visualElementLoader = visualElementLoader;
      _workplacesBatchControlRowFactory = workplacesBatchControlRowFactory;
      _attractionsBatchControlRowFactory = attractionsBatchControlRowFactory;
      _housingBatchControlRowFactory = housingBatchControlRowFactory;
      _buildingBatchControlRowItemFactory = buildingBatchControlRowItemFactory;
    }

    public BatchControlRow Create(EntityComponent entity) {
      if (entity.GetComponentFast<Workplace>()) {
        return _workplacesBatchControlRowFactory.Create(entity);
      }
      
      if (entity.GetComponentFast<Attraction>()) {
        return _attractionsBatchControlRowFactory.Create(entity);
      }

      if (entity.GetComponentFast<Dwelling>()
          || entity.GetComponentFast<BreedingPod>()) {
        return _housingBatchControlRowFactory.Create(entity);
      }

      // TODO maybe improve?
      return new(_visualElementLoader.LoadVisualElement("Game/BatchControl/BatchControlRow"),
                 entity, _buildingBatchControlRowItemFactory.Create(entity));
    }
  }
}