using System.Collections.Generic;
using System.Linq;
using Timberborn.BatchControl;
using Timberborn.Buildings;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Stockpiles;

namespace GoodTracing.BatchControl {
  public abstract class GoodTracingBatchControlTab : BatchControlTab {

    readonly IGoodService _goodService;
    readonly BatchControlRowGroupFactory _batchControlRowGroupFactory;
    readonly GoodTracingBatchControlRowFactory _goodTracingBatchControlRowFactory;
    
    public GoodTracingBatchControlTab(VisualElementLoader visualElementLoader,
                                      BatchControlDistrict batchControlDistrict,
                                      IGoodService goodService,
                                      BatchControlRowGroupFactory batchControlRowGroupFactory,
                                      GoodTracingBatchControlRowFactory
                                          goodTracingBatchControlRowFactory
    ) :
        base(visualElementLoader, batchControlDistrict) {
      _goodService = goodService;
      _batchControlRowGroupFactory = batchControlRowGroupFactory;
      _goodTracingBatchControlRowFactory = goodTracingBatchControlRowFactory;
    }
    
    public override bool RemoveEmptyRowGroups => true;
    
    public override IEnumerable<BatchControlRowGroup> GetRowGroups(
        IEnumerable<EntityComponent> entities) {
      var groups = _goodService.Goods.ToDictionary(goodId => goodId,
                                                   goodId => {
                                                     var good = _goodService.GetGood(goodId);
                                                     return _batchControlRowGroupFactory
                                                         .Create(good.PluralDisplayName,
                                                                 good.PluralDisplayName);
                                                   });

      foreach (var entity in entities
                   .Where(ShouldDisplayEntity)
                   .Where(e => e.GetComponentFast<Building>())
                   .Where(e => !e.GetComponentFast<Stockpile>())
                   .Where(e => e.GetComponentFast<Inventories>()?.HasEnabledInventories ?? false)) {

        var inventories = entity.GetComponentFast<Inventories>();
        AddRows(entity, inventories.EnabledInventories.SelectMany(GetGoods).Distinct(), groups, _goodTracingBatchControlRowFactory);
      }

      return groups.Values;
    }

    protected virtual bool ShouldDisplayEntity(EntityComponent entity) {
      return true;
    }
    
    protected abstract IEnumerable<string> GetGoods(Inventory inventory);

    protected abstract void AddRows(EntityComponent entity, IEnumerable<string> goods,
                                    IDictionary<string, BatchControlRowGroup> rowGroups, GoodTracingBatchControlRowFactory rowFactory);

  }
}