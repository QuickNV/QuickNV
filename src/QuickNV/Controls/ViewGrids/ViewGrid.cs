using Quick.Blazor.Bootstrap;
using QuickNV.Core;

namespace QuickNV.Controls.ViewGrids
{
    public class ViewGrid : IViewGrid
    {
        private Dictionary<int, ViewGridCellInfo> cellDict = new Dictionary<int, ViewGridCellInfo>();
        public string Id { get; private set; }
        public string Name { get; private set; }
        public int CellCount { get; private set; }
        public ViewGridCellInfo SelectedCell { get; private set; }
        private Dictionary<int, ViewGridCell> viewGridCellDict = new();

        public event EventHandler<ViewGridCellInfo> SelectedCellChanged;

        public ViewGrid(string id, string name, int cellCount)
        {
            Id = id;
            Name = name;
            CellCount = cellCount;
            for (var i = 1; i <= cellCount; i++)
                cellDict[i] = new ViewGridCellInfo(this, i);
            ChangeSelectedCell(1);
        }

        public ViewGridCellInfo GetCellInfo(int index)
        {
            if (cellDict.ContainsKey(index))
                return cellDict[index];
            return null;
        }

        public void ChangeSelectedCell(int index)
        {
            if (cellDict.ContainsKey(index))
                SelectedCell = cellDict[index];
            else
                SelectedCell = null;
            SelectedCellChanged?.Invoke(this, SelectedCell);
        }

        public string GetCellCssClass(int index)
        {
            var selectedCell = SelectedCell;
            if (selectedCell == null || selectedCell.Index != index)
                return null;
            return "Active";
        }

        public Func<Model.Channel, string> GetChannelViewUrlFunc { get; set; } = channel => $"live.html?DeviceId={channel.DeviceId}&ChannelId={channel.Id}";

        public string GetChannelViewUrl(int index)
        {
            var cell = GetCellInfo(index);
            if (cell.Channel == null)
                return "about:blank";
            return GetChannelViewUrlFunc(cell.Channel);
        }

        public ViewGridCellInfo GetCellInfoByChannel(Model.Channel channel)
        {
            return cellDict.Values.FirstOrDefault(t => t.Channel == channel);
        }

        public void Flash(int index)
        {
            if (!viewGridCellDict.TryGetValue(index, out var viewGridCell))
                return;
            _ = viewGridCell.FlashAsync();
        }

        internal void SetViewGridCell(int index, ViewGridCell viewGridCell)
        {
            viewGridCellDict[index] = viewGridCell;
        }
    }
}
