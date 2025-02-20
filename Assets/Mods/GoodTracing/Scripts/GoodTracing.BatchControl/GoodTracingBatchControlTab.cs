using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.BatchControl;
using Timberborn.Buildings;
using Timberborn.BuildingsBlocking;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.Goods;
using Timberborn.InputSystemUI;
using Timberborn.InventorySystem;
using Timberborn.SingletonSystem;
using Timberborn.Stockpiles;
using UnityEngine;
using UnityEngine.UIElements;

namespace GoodTracing.BatchControl {
  public abstract class GoodTracingBatchControlTab : BatchControlTab,
                                                     ILoadableSingleton {

    readonly IGoodService _goodService;
    readonly BatchControlRowGroupFactory _batchControlRowGroupFactory;
    readonly GoodTracingBatchControlRowFactory _goodTracingBatchControlRowFactory;
    readonly BindableToggleFactory _bindableToggleFactory;
    readonly EventBus _eventBus;

    VisualElement _headerVisualElement;
    BindableToggle _showPausedToggle;
    bool _showPaused;
    bool _isTabVisible;
    readonly List<EntityComponent> _trackedEntities = new();

    protected GoodTracingBatchControlTab(VisualElementLoader visualElementLoader,
                                         BatchControlDistrict batchControlDistrict,
                                         IGoodService goodService,
                                         BatchControlRowGroupFactory batchControlRowGroupFactory,
                                         GoodTracingBatchControlRowFactory
                                             goodTracingBatchControlRowFactory,
                                         BindableToggleFactory bindableToggleFactory,
                                         EventBus eventBus
    ) :
        base(visualElementLoader, batchControlDistrict) {
      _goodService = goodService;
      _batchControlRowGroupFactory = batchControlRowGroupFactory;
      _goodTracingBatchControlRowFactory = goodTracingBatchControlRowFactory;
      _bindableToggleFactory = bindableToggleFactory;
      _eventBus = eventBus;
    }

    public override bool RemoveEmptyRowGroups => true;

    public void Load() {
      _headerVisualElement =
          _visualElementLoader.LoadVisualElement("GoodTracing/GoodTracingBatchControlTabHeader");
      var toggle = _headerVisualElement.Q<Toggle>("ShowPaused");
      _showPausedToggle = _bindableToggleFactory.Create(toggle, "GoodTracingTabShowPaused",
                                                        value => {
                                                          _showPaused = value;
                                                          UpdateRowsVisibility();
                                                        },
                                                        () => _showPaused);
      _showPausedToggle.Disable();
      _eventBus.Register(this);
    }

    [OnEvent]
    public void
        OnBatchControlBoxHiddenEvent(BatchControlBoxHiddenEvent batchControlBoxHiddenEvent) {
      _trackedEntities.Clear();
    }

    public override VisualElement GetHeader() {
      return _headerVisualElement;
    }

    public override void Show() {
      _showPausedToggle.Bind();
      _showPausedToggle.Enable();
      RegisterAllListeners();
      _isTabVisible = true;
    }

    public override void Hide() {
      _showPausedToggle.Unbind();
      _showPausedToggle.Disable();
      UnregisterAllListeners();
      _isTabVisible = false;
    }

    void RegisterAllListeners() {
      foreach (var entity in _trackedEntities) {
        if (!entity) {
          continue;
        }
        var pausable = entity.GetComponentFast<PausableBuilding>();
        if (pausable) {
          pausable.PausedChanged += OnPausedStateChanged;
        }
        RegisterListenersToRefreshRowVisibility(entity);
      }
    }

    void UnregisterAllListeners() {
      foreach (var entity in _trackedEntities) {
        if (!entity) {
          continue;
        }
        var pausable = entity.GetComponentFast<PausableBuilding>();
        if (pausable) {
          pausable.PausedChanged -= OnPausedStateChanged;
        }
        UnregisterListenersToRefreshRowVisibility(entity);
      }
    }

    public override IEnumerable<BatchControlRowGroup> GetRowGroups(
        IEnumerable<EntityComponent> entities) {
      UnregisterAllListeners();
      _trackedEntities.Clear();
      var groups = _goodService.Goods.ToDictionary(goodId => goodId,
                                                   goodId => {
                                                     var good = _goodService.GetGood(goodId);
                                                     return _batchControlRowGroupFactory
                                                         .Create(good.PluralDisplayName,
                                                                 good.PluralDisplayName);
                                                   });

      foreach (var entity in entities
                   .Where(ShouldDisplayEntity)
                   .Where(e => e.GetComponentFast<Building>())
                   .Where(e => !e.GetComponentFast<Stockpile>())) {
        var inventories = entity.GetComponentFast<Inventories>();
        if (!inventories || !inventories.HasEnabledInventories) {
          continue;
        }
        _trackedEntities.Add(entity);
        foreach (var good in inventories.AllInventories.SelectMany(GetGoods).Distinct()) {
          if (groups.TryGetValue(good, out var group)) {
            group.AddRow(
                _goodTracingBatchControlRowFactory.Create(entity, good, IsRowVisibleInternal));
          } else {
            Debug.LogWarningFormat("[GoodTracing] Unknown good: {0}", good);
          }
        }
      }

      if (_isTabVisible) {
        RegisterAllListeners();
      }
      return groups.Values;
    }

    void OnPausedStateChanged(object sender, EventArgs args) {
      UpdateRowsVisibility();
    }

    bool IsRowVisibleInternal(EntityComponent entity, string goodId) {
      return IsPausedEntityRowVisible(entity) && IsRowVisible(entity, goodId);
    }

    bool IsPausedEntityRowVisible(EntityComponent entity) {
      if (_showPaused) {
        return true;
      }
      var pausable = entity.GetComponentFast<PausableBuilding>();
      if (!pausable) {
        return true;
      }
      return !pausable.Paused;
    }

    protected abstract bool IsRowVisible(EntityComponent entity, string goodId);

    protected virtual bool ShouldDisplayEntity(EntityComponent entity) {
      return true;
    }

    protected abstract IEnumerable<string> GetGoods(Inventory inventory);

    protected abstract void RegisterListenersToRefreshRowVisibility(EntityComponent entity);

    protected abstract void UnregisterListenersToRefreshRowVisibility(EntityComponent entity);

  }
}