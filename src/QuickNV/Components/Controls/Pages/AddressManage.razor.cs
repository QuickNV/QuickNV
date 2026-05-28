using Microsoft.AspNetCore.Components.Forms;
using Quick.Blazor.Bootstrap;
using Quick.EntityFrameworkCore.Plus;
using System.Collections;
using System.Text.Json;
using YiQiDong.Core.Utils;
using QuickNV.Model;
using QuickNV.Utils;

namespace QuickNV.Components.Controls.Pages
{
    public partial class AddressManage
    {
        private ModalAlert modalAlert;
        private ModalLoading modalLoading;

        private List<Data> _datas = new List<Data>();

        private Tree tree;
        private Data SelectedNode { get; set; }
        public bool modify_IsCreate { get; set; } = true;
        public Address modify_Model { get; set; } = null;
        public bool modify_Visiable { get; set; } = false;

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                _datas.Clear();
                await Task.Run(() =>
                {
                    foreach (var t in ConfigDbContext.CacheContext.Query<Address>(t => t.ParentId == null))
                    {
                        _datas.Add(new Data() { Model = t, ParentList = _datas });
                    }
                });
                SelectedNode = null;
            }
            catch (Exception ex)
            {
                modalAlert?.Show($"加载失败", ExceptionUtils.GetExceptionMessage(ex));
            }
        }

        public class Data
        {
            public Address Model { get; set; }
            public string Title => Model.Name;
            public List<Data> Childs { get; set; } = new List<Data>();
            public List<Data> ParentList { get; set; }
        }

        private void deleteNode()
        {
            var currentNode = SelectedNode;
            Address model = SelectedNode.Model;

            modalAlert.Show("删除确认", $"将要删除{model}，确认要继续?", () =>
            {
                modalLoading.Show("删除", "正在删除中...", true);
                Task.Run(() =>
                {
                    try
                    {
                        var isExistChildAddress = ConfigDbContext.CacheContext
                                                    .Query<Address>(t => t.ParentId == model.Id)
                                                    .Count() > 0;
                        if(isExistChildAddress)
                            throw new ApplicationException($"{model}存在子地点，请先删除子地点。");
                        ConfigDbContext.CacheContext.Remove(model);
                        modalAlert.Show("成功", "删除成功！");
                        currentNode.ParentList.Remove(currentNode);
                        SelectedNode = null;
                    }
                    catch (Exception ex)
                    {
                        modalAlert.Show("删除失败", ExceptionUtils.GetExceptionMessage(ex));
                    }
                    finally
                    {
                        modalLoading.Close();
                        InvokeAsync(StateHasChanged);
                    }
                });
            });
        }

        private void onSelectedNodeChanged(TreeNode treeNode)
        {
            SelectedNode = treeNode?.DataItem as Data;
        }

        private void expandAll()
        {
            tree.ExpandAllAsync();
        }

        private void createRootNode()
        {
            tree.SelectedNode = null;
            modify_IsCreate = true;
            modify_Model = new Address();
            modify_Visiable = true;
        }
        private void createChildNode()
        {
            modify_IsCreate = true;
            modify_Model = new Address()
            {
                ParentId = SelectedNode.Model.Id
            };
            modify_Visiable = true;
        }

        private void editNode()
        {
            modify_IsCreate = false;
            modify_Model = JsonSerializer.Deserialize<Address>(JsonSerializer.Serialize(SelectedNode.Model));
            modify_Visiable = true;
        }

        private IEnumerable GetChildren(TreeNode treeNode)
        {
            var dataItem = (Data)treeNode.DataItem;
            return ConfigDbContext.CacheContext
                .Query<Address>(t => t.ParentId == dataItem.Model.Id)
                .Select(t => new Data() { Model = t, ParentList = dataItem.Childs });
        }

        private async void OnValidateSubmit(EditContext editContext)
        {
            var model = JsonSerializer.Deserialize<Address>(JsonSerializer.Serialize(modify_Model));
            if (modify_IsCreate)
            {
                await addAddress(model);
                var list = SelectedNode?.Childs ?? _datas;
                list.Add(new Data() { Model = model, ParentList = _datas });
            }
            else
            {
                await editAddress(model);
            }
            modify_Visiable = false;
            await InvokeAsync(StateHasChanged);
        }

        private async Task editAddress(Address model)
        {
            modalLoading.Show("编辑中", "正在编辑...", true);
            try
            {
                var existModel = ConfigDbContext.CacheContext.Find(new Address() { Id = model.Id });
                if (existModel == null)
                    throw new ApplicationException($"编号为[{model.Id}]的地点不存在！");
                DataUtils.CopyPropertyValue(model, existModel);
                ConfigDbContext.CacheContext.Update(existModel);
                modalAlert.Show("成功", $"编辑{model}成功！");
                await OnParametersSetAsync();
            }
            catch (Exception ex)
            {
                modalAlert.Show($"编辑{model}失败", ExceptionUtils.GetExceptionMessage(ex));
            }
            finally
            {
                modalLoading.Close();
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task addAddress(Address model)
        {
            modalLoading.Show("添加中", "正在添加...", true);
            if (string.IsNullOrEmpty(model.Id))
                model.Id = Guid.NewGuid().ToString("N");
            try
            {
                var existModel = ConfigDbContext.CacheContext.Find(new Address() { Id = model.Id });
                if (existModel != null)
                    throw new ApplicationException($"{model}的编号已经存在！");
                ConfigDbContext.CacheContext.Add(model);
                modalAlert.Show("成功", $"添加{model}成功！");
            }
            catch (Exception ex)
            {
                modalAlert.Show($"添加{model}失败", ExceptionUtils.GetExceptionMessage(ex));
            }
            finally
            {
                modalLoading.Close();
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}
