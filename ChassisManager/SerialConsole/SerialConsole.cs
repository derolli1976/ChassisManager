
namespace Microsoft.GFS.WCS.ChassisManager
{       
    public class SerialStatusPacket
    {
        public CompletionCode completionCode;
    }

    public class SerialDataPacket
    {
        public CompletionCode completionCode;
        public byte[] data;
    }

}
