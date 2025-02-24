using Timberborn.BlockSystem;
using Timberborn.SingletonSystem;

namespace GoodTracing.BatchControl {
  // This class is a workaround to have my GoodTracingBatchControlTab be able to listen to EnteredFinishedStateEvent
  public class GoodTracingBatchControlTabEnteredFinishedStateWorkaround : ILoadableSingleton {

    readonly InputGoodTracingBatchControlTab _inputTab;
    readonly OutputGoodTracingBatchControlTab _outputTab;
    readonly EventBus _eventBus;

    public GoodTracingBatchControlTabEnteredFinishedStateWorkaround(
        InputGoodTracingBatchControlTab inputTab, OutputGoodTracingBatchControlTab outputTab, EventBus eventBus) {
      _inputTab = inputTab;
      _outputTab = outputTab;
      _eventBus = eventBus;
    }

    public void Load() {
      _eventBus.Register(this);
    }

    [OnEvent]
    public void OnEnterFinishedState(EnteredFinishedStateEvent evt) {
      _inputTab.OnEnteredFinishedStateEvent(evt);
      _outputTab.OnEnteredFinishedStateEvent(evt);
    }

  }
}