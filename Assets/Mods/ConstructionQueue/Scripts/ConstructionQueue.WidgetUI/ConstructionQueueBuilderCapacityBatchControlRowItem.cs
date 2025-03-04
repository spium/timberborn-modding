using Timberborn.BatchControl;
using Timberborn.ConstructionSites;
using UnityEngine.UIElements;

namespace ConstructionQueue.WidgetUI {
  public class ConstructionQueueBuilderCapacityBatchControlRowItem : IBatchControlRowItem, IUpdateableBatchControlRowItem {

    readonly Label _builderAmountLabel, _builderLimitLabel;
    readonly ConstructionSite _constructionSite;
    public VisualElement Root { get; }

    public ConstructionQueueBuilderCapacityBatchControlRowItem(VisualElement root, Label amount, Label limit, ConstructionSite constructionSite) {
      Root = root;
      _builderAmountLabel = amount;
      _builderLimitLabel = limit;
      _constructionSite = constructionSite;
    }

    public void UpdateRowItem() {
      _builderAmountLabel.text = _constructionSite._reservations._builders.Count.ToString();
      _builderLimitLabel.text = _constructionSite._reservations._capacity.ToString();
    }

  }
}