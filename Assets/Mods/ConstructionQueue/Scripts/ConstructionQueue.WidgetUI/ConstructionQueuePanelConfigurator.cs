using Bindito.Core;

namespace ConstructionQueue.WidgetUI {
  [Context("Game")]
  public class ConstructionQueuePanelConfigurator : IConfigurator {

    public void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.Bind<ConstructionQueuePanel>().AsSingleton();
      containerDefinition.Bind<ConstructionQueueRowFactory>().AsSingleton();
    }

  }
}