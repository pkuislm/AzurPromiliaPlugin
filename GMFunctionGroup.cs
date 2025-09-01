using AzurPromiliaPlugin;
using UniverseLib.UI.Widgets.ScrollView;

namespace AzurPromiliaPlugin
{
    public class GMFunctionGroup : ICellPoolDataSource<GMFunctionGroupCell>
    {
        public ScrollPool<GMFunctionGroupCell> ScrollPool { get; set; }
        public List<KeyValuePair<string, List<GMFunctionInfo>>> Groups { get; set; } = new();
        public int ItemCount => Groups.Count;

        public GMFunctionGroup(ScrollPool<GMFunctionGroupCell> scrollPool)
        {
            ScrollPool = scrollPool;
            ScrollPool.Initialize(this);
        }

        public void OnCellBorrowed(GMFunctionGroupCell cell) { }
        public void SetCell(GMFunctionGroupCell cell, int index)
        {
            if (index < Groups.Count)
            {
                cell.Apply(Groups[index]);
            }
            else
                cell.Disable();
        }

        public void Apply(List<KeyValuePair<string, List<GMFunctionInfo>>> data)
        {
            Groups = data;
            ScrollPool.Refresh(true, false);
        }
    }
}
