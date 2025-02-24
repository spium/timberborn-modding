using Bindito.Core;
using System.Linq;
using Timberborn.BatchControl;
using Timberborn.Modding;

namespace GoodTracing.BatchControl {
  [Context("Game")]
  public class GoodTracingBatchControlConfigurator : IConfigurator {

    public void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.Bind<GoodTracingBatchControlRowFactory>().AsSingleton();
      containerDefinition.Bind<GoodTracingWorkplacesBatchControlRowFactory>().AsSingleton();
      containerDefinition.Bind<GoodTracingAttractionsBatchControlRowFactory>().AsSingleton();
      containerDefinition.Bind<GoodTracingHousingBatchControlRowFactory>().AsSingleton();
      containerDefinition.Bind<GoodTracingInventoryCapacityBatchControlRowItemFactory>()
          .AsSingleton();
      containerDefinition.Bind<GoodTracingBatchControlTabEnteredFinishedStateWorkaround>()
          .AsSingleton();
      containerDefinition.Bind<InputGoodTracingBatchControlTab>().AsSingleton();
      containerDefinition.Bind<OutputGoodTracingBatchControlTab>().AsSingleton();
      containerDefinition.MultiBind<BatchControlModule>()
          .ToProvider<BatchControlModuleProvider>()
          .AsSingleton();
      
      containerDefinition.Bind<ISpecificStockpileDetector>()
          .ToProvider<SpecificStockpileDetectorProvider>()
          .AsSingleton();
    }

    class BatchControlModuleProvider : IProvider<BatchControlModule> {

      readonly InputGoodTracingBatchControlTab _inputGoodTracingBatchControlTab;
      readonly OutputGoodTracingBatchControlTab _outputGoodTracingBatchControlTab;

      public BatchControlModuleProvider(
          InputGoodTracingBatchControlTab inputGoodTracingBatchControlTab,
          OutputGoodTracingBatchControlTab outputGoodTracingBatchControlTab) {
        _inputGoodTracingBatchControlTab = inputGoodTracingBatchControlTab;
        _outputGoodTracingBatchControlTab = outputGoodTracingBatchControlTab;
      }

      public BatchControlModule Get() {
        var builder = new BatchControlModule.Builder();
        builder.AddTab(_inputGoodTracingBatchControlTab, 95);
        builder.AddTab(_outputGoodTracingBatchControlTab, 100);
        return builder.Build();
      }

    }

    class SpecificStockpileDetectorProvider : IProvider<ISpecificStockpileDetector> {

      readonly ModRepository _modRepository;

      public SpecificStockpileDetectorProvider(ModRepository modRepository) {
        _modRepository = modRepository;
      }

      bool IsPantryModEnabled =>
          _modRepository.EnabledMods.Any(m => m.Manifest.Id == "Battery.Pantry");

      public ISpecificStockpileDetector Get() {
        return IsPantryModEnabled ? new PantryDetector() : new EmptyDetector();
      }

    }
  }
}