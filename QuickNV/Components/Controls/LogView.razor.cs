using Microsoft.AspNetCore.Components;
using NPOI.SS.Formula.Functions;
using Quick.Blazor.Bootstrap;

namespace QuickNV.Components.Controls
{
    public partial class LogView : IDisposable
    {
        private LogViewControl control;

        private int LogRows = 25;

        [Parameter]
        public Core.WithLogContext Context { get; set; }

        public void AddLog(string line)
        {
            control.AddLine(line);
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            Context.NewLogPushed += Context_NewLogPushed;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (firstRender)
            {
                foreach (var line in Context.GetLogLines())
                    control?.AddLine(line);
            }
        }

        private void Context_NewLogPushed(object sender, string e)
        {
            if (control == null)
                return;
            control.AddLine(e);
        }

        public static DialogParameters<LogView> PrepareParameter(Core.WithLogContext context)
        {
            return new DialogParameters<LogView>()
            {
                {t=>t.Context,context}
            };
        }

        public void Dispose()
        {
            Context.NewLogPushed -= Context_NewLogPushed;
        }
    }
}
