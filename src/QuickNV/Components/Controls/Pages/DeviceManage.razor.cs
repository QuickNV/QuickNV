using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap;
using Quick.Blazor.Bootstrap.Admin.Utils;
using Quick.EntityFrameworkCore.Plus;
using Tewr.Blazor.FileReader;
using QuickNV.Core;
using QuickNV.Model;
using static Quick.Blazor.Bootstrap.Admin.Utils.FileUploadHelper;
using Quick.Utils;

namespace QuickNV.Components.Controls.Pages
{
    public partial class DeviceManage : IDisposable
    {
        private string searchIsOnline;
        private string searchKeywords;
        private ModalLoading modalLoading;
        private ModalAlert modalAlert;
        private ModalWindow modalWindow;
        private ToastStack toastStack;

        private ElementReference inputXlsxFile;
        [Inject]
        private IFileReaderService fileReaderService { get; set; }

        public Model.Device[] Devices { get; private set; }

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

        private bool? getSearchIsOnline()
        {
            if (string.IsNullOrEmpty(searchIsOnline))
                return null;
            return bool.Parse(searchIsOnline);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                search();
                Interfaces.Driver.Manager.Instance.DeviceOnline += Instance_DeviceOnlineOrOffline;
                Interfaces.Driver.Manager.Instance.DeviceOffline += Instance_DeviceOnlineOrOffline;
            }
        }

        private void Instance_DeviceOnlineOrOffline(object sender, Model.Device e)
        {
            Task.Run(() => search());
        }

        private void search()
        {
            IEnumerable<Model.Device> query = ConfigDbContext.CacheContext.Query<Model.Device>();
            var registerState = getSearchIsOnline();
            if (!string.IsNullOrEmpty(searchKeywords) || registerState.HasValue)
            {
                if (!string.IsNullOrEmpty(searchKeywords))
                    query = query.Where(t => t.Name.Contains(searchKeywords));
                if (registerState.HasValue)
                    query = query.Where(t => t.IsOnline == registerState.Value);
            }
            Devices = query.OrderBy(t => t.Id).ToArray();
            InvokeAsync(StateHasChanged);
        }

        private void DisplayChannels(Model.Device device)
        {
            modalWindow.Show<Controls.ChannelManage>($"通道 - {device.Name}",
                Controls.ChannelManage.PrepareParameter(device, () => search())
            );
        }

        private void DisplayLogs(Model.Device device)
        {
            modalWindow.Show<Controls.LogView>($"日志 - {device.Name}",
                Controls.LogView.PrepareParameter(device.LogContext)
            );
        }

        private void Create()
        {
            modalWindow.Show<Controls.DeviceCreateControl>("添加设备",
                Controls.DeviceCreateControl.PrepareParameter(
                    null,
                    t =>
                    {
                        modalLoading.Show("添加设备", $"正在添加设备[{t.Name}]...", true, null);
                        Task.Run(async () =>
                        {
                            try
                            {
                                await DeviceManager.Instance.AddDevice(t);
                                modalWindow.Close();
                            }
                            catch (Exception ex)
                            {
                                modalAlert.Show("错误", $"添加设备[{t.Name}]时出错！原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                            }
                            modalLoading.Close();
                            search();
                        });
                    })
                );
        }

        private void Import()
        {
            modalWindow.Show<Controls.DeviceImportControl>("导入设备",
                Controls.DeviceImportControl.PrepareParameter(
                    (driverContext, devices, channels) =>
                    {
                        if (devices.Length == 0)
                            return;
                        Task.Run(async () =>
                        {
                            modalLoading.Show("导入设备", $"正在导入[{devices.Length}]个设备...", true, null);
                            foreach (var device in devices)
                            {
                                try
                                {
                                    device.ChannelsCount = channels.Count(t => t.DeviceId == device.Id);
                                    await DeviceManager.Instance.AddDevice(device);
                                }
                                catch (Exception ex)
                                {
                                    toastStack.AddToast($"导入设备[{device.Name}]时出错", $"原因：{ExceptionUtils.GetExceptionMessage(ex)}", BackgroundTheme.warning);
                                }
                            }
                            foreach (var channel in channels)
                            {
                                try
                                {
                                    if (ConfigDbContext.CacheContext.Find(channel) != null)
                                        throw new ApplicationException($"设备[{channel.DeviceId}]中已经存在编号为[{channel.Id}]的通道");
                                    ConfigDbContext.CacheContext.Add(channel);
                                    await driverContext.OnAddChannel(channel);
                                }
                                catch (Exception ex)
                                {
                                    toastStack.AddToast($"导入通道[{channel.Name}]时出错", $"原因：{ExceptionUtils.GetExceptionMessage(ex)}", BackgroundTheme.warning);
                                }
                            }
                            modalWindow.Close();
                            modalLoading.Close();
                            search();
                        });
                    })
                );
        }

        private async Task Enable(Model.Device model)
        {
            await DeviceManager.Instance.Enable(model);
        }

        private void Disable(Model.Device device)
        {
            modalAlert.Show(
            "禁用确认",
                $"确定要禁用设备[{device.Name}]?",
                new ModalAlertOptions()
                {
                    OkCallback = () =>
                    {
                        modalLoading.Show("禁用设备", $"正在禁用设备[{device.Name}]...", true, null);
                        Task.Run(async () =>
                        {
                            try
                            {
                                await DeviceManager.Instance.Disable(device);
                                await InvokeAsync(StateHasChanged);
                            }
                            catch (Exception ex)
                            {
                                modalAlert.Show("错误", $"禁用设备[{device.Name}]时出错！原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                            }
                            modalLoading.Close();
                        });
                    }
                });
        }

        private void Edit(Model.Device model)
        {
            var device = ConfigDbContext.CacheContext.Find(new Model.Device() { Id = model.Id });
            if (device == null)
            {
                modalAlert.Show("错误", $"{device}未找到，请刷新页面。");
                return;
            }
            modalWindow.Show<Controls.DeviceCreateControl>("编辑设备",
                Controls.DeviceCreateControl.PrepareParameter(
                    device,
                    t =>
                    {
                        modalLoading.Show("编辑设备", $"正在编辑设备[{t.Name}]...", true, null);
                        Task.Run(async () =>
                        {
                            try
                            {
                                await DeviceManager.Instance.EditDevice(t);
                                modalWindow.Close();
                            }
                            catch (Exception ex)
                            {
                                modalAlert.Show("错误", $"编辑设备[{t.Name}]时出错！原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                            }
                            modalLoading.Close();
                            search();
                        });
                    })
                );
        }

        private void Delete(Model.Device model)
        {
            var device = ConfigDbContext.CacheContext.Find(new Model.Device() { Id = model.Id });
            modalAlert.Show(
                "删除确认",
                $"确定要删除设备[{device.Name}]?",
                new ModalAlertOptions()
                {
                    OkCallback = () =>
                    {
                        modalLoading.Show("删除设备", $"正在删除设备[{device.Name}]...", true, null);
                        Task.Run(async () =>
                        {
                            try
                            {
                                await DeviceManager.Instance.DeleteDevice(device.Id);
                                modalAlert.Show("信息", $"删除设备[{device.Name}]成功!");
                            }
                            catch (Exception ex)
                            {
                                modalAlert.Show("错误", $"删除设备[{device.Name}]时出错！原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                            }
                            modalLoading.Close();
                            search();
                        });
                    }
                });
        }

        private CancellationTokenSource uploadCts;
        private async Task onInputXlsxFileChanged()
        {
            var fileReaderRef = fileReaderService.CreateReference(inputXlsxFile);

            uploadCts = new CancellationTokenSource();
            UploadFileInfo uploadingFileInfo = default;
            string uploadingFileInfoStr = null;
            string uploadingFile = null;
            try
            {
                modalLoading.Show($"从Excel文件导入", "正在获取上传文件信息...", false, uploadCts.Cancel);
                var fileReference = (await fileReaderRef.EnumerateFilesAsync()).FirstOrDefault();
                uploadingFile = await FileUploadHelper.UploadFileAsync(fileReference,
                    fileInfo =>
                    {
                        uploadingFileInfo = fileInfo;
                        uploadingFileInfoStr = $"{fileInfo.Name} ({fileInfo.SizeString})";
                        modalLoading.Show($"从Excel文件导入", $"正在上传镜像文件[{uploadingFileInfoStr}]...", false, uploadCts.Cancel);
                        return null;
                    },
                    progressInfo => modalLoading.UpdateProgress(progressInfo.Percent, progressInfo.Message),
                    uploadCts.Token);

                modalLoading.UpdateProgress(null, null);
                modalLoading.Show($"从Excel文件导入", $"正在加载Excel文件[{uploadingFileInfoStr}]...", true, uploadCts.Cancel);

                var dbContextBackupContext = new DbContextBackup.Excel.XlsxDbContextBackupContext(_ => Model.ModelsJsonSerializerContext.Default2);
                var newModels = new List<Model.Channel>();

                using (var dbContext = new ConfigDbContext())
                    dbContextBackupContext.Check(dbContext, uploadingFile, model =>
                    {
                        if (model is Model.Channel channel)
                            newModels.Add(channel);
                    });

                modalLoading.Show($"从Excel文件导入", $"正在对比数据...", true, uploadCts.Cancel);
                var existModels = ConfigDbContext.CacheContext.Query<Model.Channel>();                
                var channelModelChecker = new ModelChecker<Model.Channel>(false,
                    new ModelChecker<Channel>.PropertyInfo(t => t.DeviceId, (t, v) => t.DeviceId = (string)v),
                    new ModelChecker<Channel>.PropertyInfo(t => t.Id, (t, v) => t.Id = (string)v),
                    new ModelChecker<Channel>.PropertyInfo(t => t.Name, (t, v) => t.Name = (string)v),
                    new ModelChecker<Channel>.PropertyInfo(t => t.DriverConfig, (t, v) => t.DriverConfig = (string)v),
                    new ModelChecker<Channel>.PropertyInfo(t => t.Lng, (t, v) => t.Lng = (double?)v),
                    new ModelChecker<Channel>.PropertyInfo(t => t.Lat, (t, v) => t.Lat = (double?)v),
                    new ModelChecker<Channel>.PropertyInfo(t => t.ExternalId, (t, v) => t.ExternalId = (string)v),
                    new ModelChecker<Channel>.PropertyInfo(t => t.AddressId, (t, v) => t.AddressId = (string)v)
                );
                channelModelChecker.CheckModels(existModels, newModels.ToArray(), out var addList, out var updateList, out var deleteList);
                if (addList.Count > 0)
                {
                    modalLoading.Show($"从Excel文件导入", $"正在添加数据...", true, uploadCts.Cancel);
                    for (var i = 0; i < addList.Count; i++)
                    {
                        await Task.Delay(0,uploadCts.Token);
                        var item = addList[i];
                        modalLoading.UpdateProgress(i * 100 / addList.Count, $"[{i + 1}/{addList.Count}] 正在添加{item}...");
                        await ChannelManager.Instance.AddChannel(item);
                    }
                }
                if (updateList.Count > 0)
                {
                    for (var i = 0; i < updateList.Count; i++)
                    {
                        await Task.Delay(0,uploadCts.Token);
                        var item = updateList[i];
                        modalLoading.UpdateProgress(i * 100 / updateList.Count, $"[{i + 1}/{updateList.Count}] 正在更新{item}...");
                        await ChannelManager.Instance.EditChannel(item);
                    }
                }
                modalAlert?.Show("从Excel文件导入", $"导入完成，增加[{addList.Count}]条数据，更新[{updateList.Count}]条数据。");
                search();
            }
            catch (OperationCanceledException)
            {
                modalAlert?.Show("导入已取消", $"已取消从Excel文件[{uploadingFileInfoStr}]导入.");
            }
            catch (ApplicationException ex)
            {
                modalAlert?.Show("导入失败", ExceptionUtils.GetExceptionMessage(ex));
            }
            catch (Exception ex)
            {
                modalAlert?.Show("导入失败", ExceptionUtils.GetExceptionString(ex));
            }
            finally
            {
                if (uploadingFile != null && File.Exists(uploadingFile))
                    try { File.Delete(uploadingFile); } catch { }
                modalLoading?.Close();
            }
        }

        public void Dispose()
        {
            Interfaces.Driver.Manager.Instance.DeviceOnline -= Instance_DeviceOnlineOrOffline;
            Interfaces.Driver.Manager.Instance.DeviceOffline -= Instance_DeviceOnlineOrOffline;
        }
    }
}
