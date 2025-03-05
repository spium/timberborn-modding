using System;
using Timberborn.BuilderPrioritySystem;
using Timberborn.ConstructionSites;
using Timberborn.InventorySystem;
using Timberborn.PrioritySystem;

namespace ConstructionQueue.ConstructionSites {
  public enum JobQueueChangeType {
    JobAdded, JobRemoved, JobsReordered
  }
  
  public class JobQueueChangedEventArgs : EventArgs {
    public JobQueueChangeType Type { get; }
    public ConstructionJob Job { get; }

    public JobQueueChangedEventArgs(JobQueueChangeType type, ConstructionJob job) {
      Type = type;
      Job = job;
    }
  }
  
  public class ConstructionQueueRegistry {
    public event EventHandler<JobQueueChangedEventArgs> JobQueueChanged;
    
    internal void JobAdded(ConstructionJob job) {
      // subscribe to events that may cause the construction queue to be re-ordered
      job._constructionSite.OnConstructionSiteReserved += OnConstructionSiteReservationChanged;
      job._constructionSite.OnConstructionSiteUnreserved += OnConstructionSiteReservationChanged;
      job._constructionSite.Inventory.InventoryChanged += OnInventoryChanged;
      job._constructionSite._blockableBuilding.BuildingBlocked += OnBuildingBlockedChanged;
      job._constructionSite._blockableBuilding.BuildingUnblocked += OnBuildingBlockedChanged;
      job.GetComponentFast<BuilderPrioritizable>().PriorityChanged += OnPriorityChanged;
      InvokeJobQueueChanged(JobQueueChangeType.JobAdded, job);
    }

    internal void JobRemoved(ConstructionJob job) {
      job._constructionSite.OnConstructionSiteReserved -= OnConstructionSiteReservationChanged;
      job._constructionSite.OnConstructionSiteUnreserved -= OnConstructionSiteReservationChanged;
      job._constructionSite.Inventory.InventoryChanged -= OnInventoryChanged;
      job._constructionSite._blockableBuilding.BuildingBlocked -= OnBuildingBlockedChanged;
      job._constructionSite._blockableBuilding.BuildingUnblocked -= OnBuildingBlockedChanged;
      job.GetComponentFast<BuilderPrioritizable>().PriorityChanged -= OnPriorityChanged;
      InvokeJobQueueChanged(JobQueueChangeType.JobRemoved, job);
    }
    
    void InvokeJobQueueChanged(JobQueueChangeType type, ConstructionJob job = null) {
      var jobQueueChanged = JobQueueChanged;
      if (jobQueueChanged != null) {
        jobQueueChanged.Invoke(this, new(type, job));
      }
    }

    void OnConstructionSiteReservationChanged(object sender, ConstructionSiteReservationEventArgs e) {
      InvokeJobQueueChanged(JobQueueChangeType.JobsReordered);
    }
    
    void OnInventoryChanged(object sender, InventoryChangedEventArgs e) {
      InvokeJobQueueChanged(JobQueueChangeType.JobsReordered);
    }
    
    void OnBuildingBlockedChanged(object sender, EventArgs e) {
      InvokeJobQueueChanged(JobQueueChangeType.JobsReordered);
    }

    void OnPriorityChanged(object sender, PriorityChangedEventArgs e) {
      InvokeJobQueueChanged(JobQueueChangeType.JobsReordered);
    }
  }
}