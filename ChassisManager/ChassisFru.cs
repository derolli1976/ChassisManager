// Copyright © Microsoft Open Technologies, Inc.
// All Rights Reserved
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0 

// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR
// CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT. 
// See the Apache 2 License for the specific language governing permissions and limitations under the License. 

namespace Microsoft.GFS.WCS.ChassisManager
{

    using System;

    /// <summary>
    /// Class for CM Fru Read and Write
    /// </summary>
    public class ChassisFru : ChassisSendReceive
    {
        byte _completionCode;
        public Ipmi.FruCommonHeader CommonHeader { get; internal set; }
        public Ipmi.FruChassisInfo ChassisInfo { get; protected set; }
        public Ipmi.FruBoardInfo BoardInfo { get; protected set; }
        public Ipmi.FruProductInfo ProductInfo { get; protected set; }

        /// <summary>
        /// Constructor 
        /// </summary>
        public ChassisFru()
        {
        }
        /// <summary>
        /// Constructor for ChassisFru
        /// </summary>
        /// <param name="deviceId"></param>
        public ChassisFru(byte completionCode)
        {
            this._completionCode = completionCode;
        }

        /// <summary>
        /// Complete constructor for CM Fru
        /// </summary>
        /// <param name="commonHeader"></param>
        /// <param name="chassisInfoBytes"></param>
        /// <param name="boardInfoBytes"></param>
        /// <param name="productInfoBytes"></param>
        /// <param name="completionCode"></param>
        public void PopulateChassisFru(Ipmi.FruCommonHeader commonHeader,
                            byte[] chassisInfoBytes,
                            byte[] boardInfoBytes,
                            byte[] productInfoBytes, byte completionCode)
        {
            this.CommonHeader = commonHeader;
            this._completionCode = completionCode;
            if (chassisInfoBytes != null)
            {
                this.ChassisInfo = new Ipmi.FruChassisInfo(chassisInfoBytes);
            }

            if (boardInfoBytes != null)
            {
                this.BoardInfo = new Ipmi.FruBoardInfo(boardInfoBytes);
            }

            if (productInfoBytes != null)
            {
                this.ProductInfo = new Ipmi.FruProductInfo(productInfoBytes);
            }

        }

        /// <summary>
        /// Low and High Byte commands
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte getLowByte(ushort value)
        {
            byte lowByte = (byte)(value & 0xff);
            return lowByte;
        }

        public static byte getHighByte(ushort value)
        {
            byte highByte = (byte)((value >> 8) & 0xff);
            return highByte;
        }

        /// <summary>
        /// Read function
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte readChassisFru()
        {
            // Constants - TODO - move to config file
            ushort offset = (ushort) ConfigLoaded.ChassisStartingOffset;
            ushort length = (ushort) ConfigLoaded.ChassisFruLength;
            ushort internalUseSize = (ushort) ConfigLoaded.InternalUseSize; // bytes
            ushort chassisInfoSize = (ushort) ConfigLoaded.ChassisInfoSize; // bytes
            ushort boardInfoSize = (ushort) ConfigLoaded.BoardInfoSize; // bytes
            ushort productInfoSize = (ushort) ConfigLoaded.ProductInfoSize; // bytes
            
            try
            {

                ChassisFruReadResponse fruResponse = readRawChassisFru(offset, length);

                if (fruResponse.CompletionCode != 0 || fruResponse.DataReturned == null)
                {
                    Tracer.WriteInfo("Error in CM Fru Read {0}", fruResponse.CompletionCode);
                    this._completionCode = (byte)CompletionCode.UnspecifiedError;
                    return this._completionCode;
                }

                Ipmi.FruCommonHeader commonHeader = new Ipmi.FruCommonHeader(fruResponse.DataReturned);
                ushort areaOffset;

                byte[] chassisInfoBytes = new byte[chassisInfoSize];
                byte[] boardInfoBytes = new byte[boardInfoSize];
                byte[] productInfoBytes = new byte[productInfoSize];

                areaOffset = commonHeader.ChassisInfoStartingOffset;
                if (areaOffset != 0)
                {
                    Array.Copy(fruResponse.DataReturned, areaOffset, chassisInfoBytes, 0, chassisInfoSize);
                }

                areaOffset = commonHeader.BoardAreaStartingOffset;
                if (areaOffset != 0)
                {
                    Array.Copy(fruResponse.DataReturned, areaOffset, boardInfoBytes, 0, boardInfoSize);
                }

                areaOffset = commonHeader.ProductAreaStartingOffset;
                if (areaOffset != 0)
                {
                    Array.Copy(fruResponse.DataReturned, areaOffset, productInfoBytes, 0, productInfoSize);
                }

                 this.PopulateChassisFru(commonHeader,
                                        chassisInfoBytes,
                                        boardInfoBytes,
                                        productInfoBytes, fruResponse.CompletionCode);
            }
            catch (Exception e)
            {
                Tracer.WriteError("CM Fru Read had exception {0}", e);
                this._completionCode = (byte) CompletionCode.UnspecifiedError;
            }
            return this._completionCode;
        }
   
        /// <summary>
        /// Reads raw CM information - can be called individually if needed
        /// User level priority since this is not an internal call
        /// </summary>
        public ChassisFruReadResponse readRawChassisFru(ushort offset, ushort length)
        {
            byte deviceId = 1;
            ChassisFruReadResponse response = new ChassisFruReadResponse();

            try
            {
                response = (ChassisFruReadResponse)this.SendReceive(DeviceType.ChassisFruEeprom, deviceId, new ChassisFruReadRequest(offset, length),
                    typeof(ChassisFruReadResponse), (byte)PriorityLevel.User);
            }
            catch (Exception ex)
            {
                Tracer.WriteWarning("Chassis Fru read had an exception", ex);
            }

            if (response.CompletionCode != (byte)CompletionCode.Success)
            {
                Tracer.WriteError("Fru read failed with completion code {0:X}", response.CompletionCode);
            }
            else
            {
                Tracer.WriteInfo("Chassis Fru info read: {0:X}", response.DataReturned.ToString());
            }
            return response;
        }


        /// <summary>
        /// Write to Chassis Fru - (Important) note that this function enables write to any offset 
        /// Offset checks ensures that we are within permissible limits, but cannot enforce semantics within those limits
        /// Length checks validity of packet, however empty fields in packet are responsibility of writing function
        /// User level priority since this is not an internal call
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        public byte writeChassisFru(ushort offset, ushort length, byte[] packet)
        {
            byte deviceId = 1;

            ChassisFruWriteResponse response = new ChassisFruWriteResponse();

            try
            {
                response = (ChassisFruWriteResponse)this.SendReceive(DeviceType.ChassisFruEeprom, deviceId, new ChassisFruWriteRequest(offset, length, packet),
                 typeof(ChassisFruWriteResponse), (byte)PriorityLevel.User);
            }
            catch (Exception ex)
            {
                Tracer.WriteWarning("ChassisFru write had an exception", ex);
            }

            if (response.CompletionCode != (byte)CompletionCode.Success)
            {
                Tracer.WriteError("Fru Write failed with completion code {0:X}", response.CompletionCode);
            }
            else
            {
                Tracer.WriteInfo("Fru Write succeeded");
            }
         
            return (byte)response.CompletionCode;
        }

        /// <summary>
        /// Request Format for Chassis Fru read
        /// </summary>
        [ChassisMessageRequest(FunctionCode.ReadEeprom)]
        internal class ChassisFruReadRequest : ChassisRequest
        {
            [ChassisMessageData(0)]
            public byte lowOffset
            {
                get;
                set;
            }

            [ChassisMessageData(1)]
            public byte highOffset
            {
                get;
                set;
            }

            [ChassisMessageData(2)]
            public byte lowLength
            {
                get;
                set;
            }

            [ChassisMessageData(3)]
            public byte highLength
            {
                get;
                set;
            }

            public ChassisFruReadRequest(ushort offset, ushort length)
            {
                Tracer.WriteInfo("Offset: {0}, Length: {1} inside ChassisFru class", offset, length);
                this.lowOffset = getLowByte(offset);
                this.highOffset = getHighByte(offset);
                this.lowLength = getLowByte(length);
                this.highLength = getHighByte(length);
            }
        }

        
        /// <summary>
        /// Response for CM Fru Read
        /// </summary>
        [ChassisMessageResponse(FunctionCode.ReadEeprom)]
        public class ChassisFruReadResponse : ChassisResponse
        {
            // TODO - fix this
            private byte[] dataReturned = new byte[256];

            [ChassisMessageData(0)]
            public byte[] DataReturned
            {
                get { return this.dataReturned; }
                set { this.dataReturned = value; }
            }
        }

        [ChassisMessageRequest(FunctionCode.WriteEeprom)]
        internal class ChassisFruWriteRequest : ChassisRequest
        {

            [ChassisMessageData(0)]
            public byte lowOffset
            {
                get;
                set;
            }

            [ChassisMessageData(1)]
            public byte highOffset
            {
                get;
                set;
            }

            [ChassisMessageData(2)]
            public byte lowLength
            {
                get;
                set;
            }

            [ChassisMessageData(3)]
            public byte highLength
            {
                get;
                set;
            }

            [ChassisMessageData(4)]
            public byte[] dataToWrite
            {
                get;
                set;
            }


            public ChassisFruWriteRequest(ushort offset, ushort length, byte[] dataToWrite)
            {
                this.lowOffset = getLowByte(offset);
                this.highOffset = getHighByte(offset);
                this.lowLength = getLowByte(length);
                this.highLength = getHighByte(length);
                this.dataToWrite = dataToWrite;
            }
        }

        [ChassisMessageResponse(FunctionCode.WriteEeprom)]
        internal class ChassisFruWriteResponse : ChassisResponse
        {
        }
    }
}
