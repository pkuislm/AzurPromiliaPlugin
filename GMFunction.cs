using AzurPromiliaPlugin;
using UniverseLib.UI.Widgets.ScrollView;

namespace AzurPromiliaPlugin
{
    public class GMFunction : ICellPoolDataSource<GMFunctionCell>
    {
        public ScrollPool<GMFunctionCell> ScrollPool { get; set; }
        public List<GMFunctionInfo> Functions { get; set; } = new();
        public int ItemCount => Functions.Count;

        public GMFunction(ScrollPool<GMFunctionCell> scrollPool)
        {
            ScrollPool = scrollPool;
            ScrollPool.Initialize(this);
        }

        public void OnCellBorrowed(GMFunctionCell cell) { }
        public void SetCell(GMFunctionCell cell, int index)
        {
            if (index < Functions.Count)
            {
                cell.Apply(Functions[index]);
            }
            else
                cell.Disable();
        }

        public void Apply(List<GMFunctionInfo> data)
        {
            Functions = data;
            ScrollPool.Refresh(true, false);
        }
    }
}
