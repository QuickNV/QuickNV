namespace QuickNV.Core
{
    public class ApiKeyManager
    {
        public const string API_KEY = "ApiKey";
        private string apiKey;

        public static ApiKeyManager Instance { get; } = new ApiKeyManager();
        private Dictionary<string, DateTime> tempApiKeyDict = new Dictionary<string, DateTime>();
        private CancellationTokenSource cts;
        public void Init()
        {
            apiKey = ConfigManager.Instance.GetConfig(API_KEY);
            cts?.Cancel();
            cts = new CancellationTokenSource();
            beginCheckTempApiKey(cts.Token);
        }

        private void beginCheckTempApiKey(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            Task.Delay(TimeSpan.FromMinutes(1), token).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;
                var nowTime = DateTime.Now;
                lock (tempApiKeyDict)
                    foreach (var item in tempApiKeyDict.ToArray())
                        if (item.Value > DateTime.Now)
                            tempApiKeyDict.Remove(item.Key);
                beginCheckTempApiKey(token);
            });
        }

        public string GetApiKey()
        {
            return apiKey;
        }

        public void SetApiKey(string value)
        {
            apiKey = value;
            ConfigManager.Instance.SetConfig(API_KEY, value);
        }

        /// <summary>
        /// 验证ApiKey
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ValidateApiKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            if (key == apiKey)
                return true;
            lock (tempApiKeyDict)
                if (tempApiKeyDict.ContainsKey(key))
                {
                    tempApiKeyDict.Remove(key);
                    return true;
                }
            return false;
        }

        /// <summary>
        /// 获取临时ApiKey
        /// </summary>
        /// <returns></returns>
        public string GetTempApiKey()
        {
            return GetTempApiKey(TimeSpan.FromMinutes(1));
        }
        /// <summary>
        /// 获取临时ApiKey
        /// </summary>
        /// <returns></returns>
        public string GetTempApiKey(TimeSpan timeSpan)
        {
            lock (tempApiKeyDict)
            {
                var key = Guid.NewGuid().ToString("N");
                tempApiKeyDict[key] = DateTime.Now.Add(timeSpan);
                return key;
            }
        }
    }
}
