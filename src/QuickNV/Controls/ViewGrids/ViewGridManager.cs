namespace QuickNV.Controls.ViewGrids
{
    public class ViewGridManager
    {
        public static ViewGridManager Instance { get; } = new ViewGridManager();

        private List<IViewGrid> viewGrids = new List<IViewGrid>();
        public void Init()
        {
            viewGrids.Clear();
            viewGrids.Add(new Grid1().ViewModel);
            viewGrids.Add(new Grid4().ViewModel);
            viewGrids.Add(new Grid6().ViewModel);
            viewGrids.Add(new Grid9().ViewModel);
            viewGrids.Add(new Grid16().ViewModel);
            viewGrids.Add(new Grid24().ViewModel);
        }

        public IViewGrid[] GetViewGrids() => viewGrids.ToArray();
    }
}
