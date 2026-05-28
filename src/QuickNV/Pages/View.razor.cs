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
    public partial class View : ComponentBase, IViewGridContainer, IDisposable
    {
        private ViewConfig viewConfig;

        private static List<View> viewList = new List<View>();

        public static void Show(string externalId)
        {
            View[] views = null;
            lock (viewList)
                views = viewList.Where(t => t.viewConfig.EnableAutoPlay).ToArray();
            if (views.Length == 0)
                throw new IOException($"当前没有打开的实时大屏页面或者打开的实时大屏没有启用自动播放。");
            foreach (var view in views)
                view.ShowChannel(externalId);
        }

        private ModalAlert modalAlert;
        private ModalLoading modalLoading;
        private ModalWindow modalWindow;

        private bool showLeftPanel = true;
        private string searchKeywords;
        private bool isCurrentAutoPan = false;
        private float moveSpeed = 0.5f;

        [Inject]
        private NavigationManager navigationManager { get; set; }
        private HashSet<string> addressIdHashSet = null;
        private List<Data> _datas = new List<Data>();
        private Data SelectedNode { get; set; }
        private Tree tree;

        private Model.Channel selectedChannel = null;
        private DriverContext selectedChannelDriverContext = null;

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

        private string selectedViewGrid;
        private IViewGrid viewGrid;
        private RenderFragment viewGridRenderFragment;

        public View()
        {
            lock (viewList)
                viewList.Add(this);
        }

        public void Dispose()
        {
            lock (viewList)
                viewList.Remove(this);
        }

        public void ShowChannel(string externalId)
        {
            var currentViewGrid = viewGrid;
            Channel currentChannel = null;
            var channels = ChannelManager.Instance.GetChannels(null);
            currentChannel = channels.FirstOrDefault(t => t.ExternalId != null && t.ExternalId == externalId);
            if (currentChannel == null)
                currentChannel = channels.FirstOrDefault(t => t.Id == externalId);
            if (currentChannel == null)
                throw new ArgumentException($"未找到外部编号或者通道编号为[{externalId}]的通道");

            var cellIndex = viewConfig.GetCurrentCellIndex();
            var currentCell = currentViewGrid.GetCellInfo(cellIndex);
            if (currentCell == null)
                throw new ArgumentException($"获取序号为[{cellIndex}]的单元格时返回了null");

            currentCell.Update(currentChannel, null);
            viewConfig.NextCellIndex();
            //闪烁
            currentCell.Flash();
        }

        public void SetViewGrid(IViewGrid viewGrid)
        {
            if (viewGrid.Id == this.viewGrid?.Id)
                return;
            if (this.viewGrid != null)
            {
                this.viewGrid.SelectedCellChanged -= ViewGrid_SelectedCellChanged;
            }
            this.viewGrid = viewGrid;
            this.viewGrid.SelectedCellChanged += ViewGrid_SelectedCellChanged;
            selectedChannel = this.viewGrid?.SelectedCell?.Channel;
            if (selectedChannel == null)
            {
                selectedChannelDriverContext = null;
            }
            else
            {
                var device = ConfigDbContext.CacheContext.Find(new Device(selectedChannel.DeviceId));
                selectedChannelDriverContext = device?.GetDriverContext();
            }

            List<int> allViewIndexList = new();
            for (var i = 1; i <= viewGrid.CellCount; i++)
                allViewIndexList.Add(i);
            viewConfig = new ViewConfig()
            {
                AllViewIndexs = allViewIndexList.ToArray()
            };
        }

        private void ViewGrid_SelectedCellChanged(object sender, ViewGridCellInfo cell)
        {
            tree.SelectedNode = cell?.TreeNode;
            selectedChannel = this.viewGrid?.SelectedCell?.Channel;
            InvokeAsync(StateHasChanged);
        }

        private void ChangeViewGrid(string id)
        {
            selectedViewGrid = id;
            var type = Type.GetType(selectedViewGrid);
            viewGridRenderFragment = Quick.Blazor.Bootstrap.Utils.BlazorUtils.GetRenderFragment(type, new Dictionary<string, object>()
            {
                ["Container"] = this
            });
            InvokeAsync(StateHasChanged);
        }

        private void SelectedViewGridChanged(ChangeEventArgs e)
        {
            if (e.Value is not null)
            {
                ChangeViewGrid((string)e.Value);
            }
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

            ChangeViewGrid(ViewGridManager.Instance.GetViewGrids().First().Id);
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
                        fillChildren(_datas, null, searchKeywords);
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

        private async Task setting()
        {
            modalWindow.Show("设置", new DialogParameters<ViewConfigUI>()
            {
                {
                    t=>t.ViewConfig,
                    new ViewConfig()
                    {
                        EnableAutoPlay = viewConfig.EnableAutoPlay,
                        AutoPlayViewIndexs = viewConfig.AutoPlayViewIndexs,
                        AllViewIndexs = viewConfig.AllViewIndexs
                    }
                },
                {
                    t=>t.OkAction,
                    new Action<ViewConfig>(model =>
                    {
                        viewConfig.EnableAutoPlay = model.EnableAutoPlay;
                        viewConfig.AutoPlayViewIndexs = model.AutoPlayViewIndexs;
                        modalWindow.Close();
                    })
                }
            });
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
            }
            ;

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

        private async Task moveUp() => await SendPtzCommandToSelectedChannelAsync(PTZCommandType.Up, moveSpeed);
        private async Task moveDown() => await SendPtzCommandToSelectedChannelAsync(PTZCommandType.Down, moveSpeed);
        private async Task moveLeft() => await SendPtzCommandToSelectedChannelAsync(PTZCommandType.Left, moveSpeed);
        private async Task moveRight() => await SendPtzCommandToSelectedChannelAsync(PTZCommandType.Right, moveSpeed);
        private async Task zoomIn() => await SendPtzCommandToSelectedChannelAsync(PTZCommandType.ZoomIn, moveSpeed);
        private async Task zoomOut() => await SendPtzCommandToSelectedChannelAsync(PTZCommandType.ZoomOut, moveSpeed);
        private async Task focusFar() => await SendPtzCommandToSelectedChannelAsync(PTZCommandType.FocusFar, moveSpeed);
        private async Task focusNear() => await SendPtzCommandToSelectedChannelAsync(PTZCommandType.FocusNear, moveSpeed);
        private async Task irisIn() => await SendPtzCommandToSelectedChannelAsync(PTZCommandType.IrisOpen, moveSpeed);
        private async Task irisOut() => await SendPtzCommandToSelectedChannelAsync(PTZCommandType.IrisClose, moveSpeed);

        private Task SendPtzCommandToSelectedChannelAsync(PTZCommandType cmdType, float speed = 0.5f)
        {
            if (selectedChannel == null
                || selectedChannelDriverContext == null)
                return Task.CompletedTask;
            return selectedChannelDriverContext.PtzControl(selectedChannel.DeviceId, selectedChannel.Id, cmdType, speed);
        }

        private async Task autoPan()
        {
            if (isCurrentAutoPan)
            {
                await stopMove();
            }
            else
            {
                await moveRight();
                isCurrentAutoPan = true;
            }
        }

        private async Task stopMove()
        {
            await SendPtzCommandToSelectedChannelAsync(PTZCommandType.Stop);
            isCurrentAutoPan = false;
        }
    }
}
