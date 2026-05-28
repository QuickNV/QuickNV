using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using QuickNV.Core;

namespace QuickNV.Pages
{
    public partial class Login
    {
        [Inject]
        private NavigationManager NavigationManager { get; set; }

        public bool IsLogin { get; private set; } = false;
        public string Title => "QuickNV";
        public string Message { get; private set; }

        [BindProperty]
        public string Password { get; set; }
        private string CorrectPassword;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            CorrectPassword = Agent.Instance.Config.WebPassword;
#if DEBUG
            Password = CorrectPassword;
#endif
            //ApiKey登录
            var queryObj = HttpUtility.ParseQueryString(new Uri(NavigationManager.Uri).Query);
            var apiKey = queryObj.GetValues(ApiKeyManager.API_KEY)?.FirstOrDefault();
            if (ApiKeyManager.Instance.ValidateApiKey(apiKey))
                IsLogin = true;
        }

        public void OnPost()
        {
            if (!IsLogin && (string.IsNullOrEmpty(CorrectPassword) || CorrectPassword != Password))
            {
                Message = "密码不正确！";
                return;
            }
            //密码正确
            NavigationManager.NavigateTo("./api/login/login?ApiKey=" + ApiKeyManager.Instance.GetTempApiKey(), true);
        }

        private void onPasswordKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
                OnPost();
        }
    }
}
