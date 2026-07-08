using QuickNV.Components.Controls.ViewGrids;

namespace QuickNV.Components.Pages;

public class ViewConfig
{
    public bool EnableAutoPlay { get; set; } = true;
    public int[] AutoPlayViewIndexs { get; set; } = [1];
    public int[] AllViewIndexs { get; set; }
    public int AutoShowIndex { get; set; } = 0;

    public int GetCurrentCellIndex()
    {
        if (AutoShowIndex >= AutoPlayViewIndexs.Length)
            AutoShowIndex = AutoPlayViewIndexs.Length - 1;

        return AutoPlayViewIndexs[AutoShowIndex];
    }

    public void NextCellIndex()
    {
        AutoShowIndex++;
        if (AutoShowIndex >= AutoPlayViewIndexs.Length)
            AutoShowIndex = 0;
    }
}
