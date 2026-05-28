using QuickNV.Core;

namespace QuickNV.Controls.ViewGrids
{
    public interface IViewGrid
    {
        /// <summary>
        /// 编号
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// 单元格数量
        /// </summary>
        public int CellCount { get; }
        /// <summary>
        /// 选中的单元格
        /// </summary>
        public ViewGridCellInfo SelectedCell { get; }
        /// <summary>
        /// 选中的单元格改变时
        /// </summary>
        public event EventHandler<ViewGridCellInfo> SelectedCellChanged;
        /// <summary>
        /// 闪烁
        /// </summary>
        /// <param name="index">序号</param>
        void Flash(int index);

        /// <summary>
        /// 获取单元格信息
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ViewGridCellInfo GetCellInfo(int index);
        /// <summary>
        /// 根据指定的通道获取单元格
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public ViewGridCellInfo GetCellInfoByChannel(Model.Channel channel);
    }
}
