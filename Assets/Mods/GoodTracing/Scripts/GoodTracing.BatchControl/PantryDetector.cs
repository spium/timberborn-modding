using Battery.Pantry.SpecificStockpileSystem;
using Timberborn.EntitySystem;

namespace GoodTracing.BatchControl {
  public class PantryDetector : ISpecificStockpileDetector {

    public bool HasSpecificStockpile(EntityComponent entity) {
      return entity.GetComponentFast<SpecificStockpile>();
    }

  }
}