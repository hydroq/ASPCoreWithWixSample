//
// JwtUtilities.cs
//
// Copyright (c) 2018 RE'FLEKT GmbH. All Rights Reserved.
// Copyright Bosch Automotive Service Solutions Limited 2018 all rights reserved.
//
// Authors:
//     Axel Peter
//
// Defines:
//     [C] One.Core.Utilities.JwtUtilities
//

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace REFLEKT.ONEAuthor.Application.Utilities
{
    /// <summary>
    /// Static utility methods for JWT (Json Web Token) format.
    /// </summary>
    public static class JwtUtilities
    {
        /// <summary>
        /// The JWT seperator used to split the three parts.
        /// </summary>
        private const char JwtSeparator = '.';

        /// <summary>
        /// Returns the decoded payload for the given JWT string.
        /// </summary>
        /// <param name="token">The JWT token with all three parts.</param>
        /// <returns>The decoded payload.</returns>
        public static string GetPayload(string token)
        {
            var tokenParts = token.Split(JwtSeparator);
            var payload = tokenParts[1];
            return ConvertBase64ToUTF8(payload);
        }

        /// <summary>
        /// Verifies the signature for given JWT token parts.
        /// </summary>
        /// <param name="token">The input token in jwt format. Needs to contain three parts separated by JwtSeparator.</param>
		/// <param name="publicKey">The public key to verify the JWT with.</param>
		/// <param name="hashAlgorithm">The hash algorithm the JWT is encoded with.</param>
        /// <returns><c>true</c> if valid signature, otherwise <c>false</c>.</returns>
        public static bool VerifySignature(string token, string publicKey, string hashAlgorithm)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var tokenParts = token.Split(JwtSeparator);

            if (tokenParts.Length != 3)
            {
                return false;
            }

            var rsaProvider = DecodeX509PublicKey(Convert.FromBase64String(publicKey));
            var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(tokenParts[0] + JwtSeparator + tokenParts[1]));
            var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsaProvider);
            rsaDeformatter.SetHashAlgorithm(hashAlgorithm);
            return rsaDeformatter.VerifySignature(hash, ConvertBase64ToByte(tokenParts[2]));
        }

        /// <summary>
        /// Converts input from base64 to utf string.
        /// </summary>
        /// <param name="data">The data to convert.</param>
        /// <returns>The converted string.</returns>
        private static string ConvertBase64ToUTF8(string data)
        {
            data = data.Length % 4 == 0
                ? data : data + "====".Substring(data.Length % 4);

            byte[] buffer = Convert.FromBase64String(data);
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Converts base 64 url string to byte array.
        /// </summary>
        /// <param name="base64Url">String in base 64.</param>
        /// <returns>Converted byte array.</returns>
        private static byte[] ConvertBase64ToByte(string base64Url)
        {
            string padded = base64Url.Length % 4 == 0
                ? base64Url : base64Url + "====".Substring(base64Url.Length % 4);
            string base64 = padded.Replace("_", "/")
                                  .Replace("-", "+");
            return Convert.FromBase64String(base64);
        }

        /// <summary>
        /// Compares two byte arrays.
        /// </summary>
        /// <param name="arrayA">First byte array.</param>
        /// <param name="arrayB">Second byte array.</param>
        /// <returns><c>true</c> if both byte arrays are the same, otherwise <c>false</c>.</returns>
        private static bool CompareByteArrays(byte[] arrayA, byte[] arrayB)
        {
            if (arrayA.Length != arrayB.Length)
            {
                return false;
            }

            int i = 0;
            foreach (var byteA in arrayA)
            {
                if (byteA != arrayB[i])
                {
                    return false;
                }
                i++;
            }

            return true;
        }

        /// <summary>
        /// Decodes a X509 public key and generates a RSACryptoServiceProvider for it.
        /// <seealso cref="https://stackoverflow.com/questions/11506891/how-to-load-the-rsa-public-key-from-file-in-c-sharp>"/>
        /// </summary>
        /// <param name="x509key">The public key in bytes.</param>
        /// <returns>The generated RSACryptoServiceProvider using the public keys modulus and exponent.</returns>
        private static RSACryptoServiceProvider DecodeX509PublicKey(byte[] x509key)
        {
            // encoded OID sequence for PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] sequenceOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] sequence = new byte[15];

            // Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob
            var memoryStream = new MemoryStream(x509key);

            // wrap Memory Stream with BinaryReader for easy reading
            var binaryReader = new BinaryReader(memoryStream);
            byte oneByte = 0;
            ushort twoBytes = 0;

            try
            {
                twoBytes = binaryReader.ReadUInt16();
                // data read as little endian order (actual data order for Sequence is 30 81)
                if (twoBytes == 0x8130)
                    binaryReader.ReadByte();
                else if (twoBytes == 0x8230)
                    binaryReader.ReadInt16();
                else
                    return null;

                // read the Sequence OID
                sequence = binaryReader.ReadBytes(15);
                // make sure Sequence for OID is correct
                if (!CompareByteArrays(sequence, sequenceOID))
                    return null;

                twoBytes = binaryReader.ReadUInt16();
                //data read as little endian order (actual data order for Bit String is 03 81)
                if (twoBytes == 0x8103)
                    binaryReader.ReadByte();
                else if (twoBytes == 0x8203)
                    binaryReader.ReadInt16();
                else
                    return null;

                oneByte = binaryReader.ReadByte();
                //expect null byte next
                if (oneByte != 0x00)
                    return null;

                twoBytes = binaryReader.ReadUInt16();
                //data read as little endian order (actual data order for Sequence is 30 81)
                if (twoBytes == 0x8130)
                    binaryReader.ReadByte();
                else if (twoBytes == 0x8230)
                    binaryReader.ReadInt16();
                else
                    return null;

                twoBytes = binaryReader.ReadUInt16();
                byte lowByte = 0x00;
                byte highByte = 0x00;

                //data read as little endian order (actual data order for Integer is 02 81)
                if (twoBytes == 0x8102)
                {
                    lowByte = binaryReader.ReadByte();
                }
                else if (twoBytes == 0x8202)
                {
                    highByte = binaryReader.ReadByte();
                    lowByte = binaryReader.ReadByte();
                }
                else
                {
                    return null;
                }

                //reverse byte order since asn.1 key uses big endian order
                byte[] modInt = { lowByte, highByte, 0x00, 0x00 };
                int modSize = BitConverter.ToInt32(modInt, 0);

                byte firstByte = binaryReader.ReadByte();
                binaryReader.BaseStream.Seek(-1, SeekOrigin.Current);

                if (firstByte == 0x00)
                {   // if first byte (highest order) of modulus is zero, don't include it
                    binaryReader.ReadByte();
                    // reduce modulus buffer size by 1
                    modSize -= 1;
                }

                // read the modulus bytes
                byte[] modulus = binaryReader.ReadBytes(modSize);

                // expect an Integer for the exponent data
                if (binaryReader.ReadByte() != 0x02)
                    return null;

                // should only need one byte for actual exponent data (for all useful values)
                int exponentBytes = (int)binaryReader.ReadByte();
                byte[] exponent = binaryReader.ReadBytes(exponentBytes);

                // create RSACryptoServiceProvider instance and initialize with public key
                var rsaServiceProvider = new RSACryptoServiceProvider();
                var rsaParameters = new RSAParameters();
                rsaParameters.Modulus = modulus;
                rsaParameters.Exponent = exponent;
                rsaServiceProvider.ImportParameters(rsaParameters);
                return rsaServiceProvider;
            }
            catch (Exception e)
            {
                //Debug.LogWarning("Could not decode the public key: " + e);
                return null;
            }
            finally
            {
                binaryReader.Close();
            }
        }
    }
}