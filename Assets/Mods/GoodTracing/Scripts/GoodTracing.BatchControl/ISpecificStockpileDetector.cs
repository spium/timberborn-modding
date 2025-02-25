using Timberborn.EntitySystem;

namespace GoodTracing.BatchControl {
  /// <summary>
  /// Detects whether an entity has a Battery.Pantry.SpecificStockpileSystem.SpecificStockpile component attached to it
  /// </summary>
  public interface ISpecificStockpileDetector {
    bool HasSpecificStockpile(EntityComponent entity);
  }
}