using Timberborn.EntitySystem;

namespace GoodTracing.BatchControl {
  public class EmptyDetector : ISpecificStockpileDetector {

    public bool HasSpecificStockpile(EntityComponent entity) {
      return false;
    }

  }
}