using ConstructionQueue.ConstructionSites;
using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.AssetSystem;
using Timberborn.BatchControl;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.SelectionSystem;
using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace ConstructionQueue.WidgetUI {
  public class ConstructionQueuePanel : ILoadableSingleton, ILateUpdatableSingleton {

    readonly UILayout _uiLayout;
    readonly EventBus _eventBus;
    readonly VisualElementLoader _visualElementLoader;
    readonly IAssetLoader _assetLoader;
    readonly ConstructionQueueRegistry _registry;
    readonly ConstructionQueueRowFactory _rowFactory;
    readonly List<BatchControlRow> _rows = new();

    VisualElement _root;
    ScrollView _scrollView;

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

    void OnJobQueueChanged(object sender, EventArgs e) {
      Debug.LogWarning("Job queue changed");
      _scrollView.Clear();
      foreach (var row in _rows) {
        row.ClearItems();
      }
      
      //TODO this obviously needs improving, update the event args to be able to tell if an entity was added or removed, or just the order changed
      
      _rows.Clear();
      _rows.AddRange(_registry.JobQueue.Select(_rowFactory.Create));
      foreach (var row in _rows) {
        _scrollView.Add(row.Root);
      }
      
      Debug.LogWarningFormat("Added {0} rows", _rows.Count);
      // TODO use VisualElement.Sort() with visual element user data to be able to sort the visual elements in place
      //_scrollView.Sort((a, b) => a.userData);
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
      foreach (var row in _rows) {
        row.Root.EnableInClassList(BatchControlRowHighlighter.HighlightedClass, row.Entity == entity);
      }
    }

    [OnEvent]
    public void OnSelectableObjectUnselected(
        SelectableObjectUnselectedEvent selectableObjectUnselectedEvent) {
      foreach (var row in _rows) {
        row.Root.EnableInClassList(BatchControlRowHighlighter.HighlightedClass, false);
      }
    }
    
    public void LateUpdateSingleton() {
      foreach (var row in _rows) {
        row.UpdateItems();
      }
    }

  }
}