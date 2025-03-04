using Timberborn.BaseComponentSystem;
using Timberborn.BatchControl;
using Timberborn.BuildingsUI;
using Timberborn.ConstructionSitesUI;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.StatusSystemUI;

namespace ConstructionQueue.WidgetUI {
  public class ConstructionQueueRowFactory {

    readonly VisualElementLoader _visualElementLoader;
    readonly BuildingBatchControlRowItemFactory _buildingBatchControlRowItemFactory;
    readonly ConstructionSitePriorityBatchControlRowItemFactory _constructionSitePriorityBatchControlRowItemFactory;
    readonly StatusBatchControlRowItemFactory _statusBatchControlRowItemFactory;
    readonly ConstructionQueueBuilderCapacityBatchControlRowItemFactory
        _builderCapacityBatchControlRowItemFactory;

    public ConstructionQueueRowFactory(VisualElementLoader visualElementLoader,
                                       BuildingBatchControlRowItemFactory
                                           buildingBatchControlRowItemFactory,
                                       ConstructionSitePriorityBatchControlRowItemFactory
                                           constructionSitePriorityBatchControlRowItemFactory,
                                       StatusBatchControlRowItemFactory statusBatchControlRowItemFactory,
                                       ConstructionQueueBuilderCapacityBatchControlRowItemFactory constructionQueueBuilderCapacityBatchControlRowItemFactory) {
      _visualElementLoader = visualElementLoader;
      _buildingBatchControlRowItemFactory = buildingBatchControlRowItemFactory;
      _constructionSitePriorityBatchControlRowItemFactory = constructionSitePriorityBatchControlRowItemFactory;
      _statusBatchControlRowItemFactory = statusBatchControlRowItemFactory;
      _builderCapacityBatchControlRowItemFactory = constructionQueueBuilderCapacityBatchControlRowItemFactory;
    }


    public BatchControlRow Create(BaseComponent job) {
      var root = _visualElementLoader.LoadVisualElement("Game/BatchControl/BatchControlRow");
      //TODO the status for "can't get all required materials" does not seem to update in real-time, figure out why
      return new(root, job.GetComponentFast<EntityComponent>(),
                 _buildingBatchControlRowItemFactory.Create(job),
                 _builderCapacityBatchControlRowItemFactory.Create(job),
                 _constructionSitePriorityBatchControlRowItemFactory.Create(job),
                 _statusBatchControlRowItemFactory.Create(job));
    }
  }
}