using Bindito.Core;
using Timberborn.GameDistricts;
using Timberborn.TemplateSystem;

namespace DistrictImportUnpauseFix.DistributionSystem {
  [Context("Game")]
  public class DistributionSystemConfigurator : IConfigurator {

    public void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
    }

    static TemplateModule ProvideTemplateModule()
    {
      var builder = new TemplateModule.Builder();
      builder.AddDecorator<DistrictBuilding, DistrictDistributableGoodRefresher>();
      return builder.Build();
    }

  }
}