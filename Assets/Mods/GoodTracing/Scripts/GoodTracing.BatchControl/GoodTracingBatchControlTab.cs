using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.BatchControl;
using Timberborn.BlockSystem;
using Timberborn.Buildings;
using Timberborn.BuildingsBlocking;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.SingletonSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace GoodTracing.BatchControl {
  public abstract class GoodTracingBatchControlTab : BatchControlTab{

    readonly IGoodService _goodService;
    readonly BatchControlRowGroupFactory _batchControlRowGroupFactory;
    readonly GoodTracingBatchControlRowFactory _goodTracingBatchControlRowFactory;

    bool _showPaused;
    bool _isTabVisible;
    readonly HashSet<EntityComponent> _trackedEntities = new();

    protected GoodTracingBatchControlTab(VisualElementLoader visualElementLoader,
                                         BatchControlDistrict batchControlDistrict,
                                         IGoodService goodService,
                                         BatchControlRowGroupFactory batchControlRowGroupFactory,
                                         GoodTracingBatchControlRowFactory
                                             goodTracingBatchControlRowFactory,
                                         EventBus eventBus
    ) :
        base(visualElementLoader, batchControlDistrict, eventBus) {
      _goodService = goodService;
      _batchControlRowGroupFactory = batchControlRowGroupFactory;
      _goodTracingBatchControlRowFactory = goodTracingBatchControlRowFactory;
    }

    public override bool RemoveEmptyRowGroups => true;
    
    [OnEvent]
    public void
        OnBatchControlBoxHiddenEvent(BatchControlBoxHiddenEvent batchControlBoxHiddenEvent) {
      _trackedEntities.Clear();
    }
    
    /** Note: this should be [OnEvent] but because the base class already has this event registered and I cannot override it,
     * if I did put [OnEvent] here it would throw. So I'm using a workaround: I have the GoodTracingBatchControlTabEnteredFinishedStateWorkaround
     * class registered to the event instead, and it will invoke this function when the event is raised.
     */
    public void OnEnteredFinishedStateEvent(EnteredFinishedStateEvent evt) {
      if (!_isTabVisible) {
        return;
      }
      Debug.LogWarning("My finished state listener");
      if (_trackedEntities.Contains(evt.BlockObject.GetComponentFast<EntityComponent>())) {
        UpdateRowsVisibility();
      }
    }

    public override VisualElement GetHeader() {
      var header = _visualElementLoader.LoadVisualElement("GoodTracing/GoodTracingBatchControlTabHeader");
      var showPausedToggle = header.Q<Toggle>("ShowPaused");
      showPausedToggle.value = _showPaused;
      showPausedToggle.RegisterValueChangedCallback(ShowPausedBuildingsToggled);
      return header;
    }

    public override void Show() {
      RegisterAllListeners();
      _isTabVisible = true;
    }

    public override void Hide() {
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
                                                         .Create(good.PluralDisplayName.Value,
                                                                 good.PluralDisplayName.Value);
                                                   });

      foreach (var entity in entities
                   .Where(e => e.GetComponentFast<BuildingSpec>())
                   .Where(ShouldAddToRowGroups)) {
        var inventories = entity.GetComponentFast<Inventories>();
        if (!inventories || !inventories.HasEnabledInventories) {
          continue;
        }
        _trackedEntities.Add(entity);
        
        // If the building is finished, I only need to add rows for enabled inventories.
        // Otherwise I need to add all inventories: I will only display the enabled ones, but if the
        // building becomes finished while the tab is open, the enabled inventories will change (from the construction
        // ones to the manufactory or other ones).
        var blockState = entity.GetComponentFast<BlockObjectState>();
        var isFinished = blockState == null || blockState.IsFinished;
        var inventoriesToAdd = isFinished
            ? inventories.EnabledInventories
            : inventories.AllInventories;
        
        foreach (var inventory in inventoriesToAdd) {
          foreach (var good in GetGoods(inventory)) {
            if (groups.TryGetValue(good, out var group)) {
              group.AddRow(
                  _goodTracingBatchControlRowFactory.Create(entity, inventory, good, IsRowVisibleInternal));
            } else {
              Debug.LogWarningFormat("[GoodTracing] Unknown good: {0}", good);
            }
          }
        }
      }

      if (_isTabVisible) {
        RegisterAllListeners();
      }
      return groups.Values;
    }

    void ShowPausedBuildingsToggled(ChangeEvent<bool> evt) {
      _showPaused = evt.newValue;
      UpdateRowsVisibility();
    }
    
    void OnPausedStateChanged(object sender, EventArgs args) {
      UpdateRowsVisibility();
    }

    bool IsRowVisibleInternal(EntityComponent entity, string goodId) {
      return ShouldAddToRowGroups(entity)
             && IsPausedEntityRowVisible(entity)
             && IsRowVisible(entity, goodId);
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

    protected virtual bool ShouldAddToRowGroups(EntityComponent entity) {
      return true;
    }

    protected abstract IEnumerable<string> GetGoods(Inventory inventory);

    protected abstract void RegisterListenersToRefreshRowVisibility(EntityComponent entity);

    protected abstract void UnregisterListenersToRefreshRowVisibility(EntityComponent entity);

  }
}