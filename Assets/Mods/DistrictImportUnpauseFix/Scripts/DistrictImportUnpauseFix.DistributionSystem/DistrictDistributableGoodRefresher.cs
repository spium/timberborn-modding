using System;
using System.Linq;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.BuildingsBlocking;
using Timberborn.DistributionSystem;
using Timberborn.GameDistricts;
using Timberborn.InventorySystem;

namespace DistrictImportUnpauseFix.DistributionSystem {
  public class DistrictDistributableGoodRefresher : BaseComponent, IFinishedStateListener, IUnfinishedStateListener {
    DistrictBuilding _districtBuilding;
    PausableBuilding _pausableBuilding;
    Inventories _inventories;

    void Awake() {
      _districtBuilding = GetComponentFast<DistrictBuilding>();
      _pausableBuilding = GetComponentFast<PausableBuilding>();
      _inventories = GetComponentFast<Inventories>();
      if (!_pausableBuilding || !_inventories) {
        enabled = false;
      }
    }
    
    void OnPausedStateChanged(object sender, EventArgs args) {
      var district = _districtBuilding.GetDistrictOrConstructionDistrict();
      if (!district) {
        return;
      }
      var distributableGoodProvider =
          district.GetComponentFast<DistrictDistributableGoodProvider>();
      foreach (var good in _inventories.EnabledInventories.SelectMany(i => i.InputGoods._set).Distinct()) {
        distributableGoodProvider.ClearCache(good);
      }
    }

    void RegisterPausedStateChangedEventHandler() {
      _pausableBuilding.PausedChanged += OnPausedStateChanged;
    }

    void UnregisterPausedStateChangedEventHandler() {
      _pausableBuilding.PausedChanged -= OnPausedStateChanged;
    }

    public void OnEnterFinishedState() {
      if (!enabled) {
        return;
      }
      RegisterPausedStateChangedEventHandler();
    }

    public void OnExitFinishedState() {
      if (!enabled) {
        return;
      }
      UnregisterPausedStateChangedEventHandler();
    }

    public void OnEnterUnfinishedState() {
      if (!enabled) {
        return;
      }
      RegisterPausedStateChangedEventHandler();
    }

    public void OnExitUnfinishedState() {
      if (!enabled) {
        return;
      }
      UnregisterPausedStateChangedEventHandler();
    }

  }
}