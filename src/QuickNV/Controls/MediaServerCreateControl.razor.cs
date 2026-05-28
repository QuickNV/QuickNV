using Microsoft.AspNetCore.Components;
using NPOI.SS.Formula.Functions;
using Quick.Blazor.Bootstrap;

namespace QuickNV.Controls
{
    public partial class MediaServerCreateControl
    {
        private Model.MediaServer createModel = new Model.MediaServer();
        [Parameter]
        public Model.MediaServer Model { get; set; }
        [Parameter]
        public Action<Model.MediaServer> OkAction { get; set; }

        private void Ok()
        {
            OkAction?.Invoke(createModel);
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            if (Model != null)
            {
                createModel.Id = Model.Id;
                createModel.Name = Model.Name;
                createModel.ApiUrl = Model.ApiUrl;
                createModel.ApiSecret = Model.ApiSecret;
                createModel.PublicIpAddress = Model.PublicIpAddress;
                createModel.PublicUrl = Model.PublicUrl;
            }
        }

        public static DialogParameters<MediaServerCreateControl> PrepareParameter(Model.MediaServer model, Action<Model.MediaServer> okAction)
        {
            return new DialogParameters<MediaServerCreateControl>()
            {
                {t=>t.Model,model},
                {t=>t.OkAction,okAction},
            };
        }
    }
}
