namespace QuickNV.Protocol.Driver.QpModels
{
    /// <summary>
    /// 云台控制命令
    /// </summary>
    [Serializable]    
    public enum PTZCommandType : int
    {
        /// <summary>
        /// 停止
        /// </summary>
        Stop = 0,
        /// <summary>
        /// 上
        /// </summary>
        Up = 1,
        /// <summary>
        /// 下
        /// </summary>
        Down = 4,
        /// <summary>
        /// 左
        /// </summary>
        Left = 7,
        /// <summary>
        /// 右
        /// </summary>
        Right = 8,
        /// <summary>
        /// 焦点后调
        /// </summary>
        FocusFar = 9,
        /// <summary>
        /// 焦点前调
        /// </summary>
        FocusNear = 10,
        /// <summary>
        /// 焦距变大
        /// </summary>
        ZoomIn = 11,
        /// <summary>
        /// 焦距变小
        /// </summary>
        ZoomOut = 12,
        /// <summary>
        /// 光圈扩大
        /// </summary>
        IrisOpen = 13,
        /// <summary>
        /// 光圈缩小
        /// </summary>
        IrisClose = 14
    }
}
