using QuickNV.Protocol.Driver.QpModels;

namespace QuickNV.Driver.GB28181.Command.DeviceControl
{
    public class PTZCommandUtils
    {
        /// <summary>
        /// 拼接ptz控制指令
        /// </summary>
        /// <param name="ucommand">控制命令</param>
        /// <param name="dwSpeed">速度</param>
        /// <returns></returns>
        public static string GetPtzCmd(PTZCommandType ucommand, byte dwSpeed)
        {
            //指令字节数组
            List<byte> cmdList = new List<byte>(8)
            {
                0xA5,
                0x0F,
                0x01
            };
            switch (ucommand)
            {
                case PTZCommandType.Stop:
                    cmdList.Add(00);
                    cmdList.Add(00);
                    cmdList.Add(00);
                    cmdList.Add(00);
                    break;
                case PTZCommandType.Up:
                    cmdList.Add(0x08);
                    cmdList.Add(dwSpeed);
                    cmdList.Add(dwSpeed);
                    cmdList.Add(00);
                    break;
                case PTZCommandType.Down:
                    cmdList.Add(0x04);
                    cmdList.Add(dwSpeed);
                    cmdList.Add(dwSpeed);
                    cmdList.Add(00);
                    break;
                case PTZCommandType.Left:
                    cmdList.Add(0x02);
                    cmdList.Add(dwSpeed);
                    cmdList.Add(dwSpeed);
                    cmdList.Add(00);
                    break;
                case PTZCommandType.Right:
                    cmdList.Add(0x01);
                    cmdList.Add(dwSpeed);
                    cmdList.Add(dwSpeed);
                    cmdList.Add(00);
                    break;
                case PTZCommandType.ZoomIn: //镜头放大
                    cmdList.Add(0x10);
                    cmdList.Add(00);
                    cmdList.Add(00);
                    cmdList.Add(dwSpeed);
                    break;
                case PTZCommandType.ZoomOut: //镜头缩小
                    cmdList.Add(0x20);
                    cmdList.Add(00);
                    cmdList.Add(00);
                    cmdList.Add(dwSpeed);
                    break;
                case PTZCommandType.FocusFar: //聚焦+
                    cmdList.Add(0x42);
                    cmdList.Add(dwSpeed);
                    cmdList.Add(00);
                    cmdList.Add(00);
                    break;
                case PTZCommandType.FocusNear: //聚焦—
                    cmdList.Add(0x41);
                    cmdList.Add(dwSpeed);
                    cmdList.Add(00);
                    cmdList.Add(00);
                    break;
                case PTZCommandType.IrisOpen: //光圈open
                    cmdList.Add(0x44);
                    cmdList.Add(00);
                    cmdList.Add(dwSpeed);
                    cmdList.Add(00);
                    break;
                case PTZCommandType.IrisClose: //光圈close
                    cmdList.Add(0x48);
                    cmdList.Add(00);
                    cmdList.Add(dwSpeed);
                    cmdList.Add(00);
                    break;
                default:
                    break;
            }

            int checkByte = 0;
            foreach (var cmdItem in cmdList)
            {
                checkByte += cmdItem;
            }

            checkByte = checkByte % 256;
            cmdList.Add(Convert.ToByte(checkByte));

            string cmdStr = string.Empty;
            foreach (var cmdItemStr in cmdList)
            {
                cmdStr += cmdItemStr.ToString("X").PadLeft(2, '0'); //10进制转换为16进制
            }

            return cmdStr;
        }
    }
}
