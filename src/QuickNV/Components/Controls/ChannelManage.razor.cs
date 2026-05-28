using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap;
using Quick.EntityFrameworkCore.Plus;
using YiQiDong.Core.Utils;
using QuickNV.Core;

namespace QuickNV.Components.Controls
{
    public partial class ChannelManage
    {
        private ModalLoading modalLoading;
        private ModalAlert modalAlert;
        private ModalWindow modalWindow;
        private ToastStack toastStack;

        private string searchKeywords;
        [Parameter]
        public Model.Device Device { get; set; }
        [Parameter]
        public Action ChannelChanged { get; set; }

        private DriverContext driverContext;
        private Model.Channel[] channels;

        private int _Offset = 0;
        private int Offset
        {
            get { return _Offset; }
            set
            {
                _Offset = value;
                InvokeAsync(StateHasChanged);
            }
        }
        private int PageSize { get; set; } = 9;

        private void search()
        {
            channels = Device.GetChannels();
            if (!string.IsNullOrEmpty(searchKeywords))
                channels = channels.Where(t => t.Name.Contains(searchKeywords)).ToArray();
            InvokeAsync(StateHasChanged);
        }

        public static DialogParameters<ChannelManage> PrepareParameter(Model.Device device, Action channelChanged)
        {
            return new DialogParameters<ChannelManage>()
            {
                {t=>t.Device,device},
                {t=>t.ChannelChanged,channelChanged}
            };
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                driverContext = Device.GetDriverContext();
                search();
            }
        }

        private void Snapshot(Model.Channel channel)
        {
            modalWindow.Show<ImageView>(
                $"快照 - {channel.Name} - {Device.Name}",
                ImageView.PrepareParameter(channel.GetSnapshotUrl()));
        }

        private void Live(Model.Channel channel)
        {
            modalWindow.Show<ChannelLiveView>(
                $"实时预览 - {channel.Name} - {Device.Name}",
                ChannelLiveView.PrepareParameter(channel));
        }

        private void Playback(Model.Channel channel)
        {
            modalWindow.Show<ChannelPlaybackManage>(
                $"回放查看 - {channel.Name} - {Device.Name}",
                ChannelPlaybackManage.PrepareParameter(channel));
        }
        
        private void DisplayLogs(Model.Channel channel)
        {
            modalWindow.Show<Controls.LogView>($"日志 - {channel.Name}",
                Controls.LogView.PrepareParameter(channel.LogContext)
            );
        }
        private void Create()
        {
            modalWindow.Show<ChannelCreateControl>("添加通道",
                ChannelCreateControl.PrepareParameter(
                    Device,
                    null,
                    t =>
                    {
                        t.DeviceId = Device.Id;
                        modalLoading.Show("添加通道", $"正在添加通道[{t.Name}]...", true, null);
                        Task.Run(async () =>
                        {
                            try
                            {
                                await ChannelManager.Instance.AddChannel(driverContext, Device, t);
                                modalWindow.Close();
                            }
                            catch (Exception ex)
                            {
                                modalAlert.Show("错误", $"添加通道[{t.Name}]时出错！原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                            }
                            modalLoading.Close();
                            search();
                            await InvokeAsync(StateHasChanged);
                            ChannelChanged?.Invoke();
                        });
                    })
                );
        }


        private void Import()
        {
            modalWindow.Show<Controls.ChannelImportControl>("导入通道",
                Controls.ChannelImportControl.PrepareParameter(
                    Device,
                    (driverContext, channels) =>
                    {
                        if (channels.Length == 0)
                            return;
                        modalLoading.Show("导入通道", $"正在导入[{channels.Length}]个通道...", true, null);
                        Task.Run(async () =>
                        {
                            try
                            {
                                await ChannelManager.Instance.AddChannels(Device, driverContext, channels);
                            }
                            catch (Exception ex)
                            {
                                toastStack.AddToast(ex.Message, $"原因：{ExceptionUtils.GetExceptionMessage(ex)}", BackgroundTheme.warning);
                            }
                            Device.UpdateChannelsCount();
                            modalWindow.Close();
                            modalLoading.Close();
                            search();
                            await InvokeAsync(StateHasChanged);
                            ChannelChanged?.Invoke();
                        });
                    })
                );
        }

        private void Edit(Model.Channel model)
        {
            var channel = ConfigDbContext.CacheContext.Find(new Model.Channel() { DeviceId = model.DeviceId, Id = model.Id });
            modalWindow.Show<ChannelCreateControl>("编辑通道",
                ChannelCreateControl.PrepareParameter(
                    Device,
                    channel,
                    t =>
                    {
                        modalLoading.Show("编辑通道", $"正在编辑通道[{t.Name}]...", true, null);
                        Task.Run(async () =>
                        {
                            try
                            {
                                await ChannelManager.Instance.EditChannel(driverContext, Device, t);
                                modalWindow.Close();
                            }
                            catch (Exception ex)
                            {
                                modalAlert.Show("错误", $"编辑通道[{t.Name}]时出错！原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                            }
                            modalLoading.Close();
                            search();
                            await InvokeAsync(StateHasChanged);
                            ChannelChanged?.Invoke();
                        });
                    })
                );
        }

        private void Delete(Model.Channel model)
        {
            var channel = ConfigDbContext.CacheContext.Find(new Model.Channel() { DeviceId = model.DeviceId, Id = model.Id });
            modalAlert.Show(
                "删除确认",
                $"确定要删除通道[{channel.Name}]?",
                () =>
                {
                    modalLoading.Show("删除通道", $"正在删除通道[{channel.Name}]...", true, null);
                    Task.Run(async () =>
                    {
                        try
                        {
                            await ChannelManager.Instance.DeleteChannel(driverContext, Device, channel);
                            modalAlert.Show("信息", $"删除通道[{channel.Name}]成功!");
                        }
                        catch (Exception ex)
                        {
                            modalAlert.Show("错误", $"删除通道[{channel.Name}]时出错！原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                        }
                        modalLoading.Close();
                        search();
                        await InvokeAsync(StateHasChanged);
                        ChannelChanged?.Invoke();
                    });
                },
            null);
        }
    }
}
