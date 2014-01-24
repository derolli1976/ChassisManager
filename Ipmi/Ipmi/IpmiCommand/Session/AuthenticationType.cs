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

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
   
    /// <summary>
    /// Ipmi Authentication algorithms
    /// </summary>
    public enum CipherSuite
    {
        /// <summary>
        /// Authentication = None, Integrity = None, Confidentiality = None
        /// </summary>
        None = 0,

        /// <summary>
        /// Authentication = HMAC-SHA1, Integrity = None, Confidentiality = None
        /// </summary>        
        Sha1NoneNone = 1,

        /// <summary>
        /// Authentication = HMAC-SHA1, Integrity = HMAC-SHA1-96, Confidentiality = None
        /// </summary>
        Sha1Sha1None = 2,

        /// <summary>
        /// Authentication = HMAC-SHA1, Integrity = HMAC-SHA1-96, Confidentiality = AES
        /// </summary>
        Sha1Sha1Aes = 3,

        /// <summary>
        /// Authentication = HMAC-MD5, Integrity = None, Confidentiality = None
        /// </summary>
        MD5NoneNone = 6,

        /// <summary>
        /// Authentication = HMAC-MD5, Integrity = HMAC-MD5, Confidentiality = None
        /// </summary>
        MD5MD5None = 7,

        /// <summary>
        /// Authentication = HMAC-MD5, Integrity = HMAC-MD5, Confidentiality = AES
        /// </summary>
        MD5MD5Aes = 8,

        /// <summary>
        /// Authentication = HMAC-MD5, Integrity = MD5-128, Confidentiality = None
        /// </summary>
        MD5MD5128 = 11,

        /// <summary>
        /// Authentication = None, Integrity = MD5-128, Confidentiality = AES
        /// </summary>
        MD5MD5128Aes = 12
    }

    /// <summary>
    /// ipmi v1.5 authentication algorithms
    /// </summary>
    public enum AuthenticationType
    {
        None = 0,

        MD2 = 1,

        MD5 = 2,

        Straight = 4
    }

    /// <summary>
    /// ipmi v2.0 authentication algorithms 
    /// </summary>
    public enum RmcpAuthentication : byte
    {
        None = 0x00,

        HMACSHA1 = 0x01,

        HMACMD5 = 0x02,

        HMACSHA256 = 0x03
    }

    /// <summary>
    /// ipmi v2.0 integrity algorithms 
    /// </summary>
    public enum RmcpIntegrity : byte
    {
        None = 0x00,

        HMACSHA196 = 0x01,

        HMACMD5128 = 0x02,

        MD5128 = 0x03,

        HMACSHA256128 = 0x04

    }

    /// <summary>
    /// ipmi v2.0 confidentiality algorithms 
    /// </summary>
    public enum RmcpConfidentiality : byte
    {
        None = 0x00,

        AESCBC128 = 0x01
    }
}
