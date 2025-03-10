using System;
using System.Linq;
using Timberborn.BuilderPrioritySystem;
using Timberborn.BuildingsBlocking;
using Timberborn.ConstructionSites;
using Timberborn.PrefabSystem;

namespace ConstructionQueue.WidgetUI {
  public class ConstructionQueueRowComparisonData {

    readonly ConstructionJob _job;
    readonly BuilderPrioritizable _prioritizable;
    readonly PausableBuilding _pausable;

    public ConstructionQueueRowComparisonData(ConstructionJob job) {
      _job = job;
      _prioritizable = job.GetComponentFast<BuilderPrioritizable>();
      _pausable = job.GetComponentFast<PausableBuilding>();
      InstantiationOrder = job.GetComponentFast<InstantiatedPrefab>().InstantiationOrder;
    }
    
    public int InstantiationOrder { get; }
    
    public int ComparisonScore { get; private set; }

    /**
     * Sort by
     * - unpaused
     * - is on (grounded and unblocked)
     * - has reserved builder
     * - has reserved capacity
     * - material progress
     * - priority
     * - instantiation order
     */
    public void RefreshComparisonScore() {
      var isUnpaused = _pausable.Paused ? 0 : 1;
      var isOn = _job._constructionSite.IsOn ? 1 : 0;
      var reservedBuilder = _job._constructionSite._reservations._builders.Count > 0 ? 1 : 0;
      var reservedCapacity = _job._constructionSite.Inventory.ReservedCapacity().Any() ? 1 : 0;
      var materialProgress = Convert.ToInt32(_job._constructionSite.MaterialProgress * 10);
      
      var priority = (int)_prioritizable.Priority;
      ComparisonScore = 100000 * isUnpaused
                        + 10000 * isOn
                        + reservedBuilder * 1000
                        + reservedCapacity * 100
                        + materialProgress * 10
                        + priority;
    }

  }
}