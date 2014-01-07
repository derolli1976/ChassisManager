namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    using System;

    /// <summary>
    /// Represents the IPMI 'Get Guid' application response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Application, IpmiCommand.GetSystemGuid)]
    internal class GetSystemGuidResponse : IpmiResponse
    {
        /// <summary>
        /// Device Guid.
        /// </summary>
        private byte[] guid = new byte[16];

        /// <summary>
        /// Gets Device Guid.
        /// </summary>
        [IpmiMessageData(0)]
        public byte[] Guid
        {
            get { return this.guid; }
            set {
                if (value != null)
                {
                    if (value.Length == 16)
                    {
                        Buffer.BlockCopy(value, 0, this.guid, 0, 16);
                    }
                }
            }
        }
    }
}
