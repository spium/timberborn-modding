using System;
using Timberborn.BatchControl;
using Timberborn.BuildingsUI;
using Timberborn.ConstructionSitesUI;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.FieldsUI;
using Timberborn.ForestryUI;
using Timberborn.GatheringUI;
using Timberborn.HaulingUI;
using Timberborn.PlantingUI;
using Timberborn.StatusSystemUI;
using Timberborn.WorkshopsUI;
using Timberborn.WorkSystemUI;

namespace GoodTracing.BatchControl {
  /**
 * Just a copy of Timberborn.WorkplacesBatchControl.WorkplacesBatchControlRowFactory that also adds the visibility getter to the rows it creates
 */
  public class GoodTracingWorkplacesBatchControlRowFactory {

    readonly VisualElementLoader _visualElementLoader;
    readonly BuildingBatchControlRowItemFactory _buildingBatchControlRowItemFactory;
    readonly WorkplacePriorityBatchControlRowItemFactory
        _workplacePriorityBatchControlRowItemFactory;
    readonly WorkplaceBatchControlRowItemFactory _workplaceBatchControlRowItemFactory;
    readonly PlantablePrioritizerBatchControlRowItemFactory
        _plantablePrioritizerBatchControlRowItemFactory;
    readonly FarmHouseBatchControlRowItemFactory _farmHouseBatchControlRowItemFactory;
    readonly GatherablePrioritizerBatchControlRowItemFactory
        _gatherablePrioritizerBatchControlRowItemFactory;
    readonly ManufactoryBatchControlRowItemFactory _manufactoryBatchControlRowItemFactory;
    readonly HaulCandidateBatchControlRowItemFactory _haulCandidateBatchControlRowItemFactory;
    readonly ConstructionSitePriorityBatchControlRowItemFactory
        _constructionSitePriorityBatchControlRowItemFactory;
    readonly StatusBatchControlRowItemFactory _statusBatchControlRowItemFactory;
    readonly WorkplaceWorkerTypeBatchControlRowItemFactory
        _workplaceWorkerTypeBatchControlRowItemFactory;
    readonly ProductivityBatchControlRowItemFactory _productivityBatchControlRowItemFactory;
    readonly ManufactoryTogglableRecipesBatchControlRowItemFactory
        _manufactoryTogglableRecipesBatchControlRowItemFactory;
    readonly ForesterBatchControlRowItemFactory _foresterBatchControlRowItemFactory;

    public GoodTracingWorkplacesBatchControlRowFactory(
        VisualElementLoader visualElementLoader,
        BuildingBatchControlRowItemFactory buildingBatchControlRowItemFactory,
        WorkplacePriorityBatchControlRowItemFactory workplacePriorityBatchControlRowItemFactory,
        WorkplaceBatchControlRowItemFactory workplaceBatchControlRowItemFactory,
        PlantablePrioritizerBatchControlRowItemFactory
            plantablePrioritizerBatchControlRowItemFactory,
        FarmHouseBatchControlRowItemFactory farmHouseBatchControlRowItemFactory,
        GatherablePrioritizerBatchControlRowItemFactory
            gatherablePrioritizerBatchControlRowItemFactory,
        ManufactoryBatchControlRowItemFactory manufactoryBatchControlRowItemFactory,
        HaulCandidateBatchControlRowItemFactory haulCandidateBatchControlRowItemFactory,
        ConstructionSitePriorityBatchControlRowItemFactory
            constructionSitePriorityBatchControlRowItemFactory,
        StatusBatchControlRowItemFactory statusBatchControlRowItemFactory,
        WorkplaceWorkerTypeBatchControlRowItemFactory workplaceWorkerTypeBatchControlRowItemFactory,
        ProductivityBatchControlRowItemFactory productivityBatchControlRowItemFactory,
        ManufactoryTogglableRecipesBatchControlRowItemFactory
            manufactoryTogglableRecipesBatchControlRowItemFactory,
        ForesterBatchControlRowItemFactory foresterBatchControlRowItemFactory) {
      _visualElementLoader = visualElementLoader;
      _buildingBatchControlRowItemFactory = buildingBatchControlRowItemFactory;
      _workplacePriorityBatchControlRowItemFactory = workplacePriorityBatchControlRowItemFactory;
      _workplaceBatchControlRowItemFactory = workplaceBatchControlRowItemFactory;
      _plantablePrioritizerBatchControlRowItemFactory =
          plantablePrioritizerBatchControlRowItemFactory;
      _farmHouseBatchControlRowItemFactory = farmHouseBatchControlRowItemFactory;
      _gatherablePrioritizerBatchControlRowItemFactory =
          gatherablePrioritizerBatchControlRowItemFactory;
      _manufactoryBatchControlRowItemFactory = manufactoryBatchControlRowItemFactory;
      _haulCandidateBatchControlRowItemFactory = haulCandidateBatchControlRowItemFactory;
      _constructionSitePriorityBatchControlRowItemFactory =
          constructionSitePriorityBatchControlRowItemFactory;
      _statusBatchControlRowItemFactory = statusBatchControlRowItemFactory;
      _workplaceWorkerTypeBatchControlRowItemFactory =
          workplaceWorkerTypeBatchControlRowItemFactory;
      _productivityBatchControlRowItemFactory = productivityBatchControlRowItemFactory;
      _manufactoryTogglableRecipesBatchControlRowItemFactory =
          manufactoryTogglableRecipesBatchControlRowItemFactory;
      _foresterBatchControlRowItemFactory = foresterBatchControlRowItemFactory;
    }

    public BatchControlRow Create(EntityComponent entity,
                                  Func<bool> visibilityGetter) {
      return new(_visualElementLoader.LoadVisualElement("Game/BatchControl/BatchControlRow"),
                 entity, visibilityGetter, _buildingBatchControlRowItemFactory.Create(entity),
                 _workplacePriorityBatchControlRowItemFactory.Create(entity),
                 _workplaceWorkerTypeBatchControlRowItemFactory.Create(entity),
                 _workplaceBatchControlRowItemFactory.Create(entity),
                 _haulCandidateBatchControlRowItemFactory.Create(entity),
                 _productivityBatchControlRowItemFactory.Create(entity),
                 _farmHouseBatchControlRowItemFactory.Create(entity),
                 _plantablePrioritizerBatchControlRowItemFactory.Create(entity),
                 _foresterBatchControlRowItemFactory.Create(entity),
                 _gatherablePrioritizerBatchControlRowItemFactory.Create(entity),
                 _manufactoryBatchControlRowItemFactory.Create(entity),
                 _manufactoryTogglableRecipesBatchControlRowItemFactory.Create(entity),
                 _constructionSitePriorityBatchControlRowItemFactory.Create(entity),
                 _statusBatchControlRowItemFactory.Create(entity));
    }

  }
}