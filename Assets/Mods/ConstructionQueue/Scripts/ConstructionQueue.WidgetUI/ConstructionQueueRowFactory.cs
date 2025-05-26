using Timberborn.BuildingsUI;
using Timberborn.ConstructionSites;
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

    public ConstructionQueueRowFactory(VisualElementLoader visualElementLoader,
                                       BuildingBatchControlRowItemFactory
                                           buildingBatchControlRowItemFactory,
                                       ConstructionSitePriorityBatchControlRowItemFactory
                                           constructionSitePriorityBatchControlRowItemFactory,
                                       StatusBatchControlRowItemFactory statusBatchControlRowItemFactory) {
      _visualElementLoader = visualElementLoader;
      _buildingBatchControlRowItemFactory = buildingBatchControlRowItemFactory;
      _constructionSitePriorityBatchControlRowItemFactory = constructionSitePriorityBatchControlRowItemFactory;
      _statusBatchControlRowItemFactory = statusBatchControlRowItemFactory;
    }


    public ConstructionQueueBatchControlRow Create(ConstructionJob job) {
      var root = _visualElementLoader.LoadVisualElement("Game/BatchControl/BatchControlRow");
      // TODO this line below is probably not needed. See if you can make the UI look perfect (there's to much space on the right side).
      // Remove distance from district center from rows.
      root.style.flexGrow = 1;
      var comparisonData = new ConstructionQueueRowComparisonData(job);
      root.userData = comparisonData;
      //TODO the status for "can't get all required materials" does not seem to update in real-time, figure out why
      return new(root, job.GetComponentFast<EntityComponent>(),
                 //TODO use a custom item instead of BuildingBatchControlRowItem to avoid showing the distance from district center
                 _buildingBatchControlRowItemFactory.Create(job),
                 _constructionSitePriorityBatchControlRowItemFactory.Create(job),
                 _statusBatchControlRowItemFactory.Create(job)) {
          ComparisonData = comparisonData
      };
    }
  }
}