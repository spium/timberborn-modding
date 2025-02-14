using Bindito.Core;
using Timberborn.BatchControl;

namespace GoodTracing.BatchControl {
  [Context("Game")]
  public class GoodTracingBatchControlConfigurator : IConfigurator {

    public void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.Bind<GoodTracingBatchControlRowFactory>().AsSingleton();
      containerDefinition.Bind<InputGoodTracingBatchControlTab>().AsSingleton();
      containerDefinition.Bind<OutputGoodTracingBatchControlTab>().AsSingleton();
      containerDefinition.MultiBind<BatchControlModule>()
          .ToProvider<BatchControlModuleProvider>()
          .AsSingleton();
    }

    class BatchControlModuleProvider : IProvider<BatchControlModule> {

      readonly InputGoodTracingBatchControlTab _inputGoodTracingBatchControlTab;
      readonly OutputGoodTracingBatchControlTab _outputGoodTracingBatchControlTab;

      public BatchControlModuleProvider(InputGoodTracingBatchControlTab inputGoodTracingBatchControlTab, OutputGoodTracingBatchControlTab outputGoodTracingBatchControlTab) {
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
  }
}