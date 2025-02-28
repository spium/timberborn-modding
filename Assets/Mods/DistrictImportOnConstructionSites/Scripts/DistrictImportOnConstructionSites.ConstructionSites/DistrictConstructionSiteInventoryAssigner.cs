using System;
using System.Collections.Generic;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.BuildingsBlocking;
using Timberborn.ConstructionSites;
using Timberborn.DistributionSystem;
using Timberborn.InventorySystem;

namespace DistrictImportOnConstructionSites.ConstructionSites {
  public class DistrictConstructionSiteInventoryAssigner : BaseComponent, IUnfinishedStateListener {
    
    Inventory _constructionSiteInventory;
    PausableBuilding _pausableBuilding;
    
    readonly HashSet<DistrictInventoryRegistry> _districtInventoryRegistries = new();
    
    void Awake() {
      _constructionSiteInventory = GetComponentFast<ConstructionSite>().Inventory;
      _pausableBuilding = GetComponentFast<PausableBuilding>();
    }

    public void OnEnterUnfinishedState() {
      if (_pausableBuilding) {
        _pausableBuilding.PausedChanged += OnPausedChanged;
      }
    }
    
    public void OnExitUnfinishedState() {
      if (_pausableBuilding) {
        _pausableBuilding.PausedChanged -= OnPausedChanged;
      }  
    }

    void OnPausedChanged(object sender, EventArgs e) {
      foreach (var districtInventoryRegistry in _districtInventoryRegistries) {
        if (!districtInventoryRegistry) {
          continue;
        }
        var distributableGoodProvider =
            districtInventoryRegistry.GetComponentFast<DistrictDistributableGoodProvider>();
        
        foreach (var good in _constructionSiteInventory.InputGoods) {
          distributableGoodProvider.ClearCache(good);
        }
      }
    }
    
    public void AddConstructionSiteInventory(DistrictInventoryRegistry districtInventoryRegistry) {
      if (!districtInventoryRegistry) {
        return;
      }
      if (!_districtInventoryRegistries.Add(districtInventoryRegistry)) {
        return;
      }
      
      districtInventoryRegistry.Add(_constructionSiteInventory);
    }
    
    public void RemoveConstructionSiteInventoryFromAllRegistries()
    {
      foreach (var districtInventoryRegistry in _districtInventoryRegistries) {
        if (districtInventoryRegistry) {
          districtInventoryRegistry.Remove(_constructionSiteInventory);
        }
      }
      _districtInventoryRegistries.Clear();
    }

    public void RemoveConstructionSiteInventory(DistrictInventoryRegistry districtInventoryRegistry) {
      if (!districtInventoryRegistry) {
        return;
      }
      if (!_districtInventoryRegistries.Remove(districtInventoryRegistry)) {
        return;
      }
      
      districtInventoryRegistry.Remove(_constructionSiteInventory);
    }

  }
}