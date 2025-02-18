using System.Collections.Generic;
using System.Linq;
using Timberborn.BatchControl;
using Timberborn.Buildings;
using Timberborn.BuildingsBlocking;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.Goods;
using Timberborn.InputSystemUI;
using Timberborn.InventorySystem;
using Timberborn.SingletonSystem;
using Timberborn.Stockpiles;
using UnityEngine.UIElements;

namespace GoodTracing.BatchControl {
  public abstract class GoodTracingBatchControlTab : BatchControlTab, ILoadableSingleton {

    readonly IGoodService _goodService;
    readonly BatchControlRowGroupFactory _batchControlRowGroupFactory;
    readonly GoodTracingBatchControlRowFactory _goodTracingBatchControlRowFactory;
    readonly BindableToggleFactory _bindableToggleFactory;

    VisualElement _headerVisualElement;
    BindableToggle _showPausedToggle;
    bool _showPaused;

    protected GoodTracingBatchControlTab(VisualElementLoader visualElementLoader,
                                         BatchControlDistrict batchControlDistrict,
                                         IGoodService goodService,
                                         BatchControlRowGroupFactory batchControlRowGroupFactory,
                                         GoodTracingBatchControlRowFactory
                                             goodTracingBatchControlRowFactory,
                                         BindableToggleFactory bindableToggleFactory
    ) :
        base(visualElementLoader, batchControlDistrict) {
      _goodService = goodService;
      _batchControlRowGroupFactory = batchControlRowGroupFactory;
      _goodTracingBatchControlRowFactory = goodTracingBatchControlRowFactory;
      _bindableToggleFactory = bindableToggleFactory;
    }
    
    
    public override bool RemoveEmptyRowGroups => true;

    public void Load() {
      _headerVisualElement =
          _visualElementLoader.LoadVisualElement("GoodTracing/GoodTracingBatchControlTabHeader");
      var toggle = _headerVisualElement.Q<Toggle>("ShowPaused");
      _showPausedToggle = _bindableToggleFactory.Create(toggle, "GoodTracingTabShowPaused",
                                                        value => { 
                                                          _showPaused = value;
                                                          // TODO is there a better way than to refresh the whole UI?
                                                          IsDirty = true;
                                                        },
                                                        () => _showPaused);
      _showPausedToggle.Disable();
    }
    
    public override VisualElement GetHeader() {
      return _headerVisualElement;
    }

    public override void Show() {
      _showPausedToggle.Bind();
      _showPausedToggle.Enable();
    }

    public override void Hide() {
      _showPausedToggle.Unbind();
      _showPausedToggle.Disable();
    }

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

        var pausable = entity.GetComponentFast<PausableBuilding>();
        if (!_showPaused && pausable != null && pausable.Paused) {
          continue;
        }
        
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