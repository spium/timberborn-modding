using Bindito.Core;
using Timberborn.ConstructionSites;
using Timberborn.TemplateSystem;

namespace ConstructionQueue.ConstructionSites {
  [Context("Game")]
  public class ConstructionSitesConfigurator : IConfigurator {

    public void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.Bind<ConstructionQueueRegistry>().AsSingleton();
      containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
    }

    static TemplateModule ProvideTemplateModule() {
      var builder = new TemplateModule.Builder();
      builder.AddDecorator<ConstructionRegistrar, ConstructionQueueRegistrar>();
      return builder.Build();
    }

  }
}