using System;
using Microsoft.AspNetCore.Components;

namespace QuickNV.Pages;

public partial class ViewConfigUI : ComponentBase
{
    [Parameter]
    public ViewConfig ViewConfig { get; set; }

    [Parameter]
    public Action<ViewConfig> OkAction { get; set; }

    private SelectViewInfo[] selectViews;
    public class SelectViewInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Checked { get; set; }
    }

    private void checkView(SelectViewInfo item)
    {
        item.Checked = !item.Checked;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        selectViews = ViewConfig.AllViewIndexs.Select(t => new SelectViewInfo()
        {
            Id = t,
            Name = $"View{t}",
            Checked = ViewConfig.AutoPlayViewIndexs.Contains(t)
        }).ToArray();
    }

    private void Ok()
    {
        ViewConfig.AutoPlayViewIndexs = selectViews.Where(t => t.Checked).Select(t => t.Id).ToArray();
        OkAction?.Invoke(ViewConfig);
    }
}
