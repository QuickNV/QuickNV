using Microsoft.EntityFrameworkCore;
using Quick.EntityFrameworkCore.Plus;
using Quick.Protocol;
using System.Reflection;
using Tewr.Blazor.FileReader;
using YiQiDong.Agent;
using YiQiDong.Core;
using QuickNV.Core;
using QuickNV.Model;
using QuickNV.Utils;
using QuickNV.Components;
using Quick.Utils;

namespace QuickNV;

public class Agent : AbstractAgent
{
    public static Agent Instance { get; private set; }

    public ConfigModel Config { get; private set; }
    public int WebServerPort { get; private set; }


    private CancellationTokenSource cts;
    private WebApplication app;

    public Agent()
    {
        Instance = this;
    }

    private Functions.Config configFunction;
    public override void Init()
    {
        Quick.Protocol.Pipeline.QpPipelineClientOptions.RegisterUriSchema();
        Quick.Protocol.Tcp.QpTcpClientOptions.RegisterUriSchema();
        Quick.Protocol.WebSocket.Client.QpWebSocketClientOptions.RegisterUriSchema();

        base.Init();
        //使用的数据库类型：SQLite、MySQL、达梦
        DbUtils.Init(
#if DEBUG
            Quick.EntityFrameworkCore.Plus.SQLite.SQLiteDbContextConfigHandler.Info,
#endif
            Quick.EntityFrameworkCore.Plus.MySql.MySqlDbContextConfigHandler.Info,
            Quick.EntityFrameworkCore.Plus.Dm.DmDbContextConfigHandler.Info
            );
        ConfigDbContext.ModelBuilderHandler = OnModelCreating;
        configFunction = new Functions.Config();
        AddFunction(configFunction);
    }

    protected virtual void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Config>();
        modelBuilder.Entity<Address>();
        modelBuilder.Entity<MediaServer>();
        modelBuilder.Entity<Device>();
        modelBuilder.Entity<Channel>().HasKey(t => new { t.DeviceId, t.Id });
    }

    public override void Start()
    {
        //加载配置文件
        Config = configFunction.ReadConfig();
        Components.Controls.Pages.PlayerConfigManage.Init(null);

        try
        {
            //初始化数据库连接
            ConfigDbContext.ConfigHandler = DbUtils.GetDbContextConfigHandler(Config.AppDb.DbType, t => ModelsJsonSerializerContext.Default2, Config.AppDb.DbConnectionParameter);
            AgentContext.LogInfo("确保数据库创建和更新...");
            ConfigDbContext.ConfigHandler.DatabaseEnsureCreatedAndUpdated(() => new ConfigDbContext());
            ConfigDbContext.CacheContext.LoadCache();
            AgentContext.LogInfo("数据库连接初始化完成.");
        }
        catch (Exception ex)
        {
            AgentContext.LogWarn($"初始化数据库连接时出错，原因：{ExceptionUtils.GetExceptionMessage(ex)}");
            throw new IOException($"初始化数据库连接时出错，原因：{ExceptionUtils.GetExceptionMessage(ex)}");
        }

        //ViewGrid管理器初始化
        Components.Controls.ViewGrids.ViewGridManager.Instance.Init();
        //ApiKey管理器初始化
        ApiKeyManager.Instance.Init();
        //启动Web服务
        cts = new CancellationTokenSource();
        var token = cts.Token;
        Task.Run(() =>
        {
            #if DEBUG
            var contentRootPath = Path.Combine(AppContext.BaseDirectory, "../../");
            var defaultWebRootPath = Path.Combine(contentRootPath, "../QiYun.UI/dist");
#else
            var contentRootPath = AgentContext.Container.ImageFolder;
            var defaultWebRootPath = Path.Combine(contentRootPath, "wwwroot");
#endif
            var webApplicationOptions = new WebApplicationOptions()
            {
                ContentRootPath = contentRootPath,
                WebRootPath = defaultWebRootPath
            };
            var builder = WebApplication.CreateBuilder(webApplicationOptions);
#if DEBUG
            builder.WebHost.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
#endif
            builder.Logging.ClearProviders();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = Config.SessionCookieName;
            });
            Core.Web.ReverseProxyManager.Instance.Load(builder.Services.AddReverseProxy());
            builder.Services.AddFileReaderService();
            builder.Services.AddControllers();
            builder.Services.AddRazorComponents()
                    .AddInteractiveServerComponents()
                    .AddHubOptions(options =>
                    {
                        options.EnableDetailedErrors = true;
                        //设置最大包大小为100 MB
                        options.MaximumReceiveMessageSize = 100 * 1024 * 1024;
                    });
#if DEBUG
            builder.Services.AddSwaggerGen(c =>
            {
                var docId = this.GetType().Assembly.GetName().Name;
                c.SwaggerDoc(docId, new Microsoft.OpenApi.OpenApiInfo
                {
                    Version = docId,
                    Title = "QuickNV",
                    Description = this.GetType().Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description
                });
                c.CustomSchemaIds(x => x.FullName);
                //添加当前程序集的XML文档
                var xmlFile = $"{this.GetType().Assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.TagActionsBy(t =>
                {
                    var tagName = t.ActionDescriptor.AttributeRouteInfo.Name
                        ?? t.ActionDescriptor.DisplayName;
                    if (t.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)
                    {
                        var actionDescriptor = (Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)t.ActionDescriptor;
                        var attr = actionDescriptor.ControllerTypeInfo.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
                        if (attr == null)
                            tagName = actionDescriptor.ControllerName;
                        else
                            tagName = attr.DisplayName;
                    }
                    return new[] { tagName };
                });
            });
#endif

#if DEBUG
            builder.WebHost.UseUrls(new[] { "http://localhost:8097" });
#else
            builder.WebHost.UseUrls(Config.WebUrls.Split(new char[] { ',', ';' }));
#endif

            app = builder.Build();
#if DEBUG
            app.UseDeveloperExceptionPage();
#else
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "text/plain;charset=UTF-8";
                    var exceptionHandlerPathFeature =
                        context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                    await context.Response.WriteAsync(ExceptionUtils.GetExceptionMessage(exceptionHandlerPathFeature?.Error));
                });
            });
#endif
            app.UseWebSockets();
            Interfaces.Driver.Manager.Instance.Init(app, Config);
            Interfaces.North.Manager.Instance.Init(app, Config);
            app.MapStaticAssets();
            app.MapReverseProxy();
#if DEBUG
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"{this.GetType().Assembly.GetName().Name}/swagger.json", "QuickNV");
            });
#endif
            app.UseSession();
            app.UseMiddleware<Core.Web.LoginMiddleware>();
            app.UseRouting();
            app.UseAntiforgery();
            app.MapControllers();
            app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

            while (true)
            {
                if (token.IsCancellationRequested)
                    return;
                //启动Web服务
                try
                {
                    //启动易认证管理器
                    YiRenZhengManager.Instance.Start();

                    app.Start();
                    AgentContext.LogInfo($"Web服务启动完成，地址：{string.Join(",", app.Urls)}");

                    WebServerPort = new Uri(app.Urls.First()).Port;
                    //启动媒体服务管理器
                    MediaServerManager.Instance.Start();
                    //启动驱动管理器
                    DriverManager.Instance.Start();
                    //启动驱动接口
                    Interfaces.Driver.Manager.Instance.Start();
                    //启动北向接口
                    Interfaces.North.Manager.Instance.Start();
                    break;
                }
                catch (Exception ex)
                {
                    var message = $"启动Web服务时失败，原因：" + ex.Message;
                    AgentContext.LogError(message);
                    if (!AgentContext.IsContainerRuning)
                        throw new Exception(message, ex);
                    Thread.Sleep(5000);
                }
            }
        });
    }


    public override void Stop()
    {
        cts?.Cancel();
        cts = null;

        //停止北向接口
        Interfaces.North.Manager.Instance.Stop();
        //停止驱动接口
        Interfaces.Driver.Manager.Instance.Stop();
        //停止驱动管理器
        DriverManager.Instance.Stop();
        //停止媒体服务管理器
        MediaServerManager.Instance.Stop();

        app?.StopAsync();
        app = null;

        //停止易认证管理器
        YiRenZhengManager.Instance.Stop();
    }
}
