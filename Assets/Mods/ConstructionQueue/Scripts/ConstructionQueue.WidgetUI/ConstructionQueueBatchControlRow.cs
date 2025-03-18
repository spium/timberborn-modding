using System;
using Timberborn.BatchControl;
using Timberborn.EntitySystem;
using UnityEngine.UIElements;

namespace ConstructionQueue.WidgetUI {
  public class ConstructionQueueBatchControlRow : BatchControlRow {
    public ConstructionQueueBatchControlRow(VisualElement root, params IBatchControlRowItem[] batchControlRowItems) : base(root, batchControlRowItems) { }
    public ConstructionQueueBatchControlRow(VisualElement root, EntityComponent entity, params IBatchControlRowItem[] batchControlRowItems) : base(root, entity, batchControlRowItems) { }
    public ConstructionQueueBatchControlRow(VisualElement root, EntityComponent entity, Func<bool> visibilityGetter, params IBatchControlRowItem[] batchControlRowItems) : base(root, entity, visibilityGetter, batchControlRowItems) { }
    
    public ConstructionQueueRowComparisonData ComparisonData { get; init; }
  }
}