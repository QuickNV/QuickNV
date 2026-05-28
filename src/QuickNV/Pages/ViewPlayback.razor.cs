using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap;
using Quick.EntityFrameworkCore.Plus;
using System.Web;
using YiQiDong.Core.Utils;
using QuickNV.Controls.ViewGrids;
using QuickNV.Core;
using QuickNV.Driver.Protocol.QpModels;
using QuickNV.Model;

namespace QuickNV.Pages
{
    public partial class ViewPlayback : ComponentBase, IViewGridContainer
    {
        private ModalAlert modalAlert;
        private ModalLoading modalLoading;

        private bool showLeftPanel = true;
        private string searchKeywords;

        [Inject]
        private NavigationManager navigationManager { get; set; }
        private HashSet<string> addressIdHashSet = null;
        private List<Data> _datas = new List<Data>();
        private Data SelectedNode { get; set; }
        private Tree tree;

        private Model.Channel selectedChannel = null;       

        public class Data
        {
            public Address Address { get; set; }
            public Model.Channel Channel { get; set; }

            public bool IsLeaf => Address == null;
            public string Title => IsLeaf ? Channel.Name : Address.Name;
            public List<Data> Childs { get; set; } = new List<Data>();
            public List<Data> ParentList { get; set; }

            public override int GetHashCode()
            {
                if (IsLeaf)
                    return Channel.GetHashCode();
                return Address.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                var objB = obj as Data;
                if (objB == null)
                    return false;

                if (IsLeaf != objB.IsLeaf)
                    return false;

                if (IsLeaf)
                    return Channel.Equals(objB.Channel);

                return Address.Equals(objB.Address);
            }
        }

        private IViewGrid viewGrid;
        private RenderFragment viewGridRenderFragment;

        public void SetViewGrid(IViewGrid viewGrid)
        {
            if (this.viewGrid != null)
            {
                this.viewGrid.SelectedCellChanged -= ViewGrid_SelectedCellChanged;
            }
            this.viewGrid = viewGrid;
            this.viewGrid.SelectedCellChanged += ViewGrid_SelectedCellChanged;
            selectedChannel = this.viewGrid?.SelectedCell?.Channel;
        }

        private void ViewGrid_SelectedCellChanged(object sender, ViewGridCellInfo cell)
        {
            tree.SelectedNode = cell?.TreeNode;
            selectedChannel = this.viewGrid?.SelectedCell?.Channel;
            InvokeAsync(StateHasChanged);
        }

        private void onSelectedNodeChanged(TreeNode treeNode)
        {
            SelectedNode = treeNode?.DataItem as Data;
        }

        private TreeNode[] GetAddressChannelNodes(TreeNode addressNode)
        {
            List<TreeNode> list = new List<TreeNode>();
            foreach (var child in addressNode.ChildNodes)
            {
                var item = child.DataItem as Data;
                if (item.IsLeaf)
                    list.Add(child);
                else
                    list.AddRange(GetAddressChannelNodes(child));
            }
            return list.ToArray();
        }

        private void onTreeNodeDblClicked(TreeNode treeNode)
        {
            var item = (Data)treeNode.DataItem;
            if (item.IsLeaf)
            {
                var channelCell = viewGrid.GetCellInfoByChannel(item.Channel);
                //如果显示网格中不包含此通道，则将当前选中单元格显示点击的通道
                if (channelCell == null)
                {
                    viewGrid.SelectedCell?.Update(item.Channel, treeNode);

                }
                //如果显示网格中已经包含此通道，则将单元格置空
                else
                {
                    channelCell.Update(null, null);
                }
            }
            else
            {
                treeNode.ExpandAll(true);
                var channelNodes = GetAddressChannelNodes(treeNode);
                for (var i = 0; i < viewGrid.CellCount; i++)
                {
                    var cell = viewGrid.GetCellInfo(i + 1);
                    cell.Update(null, null);
                    if (i >= channelNodes.Length)
                        continue;
                    var channelNode = channelNodes[i];
                    cell.Update(((Data)channelNode.DataItem).Channel, channelNode);
                }
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            var uri = new Uri(navigationManager.Uri);
            var queryDict = HttpUtility.ParseQueryString(uri.Query);
            var addressIds = queryDict.Get("AddressIds");
            if (!string.IsNullOrEmpty(addressIds))
                addressIdHashSet = addressIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .ToHashSet();

            var grid1 = new Grid1();
            grid1.ViewModel.GetChannelViewUrlFunc = channel => $"playback.html?DeviceId={channel.DeviceId}&ChannelId={channel.Id}";
            viewGrid = grid1.ViewModel;
            viewGridRenderFragment = Quick.Blazor.Bootstrap.Utils.BlazorUtils.GetRenderFragment(grid1.GetType(), new Dictionary<string, object>()
            {
                [nameof(Grid1.Container)] = this,
                [nameof(Grid1.ViewModel)] = grid1.ViewModel
            });
            await refresh();
        }

        private async Task refresh()
        {
            modalLoading?.Show("搜索中", "正在搜索...", true);
            try
            {
                if (tree != null)
                    await tree.CollapseAllAsync();
                await Task.Run(() =>
                {
                    lock (this)
                    {
                        _datas.Clear();
                        fillChildren(_datas, null,searchKeywords);
                    }
                });
                await Task.Delay(100);
                SelectedNode = null;
                if (tree != null)
                    await tree.ExpandAllAsync();
            }
            catch (Exception ex)
            {
                modalAlert?.Show($"加载失败", ExceptionUtils.GetExceptionMessage(ex));
            }
            modalLoading?.Close();
        }

        private void fillChildren(List<Data> list, string addressId, string searchKeywords)
        {
            //添加子地点
            foreach (var t in ConfigDbContext.CacheContext
                .Query<Address>(t =>
                t.ParentId == addressId
                && (addressIdHashSet == null || addressIdHashSet.Contains(t.Id))
                ))
            {
                var childAddress = new Data() { Address = t, ParentList = list };
                fillChildren(childAddress.Childs, t.Id, searchKeywords);
                if (childAddress.Childs.Count == 0)
                    continue;
                list.Add(childAddress);
            };

            //添加绑定此地点的通道                    
            if (addressId != null)
            {
                foreach (var channel in ConfigDbContext.CacheContext.Query<Model.Channel>())
                {
                    if (addressId != channel.AddressId)
                        continue;

                    if (!string.IsNullOrEmpty(searchKeywords)
                           && !channel.Name.Contains(searchKeywords))
                        continue;

                    list.Add(new Data() { Channel = channel, ParentList = list });
                }
            }
        }
    }
}
