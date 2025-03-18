using ConstructionQueue.ConstructionSites;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    readonly HashSet<ConstructionJob> _jobsToAdd = new();

    VisualElement _root;
    ScrollView _scrollView;
    Label _constructionCountLabel;
    bool _orderDirty, _panelVisible, _selectedDirty;
    int _refreshFrames;
    EntityComponent _selected;

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
      _constructionCountLabel = _root.Q<Label>("ConstructionCount");
      ConfigureVisibilityToggling(_root, _root.Q<VisualElement>("ContentWrapper"));
      _registry.JobQueueChanged += OnJobQueueChanged;
      _eventBus.Register(this);
    }

    void OnJobQueueChanged(object sender, JobQueueChangedEventArgs e) {
      switch (e.Type) {
        case JobQueueChangeType.JobAdded:
          if (_jobsToAdd.Add(e.Job)) {
            _refreshFrames = 0;
          }
          break;
        
        case JobQueueChangeType.JobRemoved:
          if (_rows.TryGetValue(e.Job, out var row)) {
            _scrollView.Remove(row.Root);
            _rows.Remove(e.Job);
            row.ClearItems();
            _refreshFrames = 0;
          } else {
            _jobsToAdd.Remove(e.Job);
          }
          break;
      }

      _orderDirty = true;
    }

    [OnEvent]
    public void OnShowPrimaryUI(ShowPrimaryUIEvent showPrimaryUIEvent)
    {
      _uiLayout.AddTopLeft(_root, 1000);
    }

    [OnEvent]
    public void OnSelectableObjectSelected(
        SelectableObjectSelectedEvent selectableObjectSelectedEvent) {
      if (!selectableObjectSelectedEvent.SelectableObject.TryGetComponentFast(
              out ConstructionSite site)
          || !site.enabled) {
        return;
      }
      
      var entity =
          selectableObjectSelectedEvent.SelectableObject.GetComponentFast<EntityComponent>();
      _selected = entity;
      _selectedDirty = true;
    }

    [OnEvent]
    public void OnSelectableObjectUnselected(
        SelectableObjectUnselectedEvent selectableObjectUnselectedEvent) {
      var wasSelected = _selected != null;
      _selected = null;
      _selectedDirty = wasSelected;
    }
    
    public void LateUpdateSingleton() {
      // TODO localize
      _constructionCountLabel.text =
          string.Format("Construction sites: {0}", _rows.Count + _jobsToAdd.Count);
      
      if (!_panelVisible) {
        return;
      }

      if (_jobsToAdd.Count > 0) {
        foreach (var job in _jobsToAdd) {
          var row = _rowFactory.Create(job);
          _rows.Add(job, row);
          _scrollView.Add(row.Root);
        }
        _jobsToAdd.Clear();
      }

      foreach (var row in _rows.Values) {
        row.UpdateItems();
      }
      
      RefreshSelected();

      if (_orderDirty) {
        if (_refreshFrames > 0) {
          _refreshFrames--;
          return;
        }
        
        _orderDirty = false;
        _refreshFrames = 120;
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
    
    void ConfigureVisibilityToggling(VisualElement root, VisualElement content)
    {
      var toggler = root.Q<Button>("ExtensionToggler");
      var background = root.Q<VisualElement>("Background");
      _panelVisible = content.IsDisplayed();
      toggler.RegisterCallback<ClickEvent>(_ => ToggleVisibility(toggler, content, background));
    }
    
    void ToggleVisibility(
        Button toggler,
        VisualElement items,
        VisualElement background)
    {
      var shouldHide = items.IsDisplayed();
      _panelVisible = !shouldHide;
      toggler.EnableInClassList("extension-clamp--hidden", shouldHide);
      background.ToggleDisplayStyle(!shouldHide);
      items.ToggleDisplayStyle(!shouldHide);
      if (_panelVisible) {
        _refreshFrames = 0;
      }
    }

    void RefreshSelected() {
      if (!_selectedDirty) {
        return;
      }
      _selectedDirty = false;
      foreach (var row in _rows.Values) {
        row.Root.EnableInClassList(BatchControlRowHighlighter.HighlightedClass, ReferenceEquals(row.Entity, _selected));
      }
    }
  }
}