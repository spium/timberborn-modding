using System;
using System.Linq;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.BuildingsBlocking;
using Timberborn.DistributionSystem;
using Timberborn.GameDistricts;
using Timberborn.InventorySystem;

namespace DistrictImportUnpauseFix.DistributionSystem {
  public class DistrictDistributableGoodRefresher : BaseComponent, IFinishedStateListener {
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
      var district = _districtBuilding.District;
      if (!district) {
        return;
      }
      var distributableGoodProvider =
          district.GetComponentFast<DistrictDistributableGoodProvider>();
      foreach (var good in _inventories.EnabledInventories.SelectMany(i => i.InputGoods._set).Distinct()) {
        distributableGoodProvider.ClearCache(good);
      }
    }

    public void OnEnterFinishedState() {
      if (!enabled) {
        return;
      }
      _pausableBuilding.PausedChanged += OnPausedStateChanged;
    }

    public void OnExitFinishedState() {
      if (!enabled) {
        return;
      }
      _pausableBuilding.PausedChanged -= OnPausedStateChanged;
    }
  }
}