using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Timberborn.BlockSystem;
using Timberborn.BuildingsNavigation;
using Timberborn.GameDistricts;
using Timberborn.InventorySystem;
using Timberborn.Navigation;
using Timberborn.SingletonSystem;

namespace DistrictImportOnConstructionSites.ConstructionSites {
  public class DistrictConstructionSitesRegistry : ILoadableSingleton, ISingletonNavMeshListener, IPostLoadableSingleton {

    readonly EventBus _eventBus;
    readonly DistrictCenterRegistry _districtCenterRegistry;
    readonly DistrictMap _districtMap;
    readonly NodeIdService _nodeIdService;
    readonly HashSet<DistrictConstructionSiteInventoryAssigner> _constructionSiteInventoryAssigners = new();
    
    bool _isLoaded;
    
    public DistrictConstructionSitesRegistry(EventBus eventBus, DistrictCenterRegistry districtCenterRegistry, DistrictMap districtMap, NodeIdService nodeIdService) {
      _eventBus = eventBus;
      _districtCenterRegistry = districtCenterRegistry;
      _districtMap = districtMap;
      _nodeIdService = nodeIdService;
    }

    public void Load() {
      _eventBus.Register(this);
    }
    
    [OnEvent]
    public void OnEnterUnfinishedState(EnteredUnfinishedStateEvent evt) {
      if (!evt.BlockObject.TryGetComponentFast(
              out DistrictConstructionSiteInventoryAssigner constructionSiteInventoryAssigner)) {
        return;
      }
      
      // track it, if it's already being tracked do nothing
      if (!_constructionSiteInventoryAssigners.Add(constructionSiteInventoryAssigner)) {
        return;
      }

      if (!_isLoaded) {
        return;
      }

      // no reason to do anything if there's only 1 district
      if (_districtCenterRegistry.FinishedDistrictCenters.Count <= 1) {
        return;
      }
      
      UpdateDistrictInventoryRegistries(_districtCenterRegistry.FinishedDistrictCenters, ImmutableArray.Create(constructionSiteInventoryAssigner));
    }
    
    [OnEvent]
    public void OnExitUnfinishedState(ExitedUnfinishedStateEvent evt) {
      if (!evt.BlockObject.TryGetComponentFast(
              out DistrictConstructionSiteInventoryAssigner constructionSiteInventoryAssigner)) {
        return;
      }
      
      // stop tracking it, if it wasn't being tracked do nothing
      if (!_constructionSiteInventoryAssigners.Remove(constructionSiteInventoryAssigner)) {
        return;
      }
      
      if (!_isLoaded) {
        return;
      }
      
      // no reason to do anything if there's only 1 district
      if (_districtCenterRegistry.FinishedDistrictCenters.Count <= 1) {
        return;
      }
      
      constructionSiteInventoryAssigner.RemoveConstructionSiteInventoryFromAllRegistries();
    }
    
    [OnEvent]
    public void OnDistrictCenterRegistryChanged(DistrictCenterRegistryChangedEvent evt) {
      if (!_isLoaded) {
        return;
      }

      // if there's only 1 district, clean up, but then there's no reason to do anything else.
      if (_districtCenterRegistry.FinishedDistrictCenters.Count <= 1) {
        foreach (var constructionSiteInventoryAssigner in _constructionSiteInventoryAssigners) {
          constructionSiteInventoryAssigner.RemoveConstructionSiteInventoryFromAllRegistries();
        }
        return;
      }
      
      UpdateDistrictInventoryRegistries(_districtCenterRegistry.FinishedDistrictCenters, _constructionSiteInventoryAssigners);
    }

    public void OnNavMeshUpdated(NavMeshUpdate navMeshUpdate) {
      if (!_isLoaded) {
        return;
      }
      
      // no reason to do anything if there's only 1 district
      if (_districtCenterRegistry.FinishedDistrictCenters.Count <= 1) {
        return;
      }
      
      UpdateDistrictInventoryRegistries(_districtCenterRegistry.FinishedDistrictCenters, _constructionSiteInventoryAssigners);
    }

    // for each district and for each construction site, if the construction site is reachable by the district
    // then add its construction inventory, otherwise remove it.
    void UpdateDistrictInventoryRegistries(IEnumerable<DistrictCenter> districts, ICollection<DistrictConstructionSiteInventoryAssigner> inventoryAssigners) {
      foreach (var districtCenter in districts) {
        var flowField = _districtMap.GetDistrictRoadSpillFlowField(districtCenter.District);
        var districtInventoryRegistry =
            districtCenter.GetComponentFast<DistrictInventoryRegistry>();
        foreach (var constructionSiteInventoryAssigner in inventoryAssigners) {
          var constructionSiteAccessible = constructionSiteInventoryAssigner.GetComponentFast<ConstructionSiteAccessible>();
          // doing the same checks that ReachableConstructionSite.IsReachableByBuilders() is doing
          var isReachableFromDistrict = !constructionSiteAccessible.Accessible.enabled
                                        || constructionSiteAccessible.Accessible.Accesses.Any(
                                            access => _nodeIdService.Contains(access)
                                                      && flowField.HasNode(
                                                          _nodeIdService.WorldToId(access)));
          if (isReachableFromDistrict) {
            constructionSiteInventoryAssigner.AddConstructionSiteInventory(districtInventoryRegistry);
          } else {
            constructionSiteInventoryAssigner.RemoveConstructionSiteInventory(districtInventoryRegistry);
          }
          
        }
      }
    }
    
    public void PostLoad() {
      _isLoaded = true;
      // no reason to do anything if there's only 1 district
      if (_districtCenterRegistry.FinishedDistrictCenters.Count <= 1) {
        return;
      }
      UpdateDistrictInventoryRegistries(_districtCenterRegistry.FinishedDistrictCenters, _constructionSiteInventoryAssigners);
    }

  }
}