using Bindito.Core;
using Timberborn.BatchControl;

namespace GoodTracing.BatchControl {
  [Context("Game")]
  public class GoodTracingBatchControlConfigurator : IConfigurator {

    public void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.Bind<GoodTracingBatchControlRowFactory>().AsSingleton();
      containerDefinition.Bind<InputGoodTracingBatchControlTab>().AsSingleton();
      containerDefinition.MultiBind<BatchControlModule>()
          .ToProvider<BatchControlModuleProvider>()
          .AsSingleton();
    }

    class BatchControlModuleProvider : IProvider<BatchControlModule> {

      readonly InputGoodTracingBatchControlTab _inputGoodTracingBatchControlTab;

      public BatchControlModuleProvider(InputGoodTracingBatchControlTab inputGoodTracingBatchControlTab) {
        _inputGoodTracingBatchControlTab = inputGoodTracingBatchControlTab;
      }

      public BatchControlModule Get() {
        var builder = new BatchControlModule.Builder();
        builder.AddTab(_inputGoodTracingBatchControlTab, 95);
        return builder.Build();
      }
    }
  }
}