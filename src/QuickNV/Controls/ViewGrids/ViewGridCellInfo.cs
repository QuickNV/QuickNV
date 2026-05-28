using Quick.Blazor.Bootstrap;

namespace QuickNV.Controls.ViewGrids
{
    public class ViewGridCellInfo
    {
        private IViewGrid viewGrid;
        public int Index { get; private set; }
        public Model.Channel Channel { get; private set; }
        public TreeNode TreeNode { get; private set; }

        public ViewGridCellInfo(IViewGrid viewGrid, int index)
        {
            this.viewGrid = viewGrid;
            Index = index;
        }

        public void Update(Model.Channel channel, TreeNode treeNode)
        {
            Channel = channel;
            TreeNode = treeNode;
        }

        public void Flash()
        {
            viewGrid.Flash(Index);
        }
    }
}
