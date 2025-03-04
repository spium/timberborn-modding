using Timberborn.BaseComponentSystem;
using Timberborn.BatchControl;
using Timberborn.ConstructionSites;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using UnityEngine.UIElements;

namespace ConstructionQueue.WidgetUI {
  public class ConstructionQueueBuilderCapacityBatchControlRowItemFactory {

    readonly VisualElementLoader _visualElementLoader;

    public ConstructionQueueBuilderCapacityBatchControlRowItemFactory(
        VisualElementLoader visualElementLoader) {
      _visualElementLoader = visualElementLoader;
    }

    public IBatchControlRowItem Create(BaseComponent entity) {
      var root = _visualElementLoader.LoadVisualElement("ConstructionQueue/BuilderCapacityRowItem");
      var amount = root.Q<Label>("BuilderAmount");
      var limit = root.Q<Label>("BuilderLimit");
      return new ConstructionQueueBuilderCapacityBatchControlRowItem(
          root, amount, limit, entity.GetComponentFast<ConstructionSite>());
    }

  }
}