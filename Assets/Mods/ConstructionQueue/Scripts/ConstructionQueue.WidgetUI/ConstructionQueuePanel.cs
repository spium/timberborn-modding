using ConstructionQueue.ConstructionSites;
using System.Collections.Generic;
using Timberborn.AssetSystem;
using Timberborn.BatchControl;
using Timberborn.ConstructionSites;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.SelectionSystem;
using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;
using UnityEngine.UIElements;

namespace ConstructionQueue.WidgetUI {
  public class ConstructionQueuePanel : ILoadableSingleton, ILateUpdatableSingleton {

    readonly UILayout _uiLayout;
    readonly EventBus _eventBus;
    readonly VisualElementLoader _visualElementLoader;
    readonly IAssetLoader _assetLoader;
    readonly ConstructionQueueRegistry _registry;
    readonly ConstructionQueueRowFactory _rowFactory;
    readonly Dictionary<ConstructionJob, ConstructionQueueBatchControlRow> _rows = new();

    VisualElement _root;
    ScrollView _scrollView;
    bool _dirty;

    public ConstructionQueuePanel(UILayout uiLayout, EventBus eventBus,
                                  VisualElementLoader visualElementLoader, IAssetLoader assetLoader,
                                  ConstructionQueueRegistry registry,
                                  ConstructionQueueRowFactory rowFactory) {
      _uiLayout = uiLayout;
      _eventBus = eventBus;
      _visualElementLoader = visualElementLoader;
      _assetLoader = assetLoader;
      _registry = registry;
      _rowFactory = rowFactory;
    }

    public void Load() {
      _root = _visualElementLoader.LoadVisualElement("ConstructionQueue/WidgetContainer");
      var stylesheet =
          _assetLoader.Load<StyleSheet>("UI/Views/Game/BatchControl/BatchControlStyle");
      _root.styleSheets.Add(stylesheet);
      _scrollView = _root.Q<ScrollView>("ConstructionJobs");
      _registry.JobQueueChanged += OnJobQueueChanged;
      _eventBus.Register(this);
    }

    void OnJobQueueChanged(object sender, JobQueueChangedEventArgs e) {
      ConstructionQueueBatchControlRow row;
      switch (e.Type) {
        case JobQueueChangeType.JobAdded:
          row = _rowFactory.Create(e.Job);
          _rows.Add(e.Job, row);
          _scrollView.Add(row.Root);
          break;
        
        case JobQueueChangeType.JobRemoved:
          row = _rows[e.Job];
          _scrollView.Remove(row.Root);
          _rows.Remove(e.Job);
          row.ClearItems();
          break;
      }

      _dirty = true;
    }

    [OnEvent]
    public void OnShowPrimaryUI(ShowPrimaryUIEvent showPrimaryUIEvent)
    {
      _uiLayout.AddTopLeft(_root, 1000);
    }

    [OnEvent]
    public void OnSelectableObjectSelected(
        SelectableObjectSelectedEvent selectableObjectSelectedEvent) {
      var entity =
          selectableObjectSelectedEvent.SelectableObject.GetComponentFast<EntityComponent>();
      foreach (var row in _rows.Values) {
        row.Root.EnableInClassList(BatchControlRowHighlighter.HighlightedClass, row.Entity == entity);
      }
    }

    [OnEvent]
    public void OnSelectableObjectUnselected(
        SelectableObjectUnselectedEvent selectableObjectUnselectedEvent) {
      foreach (var row in _rows.Values) {
        row.Root.EnableInClassList(BatchControlRowHighlighter.HighlightedClass, false);
      }
    }
    
    public void LateUpdateSingleton() {
      foreach (var row in _rows.Values) {
        row.UpdateItems();
      }

      if (_dirty) {
        _dirty = false;
        foreach (var row in _rows.Values) {
          row.ComparisonData.RefreshComparisonScore();
        }
        _scrollView.Sort(RowComparer);
      }
    }

    static int RowComparer(VisualElement a, VisualElement b) {
      var cmpA = (ConstructionQueueRowComparisonData) a.userData;
      var cmpB = (ConstructionQueueRowComparisonData) b.userData;
      var cmp = cmpB.ComparisonScore - cmpA.ComparisonScore;
      return cmp != 0 ? cmp : cmpA.InstantiationOrder - cmpB.InstantiationOrder;
    }

  }
}