using Quick.Blazor.Bootstrap;
using Quick.EntityFrameworkCore.Plus;
using YiQiDong.Core.Utils;

namespace QuickNV.Controls.Pages
{
    public partial class MediaServerManage : IDisposable
    {
        private string searchKeywords;
        private ModalLoading modalLoading;
        private ModalAlert modalAlert;
        private ModalWindow modalWindow;
        private void refresh()
        {
            InvokeAsync(StateHasChanged);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
                Core.MediaServerManager.Instance.StateChanged += VideoDeviceManager_StateChanged;
        }

        private void Create()
        {
            modalWindow.Show<Controls.MediaServerCreateControl>("添加媒体服务器",
                Controls.MediaServerCreateControl.PrepareParameter(
                    null,
                    t =>
                    {
                        modalLoading.Show("添加媒体服务器", $"正在添加媒体服务器[{t.Name}]...", true, null);
                        Task.Run(() =>
                        {
                            try
                            {
                                if (ConfigDbContext.CacheContext.Find(t) != null)
                                    throw new ApplicationException($"已经存在编号为[{t.Id}]的媒体服务器");
                                ConfigDbContext.CacheContext.Add(t);
                                Core.MediaServerManager.Instance.AddMediaServer(t);
                                modalWindow.Close();
                            }
                            catch (Exception ex)
                            {
                                modalAlert.Show("错误", $"添加媒体服务器[{t.Name}]时出错！原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                            }
                            modalLoading.Close();
                        });
                    })
                );
        }

        private void Edit(Core.MediaServerContext context)
        {
            var device = ConfigDbContext.CacheContext.Find(new Model.MediaServer() { Id = context.Model.Id });
            modalWindow.Show<Controls.MediaServerCreateControl>("编辑媒体服务器",
                Controls.MediaServerCreateControl.PrepareParameter(
                    device,
                    t =>
                    {
                        modalLoading.Show("编辑媒体服务器", $"正在编辑媒体服务器[{t.Name}]...", true, null);
                        Task.Run(() =>
                        {
                            try
                            {
                                ConfigDbContext.CacheContext.Update(t);

                                Core.MediaServerManager.Instance.RemoveMediaServer(t.Id);
                                Core.MediaServerManager.Instance.AddMediaServer(t);
                                modalWindow.Close();
                            }
                            catch (Exception ex)
                            {
                                modalAlert.Show("错误", $"编辑媒体服务器[{t.Name}]时出错！原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                            }
                            modalLoading.Close();
                        });
                    })
                );
        }

        private void Delete(Core.MediaServerContext context)
        {
            var device = ConfigDbContext.CacheContext.Find(new Model.MediaServer() { Id = context.Model.Id });
            modalAlert.Show(
                "删除确认",
                $"确定要删除媒体服务器[{device.Name}]?",
                () =>
                {
                    modalLoading.Show("删除媒体服务器", $"正在删除媒体服务器[{device.Name}]...", true, null);
                    Task.Run(() =>
                    {
                        try
                        {
                            ConfigDbContext.CacheContext.Remove(device);
                            Core.MediaServerManager.Instance.RemoveMediaServer(device.Id);
                            modalAlert.Show("信息", $"删除媒体服务器[{device.Name}]成功!");
                        }
                        catch (Exception ex)
                        {
                            modalAlert.Show("错误", $"删除视频设备[{device.Name}]时出错！原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                        }
                        modalLoading.Close();
                    });
                },
            null);
        }

        private void DisplayMediaInfos(Core.MediaServerContext context)
        {
            modalWindow.Show<Controls.MediaServerMediaInfosView>($"媒体流 - {context.Model.Name}",
                Controls.MediaServerMediaInfosView.PrepareParameter(context)
            );
        }

        private void DisplayLogs(Core.MediaServerContext context)
        {
            modalWindow.Show<Controls.LogView>($"日志 - {context.Model.Name}",
                Controls.LogView.PrepareParameter(context)
            );
        }

        private void VideoDeviceManager_StateChanged(object sender, EventArgs e)
        {
            refresh();
        }

        public void Dispose()
        {
            Core.MediaServerManager.Instance.StateChanged -= VideoDeviceManager_StateChanged;
        }
    }
}
