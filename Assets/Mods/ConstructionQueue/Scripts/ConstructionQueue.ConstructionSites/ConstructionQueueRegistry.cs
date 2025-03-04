using System;
using System.Collections.Generic;
using System.Text;
using Timberborn.Common;
using Timberborn.ConstructionSites;
using Timberborn.EntitySystem;
using UnityEngine;

namespace ConstructionQueue.ConstructionSites {
  class ConstructionJobComparer : IComparer<ConstructionJob> {
    public int Compare(ConstructionJob a, ConstructionJob b) {
      return (a, b) switch {
          (null, null) => 0,
          (_, null) => 1,
          (null, _) => -1,
          _ => b._constructionSite._reservations._builders.Count
               - a._constructionSite._reservations._builders.Count
      };
    }

  }
  
  public class ConstructionQueueRegistry {
    static readonly ConstructionJobComparer Comparer = new();
    readonly List<ConstructionJob> _sortedJobs = new();
    
    public event EventHandler JobQueueChanged;

    public ReadOnlyList<ConstructionJob> JobQueue => _sortedJobs.AsReadOnlyList();
    
    public void JobAdded(ConstructionJob job) {
      // TODO possibly sorting is not needed here and can be done in the UI logic instead
      _sortedJobs.InsertSorted(job, Comparer, out _);
      // TODO this does not seem like a good indicator of what buildings are being constructed.
      // Maybe see if I can detect which construction sites are receiving goods (have reserved inventory)
      job._constructionSite.OnConstructionSiteReserved += OnConstructionSiteReservationChanged;
      job._constructionSite.OnConstructionSiteUnreserved += OnConstructionSiteReservationChanged;
      InvokeJobQueueChanged();
    }

    public void JobRemoved(ConstructionJob job) {
      _sortedJobs.Remove(job);
      job._constructionSite.OnConstructionSiteReserved -= OnConstructionSiteReservationChanged;
      job._constructionSite.OnConstructionSiteUnreserved -= OnConstructionSiteReservationChanged;
      InvokeJobQueueChanged();
    }

    void SortJobs() {
      _sortedJobs.Sort(Comparer);
      InvokeJobQueueChanged();
    }

    void InvokeJobQueueChanged() {
      PrintList();
      var jobQueueChanged = JobQueueChanged;
      if (jobQueueChanged != null) {
        jobQueueChanged.Invoke(this, EventArgs.Empty);
      }
    }

    void OnConstructionSiteReservationChanged(object sender, ConstructionSiteReservationEventArgs e) {
      SortJobs();
    }

    void PrintList() {
      var sb = new StringBuilder();
      sb.Append("Jobs Queue: ").AppendLine();
      foreach (var job in _sortedJobs) {
        sb.AppendFormat("- {0} - builders: {1}/{2}",
                        job.GetComponentFast<LabeledEntity>().DisplayName,
                        job._constructionSite._reservations._builders.Count,
                        job._constructionSite._reservations._capacity).AppendLine();
      }
      Debug.LogWarning(sb.ToString());
    }
  }
}