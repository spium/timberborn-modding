using Bindito.Core;
using Timberborn.ConstructionSites;
using Timberborn.TemplateSystem;

namespace DistrictImportOnConstructionSites.ConstructionSites {
  [Context("Game")]
  public class DistrictConstructionSitesConfigurator : IConfigurator {

    public void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
      containerDefinition.Bind<DistrictConstructionSitesRegistry>().AsSingleton();
    }

    static TemplateModule ProvideTemplateModule()
    {
      var builder = new TemplateModule.Builder();
      builder.AddDecorator<ConstructionSite, DistrictConstructionSiteInventoryAssigner>();
      return builder.Build();
    }
    
  }
}