using helpers.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace helpers
{
    public static class Utility
    {
        public static string FormatNumber10Digits(string phoneNumber, bool returnIfInvalid = false)
        {
            phoneNumber = phoneNumber.Trim(); 
            if (phoneNumber == null ||
                phoneNumber.Trim() == "" ||
                !Regex.IsMatch(phoneNumber, @"^\d+$"))
            {
                if (returnIfInvalid)
                {
                    return phoneNumber;
                }
                else
                {
                    throw new WarningException($"Invalid Number: {phoneNumber}");
                }

            } 

            if (phoneNumber.Length == 9)
            {
                return $"0{phoneNumber}";
            }
            if (phoneNumber.StartsWith("0") && phoneNumber.Length == 10)
            {
                return phoneNumber;
            }
            if (phoneNumber.StartsWith("233") && phoneNumber.Length == 12)
            {
                return $"0{phoneNumber.Substring(3)}";
            }
            if (phoneNumber.StartsWith("+233") && phoneNumber.Length == 13)
            {
                return $"0{phoneNumber.Substring(4)}";
            }
            if (phoneNumber.StartsWith("00233") && phoneNumber.Length == 14)
            {
                return $"0{phoneNumber.Substring(5)}";
            }
            if (returnIfInvalid)
            {
                return phoneNumber;
            }
            else
            {
                throw new WarningException($"Invalid Number: {phoneNumber}");
            }

        }

        public static string FormatNumber9Digits(string phoneNumber)
        {
            phoneNumber = phoneNumber.Trim(); 
            if (phoneNumber == null ||
                phoneNumber.Trim() == "" ||
                !Regex.IsMatch(phoneNumber, @"^\d+$"))
            {
                throw new WarningException($"Invalid Number: {phoneNumber}");
            }

            //init variable to hold the formatted phone number
            if (phoneNumber.StartsWith("00233") && phoneNumber.Length == 14)
            {
                phoneNumber = phoneNumber.Substring(5);
            }
            else if (phoneNumber.StartsWith("+233") && phoneNumber.Length == 13)
            {
                phoneNumber = phoneNumber.Substring(4);
            }
            else if (phoneNumber.StartsWith("233") && phoneNumber.Length == 12)
            {
                phoneNumber = phoneNumber.Substring(3);
            }
            else if (phoneNumber.StartsWith("0") && phoneNumber.Length == 10)
            {
                phoneNumber = phoneNumber.Substring(1);
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new WarningException($"Invalid Number: {phoneNumber}");
            }
            if (phoneNumber.Length != 9)
            {
                throw new WarningException($"Invalid Number: {phoneNumber}");
            }
            return phoneNumber;
        }

        public static string FormatNumber12Digits(string phoneNumber)
        {
            phoneNumber = phoneNumber.Trim(); 
            if (phoneNumber == null ||
                phoneNumber.Trim() == "" ||
                !Regex.IsMatch(phoneNumber, @"^\d+$"))
            {
                throw new WarningException($"Invalid Number: {phoneNumber}");
            }
             

            if (phoneNumber.StartsWith("+233") && phoneNumber.Length == 13)
            {
                phoneNumber = phoneNumber.Substring(1);
            }
            if (phoneNumber.StartsWith("00233") && phoneNumber.Length == 14)
            {
                phoneNumber = phoneNumber.Substring(2);
            }
            if (phoneNumber.StartsWith("0") && phoneNumber.Length == 10)
            {
                phoneNumber = "233" + phoneNumber.Substring(1);
            }
            if (phoneNumber.Length == 9)
            {
                phoneNumber = "233" + phoneNumber;
            }
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new WarningException($"Invalid Number: {phoneNumber}");
            }
            if (phoneNumber.Length != 12)
            {
                throw new WarningException($"Invalid Number: {phoneNumber}");
            }
            return phoneNumber;
        }

        public static string Encrypt(string clearText, string EncryptionKey)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x76, 0x49, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(string cipherText, string EncryptionKey)
        {
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x76, 0x49, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public static string Stringify(this object obj, JsonSerializerSettings settings = null) => JsonConvert.SerializeObject(obj,settings);
        public static T ParseObject<T>(this string obj) => JsonConvert.DeserializeObject<T>(obj);

        public static bool IsEmpty<T>(List<T> list)
        {
            if (list == null)return true;

            return !list.Any();
        }


        public static bool IsValidMsisdn(string phoneNumber)
        {
            phoneNumber = phoneNumber.Trim();
            //check for valid phone number
            if (phoneNumber == null ||
                phoneNumber.Trim() == "" ||
                !Regex.IsMatch(phoneNumber, @"^\d+$"))
            {
                return false;
            } 

            //init variable to hold the formatted phone number

            if (phoneNumber.StartsWith("+233") && phoneNumber.Length == 13)
            {
                phoneNumber = phoneNumber.Substring(1);
            }
            if (phoneNumber.StartsWith("00233") && phoneNumber.Length == 14)
            {
                phoneNumber = phoneNumber.Substring(2);
            }
            if (phoneNumber.StartsWith("0") && phoneNumber.Length == 10)
            {
                phoneNumber = "233" + phoneNumber.Substring(1);
            }
            if (phoneNumber.Length == 9)
            {
                phoneNumber = "233" + phoneNumber;
            }
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }
            if (phoneNumber.Length != 12)
            {
                return false;
            }
            return true;
        }


    }
}
