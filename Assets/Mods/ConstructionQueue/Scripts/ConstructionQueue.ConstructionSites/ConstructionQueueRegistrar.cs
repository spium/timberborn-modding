using Bindito.Core;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.ConstructionSites;

namespace ConstructionQueue.ConstructionSites {
  public class ConstructionQueueRegistrar : BaseComponent, IUnfinishedStateListener {

    ConstructionQueueRegistry _registry;
    
    ConstructionJob _constructionJob;

    void Awake() {
      _constructionJob = GetComponentFast<ConstructionJob>();
    }

    [Inject]
    public void InjectDependencies(ConstructionQueueRegistry registry) {
      _registry = registry;
    }

    public void OnEnterUnfinishedState() {
      _registry.JobAdded(_constructionJob);
    }

    public void OnExitUnfinishedState() {
      _registry.JobRemoved(_constructionJob);
    }

  }
}