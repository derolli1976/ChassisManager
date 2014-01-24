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
    using System.Diagnostics;
    using System.Reflection;

    /// <summary>
    /// message encapsulation class.  serialize request/response message into bytes.
    /// </summary>
    public abstract class ChassisMessage
    {
        /// <summary>
        /// ChassisMessageAttribute instance attached to this ChassisMessage instance.
        /// </summary>
        private readonly ChassisMessageAttribute _chassisMessageAttribute;

        private byte _completionCode;
        
        public byte CompletionCode
        {
            get { return this._completionCode; }
            internal set { this._completionCode = value; }
        }

        /// <summary>
        /// Gets the Command within the scope of the message function.
        /// </summary>
        public virtual FunctionCode Command
        {
            get { return this._chassisMessageAttribute.Command; }
        }

        protected ChassisMessage()
        {
            ChassisMessageAttribute[] attributes =
                (ChassisMessageAttribute[])this.GetType().GetCustomAttributes(typeof(ChassisMessageAttribute), true);
            if (attributes.Length != 1)
            {
                throw new InvalidOperationException();
            }

            this._chassisMessageAttribute = attributes[0];
        }

        /// <summary>
        /// Set the Response object properteis.
        /// </summary>
        private void SetProperties(byte[] message)
        {
            foreach (PropertyInfo propertyInfo in this.GetType().GetProperties())
            {
                ChassisMessageDataAttribute[] attributes =
                    (ChassisMessageDataAttribute[])propertyInfo.GetCustomAttributes(typeof(ChassisMessageDataAttribute), true);

                if (attributes.Length > 0)
                {
                    if (propertyInfo.PropertyType == typeof(Byte))
                    {
                        if (attributes[0].Offset < message.Length)
                        {
                            propertyInfo.SetValue(this, message[attributes[0].Offset], null);
                        }
                    }
                    else if (propertyInfo.PropertyType == typeof(UInt16))
                    {
                        propertyInfo.SetValue(this, BitConverter.ToUInt16(message, attributes[0].Offset), null);
                    }
                    else if (propertyInfo.PropertyType == typeof(UInt32))
                    {
                        propertyInfo.SetValue(this, BitConverter.ToUInt32(message, attributes[0].Offset), null);
                    }
                    else if (propertyInfo.PropertyType == typeof(Byte[]))
                    {
                        Int32 propertyLength = attributes[0].Length;

                        if (propertyLength == 0)
                        {
                            propertyLength = message.Length - attributes[0].Offset;
                        }

                        if (attributes[0].Offset < message.Length)
                        {
                            byte[] propertyData = new byte[propertyLength];
                            Buffer.BlockCopy(message, attributes[0].Offset, propertyData, 0, propertyData.Length);

                            propertyInfo.SetValue(this, propertyData, null);
                        }
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
            }
        }

        /// <summary>
        /// Processes received omc message
        /// Expected Packet Format:
        ///        4            5-6        N    
        /// |Completion Code|Byte Count|Payload|
        ///       1 byte       2 byte
        /// </summary>
        internal void Initialize(byte[] message, int length)
        {
            // completion code
            _completionCode = message[0];

            // payload lenght
            ushort payloadLength = BitConverter.ToUInt16(message, 1);

            // payload offset ( 0 = completion code, 1:2 = byte count)
            int payloadOffset = 3;

            // get the message data
            byte[] payload = new byte[payloadLength];

            Buffer.BlockCopy(message, payloadOffset, payload, 0, payloadLength);

            // set response properties
            if (_completionCode == 0)
            {
                SetProperties(payload);
            }
        }

        private int GetDataLenght()
        {
            int lenght = 0;

            foreach (PropertyInfo propertyInfo in this.GetType().GetProperties())
            {
                ChassisMessageDataAttribute[] attributes2 =
                    (ChassisMessageDataAttribute[])propertyInfo.GetCustomAttributes(typeof(ChassisMessageDataAttribute), true);

                if (attributes2.Length > 0)
                {
                    if (propertyInfo.PropertyType == typeof(byte))
                    {
                        lenght += 1;
                    }
                    else if (propertyInfo.PropertyType == typeof(ushort))
                    {
                        lenght += 2;
                    }
                    else if (propertyInfo.PropertyType == typeof(short))
                    {
                        lenght += 2;
                    }
                    else if (propertyInfo.PropertyType == typeof(int))
                    {
                        lenght += 4;
                    }
                    else if (propertyInfo.PropertyType == typeof(uint))
                    {
                        lenght += 4;
                    }
                    else if (propertyInfo.PropertyType == typeof(byte[]))
                    {
                        byte[] bytes = (byte[])propertyInfo.GetValue(this, null);
                        lenght += bytes.Length;
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
            }

            return lenght;
        }

        /// <summary>
        /// Get Request Message Properteis
        /// </summary>
        private byte[] GetProperties(int lenght)
        { 
            byte[] data = new byte[lenght];

            if (lenght > 0)
            {
                foreach (PropertyInfo propertyInfo in this.GetType().GetProperties())
                {
                    ChassisMessageDataAttribute[] attributes =
                        (ChassisMessageDataAttribute[])propertyInfo.GetCustomAttributes(typeof(ChassisMessageDataAttribute), true);

                    if (attributes.Length > 0)
                    {
                        if (propertyInfo.PropertyType == typeof(byte))
                        {
                            data[attributes[0].Offset] = (byte)propertyInfo.GetValue(this, new Object[0]);
                        }
                        else if (propertyInfo.PropertyType == typeof(ushort))
                        {
                            byte[] raw = BitConverter.GetBytes((ushort)propertyInfo.GetValue(this, new Object[0]));
                            Buffer.BlockCopy(raw, 0, data, attributes[0].Offset, 2);
                        }
                        else if (propertyInfo.PropertyType == typeof(short))
                        {
                            byte[] raw = BitConverter.GetBytes((short)propertyInfo.GetValue(this, new Object[0]));
                            Buffer.BlockCopy(raw, 0, data, attributes[0].Offset, 2);
                        }
                        else if (propertyInfo.PropertyType == typeof(uint))
                        {
                            byte[] raw = BitConverter.GetBytes((uint)propertyInfo.GetValue(this, new Object[0]));
                            Buffer.BlockCopy(raw, 0, data, attributes[0].Offset, raw.Length);
                        }
                        else if (propertyInfo.PropertyType == typeof(int))
                        {
                            byte[] raw = BitConverter.GetBytes((int)propertyInfo.GetValue(this, new Object[0]));
                            Buffer.BlockCopy(raw, 0, data, attributes[0].Offset, raw.Length);
                        }
                        else if (propertyInfo.PropertyType == typeof(byte[]))
                        {
                            byte[] raw = (byte[])propertyInfo.GetValue(this, new Object[0]);
                            Buffer.BlockCopy(raw, 0, data, attributes[0].Offset, raw.Length);
                        }
                        else
                        {
                            Debug.Assert(false);
                        }
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Convert the Chassis Manager request meeting into a byte stream for transmission to the serial layer.
        /// Expected Packet Format:
        ///      1           5-6       N     
        ///|Function Code|Byte Count|Payload|
        ///   1 byte       2 byte    N bytes
        /// </summary>
        public byte[] GetBytes()
        {
            // get message data lenght
            int payloadLength = GetDataLenght();

            // Message lenght
            // + 1 byte for command byte
            // + 2 bytes for byte count
            // + N bytes for payload
            int messageLength = 3 + payloadLength;
            
            // byte array for return message
            byte[] message = new byte[messageLength];

            message[0] = (byte)this.Command;

            // change message byte ordering to satisy device layer code expectation of byte ordering
            message[2] = (byte)(payloadLength >> 8); // second byte 
            message[1] = (byte)payloadLength; // first byte;

            // Get message data
            byte[] data = GetProperties(payloadLength);

            if (payloadLength > 0)
            {                       
                // dataIndex + 3 for function and count offsets into message
                Buffer.BlockCopy(data, 0, message, 3, data.Length);
            }

           return message;
        }

    }
}
