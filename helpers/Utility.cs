using helpers.Exceptions;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace helpers
{
    public static class Utility
    {

        public static string COUNTRY_CODE = "233";
        public static string FormatNumber10Digits(string phoneNumber, bool returnIfInvalid = false)
        {
            phoneNumber = phoneNumber.Trim();
            if (phoneNumber == null || phoneNumber.Trim() == "" || !Regex.IsMatch(phoneNumber, "^\\d+$"))
            {
                if (returnIfInvalid)
                {
                    return phoneNumber;
                }
                throw new WarningException("Invalid Number: " + phoneNumber);
            }
            if (phoneNumber.Length == 9)
            {
                return "0" + phoneNumber;
            }
            if (phoneNumber.StartsWith("0") && phoneNumber.Length == 10)
            {
                return phoneNumber;
            }
            if (phoneNumber.StartsWith(COUNTRY_CODE) && phoneNumber.Length == 9 + COUNTRY_CODE.Length)
            {
                return "0" + phoneNumber.Substring(COUNTRY_CODE.Length);
            }
            if (phoneNumber.StartsWith($"{COUNTRY_CODE}{COUNTRY_CODE}") && phoneNumber.Length == 9 + $"{COUNTRY_CODE}{COUNTRY_CODE}".Length)
            {
                return "0" + phoneNumber.Substring($"{COUNTRY_CODE}{COUNTRY_CODE}".Length);
            }
            if (phoneNumber.StartsWith("+" + COUNTRY_CODE) && phoneNumber.Length == 10 + COUNTRY_CODE.Length)
            {
                return "0" + phoneNumber.Substring(COUNTRY_CODE.Length + 1);
            }
            if (phoneNumber.StartsWith("00" + COUNTRY_CODE) && phoneNumber.Length == 11 + COUNTRY_CODE.Length)
            {
                return "0" + phoneNumber.Substring(COUNTRY_CODE.Length + 2);
            }
            if (returnIfInvalid)
            {
                return phoneNumber;
            }
            throw new WarningException("Invalid Number: " + phoneNumber);
        }

        public static string FormatNumber9Digits(string phoneNumber)
        {
            phoneNumber = phoneNumber.Trim();
            if (phoneNumber == null || phoneNumber.Trim() == "" || !Regex.IsMatch(phoneNumber, "^\\d+$"))
            {
                throw new WarningException("Invalid Number: " + phoneNumber);
            }
            if (phoneNumber.StartsWith("00" + COUNTRY_CODE) && phoneNumber.Length == 10 + COUNTRY_CODE.Length)
            {
                phoneNumber = phoneNumber.Substring(COUNTRY_CODE.Length + 2);
            }
            else if (phoneNumber.StartsWith("+" + COUNTRY_CODE) && phoneNumber.Length == 9 + COUNTRY_CODE.Length)
            {
                phoneNumber = phoneNumber.Substring(COUNTRY_CODE.Length + 1);
            }
            else if (phoneNumber.StartsWith(COUNTRY_CODE) && phoneNumber.Length == 9 + COUNTRY_CODE.Length)
            {
                phoneNumber = phoneNumber.Substring(COUNTRY_CODE.Length);
            }
            else if (phoneNumber.StartsWith($"{COUNTRY_CODE}{COUNTRY_CODE}") && phoneNumber.Length == 9 + $"{COUNTRY_CODE}{COUNTRY_CODE}".Length)
            {
                phoneNumber = phoneNumber.Substring($"{COUNTRY_CODE}{COUNTRY_CODE}".Length);
            }
            else if (phoneNumber.StartsWith("0") && phoneNumber.Length == 10)
            {
                phoneNumber = phoneNumber.Substring(1);
            }
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new WarningException("Invalid Number: " + phoneNumber);
            }
            if (phoneNumber.Length != 9)
            {
                throw new WarningException("Invalid Number: " + phoneNumber);
            }
            return phoneNumber;
        }

        public static string FormatNumber12Digits(string phoneNumber)
        {
            phoneNumber = phoneNumber.Trim();
            if (phoneNumber == null || phoneNumber.Trim() == "" || !Regex.IsMatch(phoneNumber, "^\\d+$"))
            {
                throw new WarningException("Invalid Number: " + phoneNumber);
            }
            if (phoneNumber.StartsWith("+" + COUNTRY_CODE) && phoneNumber.Length == 10 + COUNTRY_CODE.Length)
            {
                phoneNumber = phoneNumber.Substring(1);
            }
            if (phoneNumber.StartsWith("00" + COUNTRY_CODE) && phoneNumber.Length == 11 + COUNTRY_CODE.Length)
            {
                phoneNumber = phoneNumber.Substring(2);
            }
            if (phoneNumber.StartsWith("0") && phoneNumber.Length == 10)
            {
                phoneNumber = COUNTRY_CODE + phoneNumber.Substring(1);
            }
            if (phoneNumber.Length == 9)
            {
                phoneNumber = COUNTRY_CODE + phoneNumber;
            }
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new WarningException("Invalid Number: " + phoneNumber);
            }
            if (phoneNumber.Length != 9 + COUNTRY_CODE.Length)
            {
                throw new WarningException("Invalid Number: " + phoneNumber);
            }
            return phoneNumber;
        }

        public static bool IsValidMsisdn(string phoneNumber)
        {
            phoneNumber = phoneNumber.Trim();
            if (phoneNumber == null || phoneNumber.Trim() == "" || !Regex.IsMatch(phoneNumber, "^\\d+$"))
            {
                throw new WarningException("Invalid Number: " + phoneNumber);
            }
            if (phoneNumber.StartsWith("+" + COUNTRY_CODE) && phoneNumber.Length == 10 + COUNTRY_CODE.Length)
            {
                phoneNumber = phoneNumber.Substring(1);
            }
            if (phoneNumber.StartsWith("00" + COUNTRY_CODE) && phoneNumber.Length == 11 + COUNTRY_CODE.Length)
            {
                phoneNumber = phoneNumber.Substring(2);
            }
            if (phoneNumber.StartsWith("0") && phoneNumber.Length == 10)
            {
                phoneNumber = COUNTRY_CODE + phoneNumber.Substring(1);
            }
            if (phoneNumber.Length == 9)
            {
                phoneNumber = COUNTRY_CODE + phoneNumber;
            }
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }
            if (phoneNumber.Length != 9 + COUNTRY_CODE.Length)
            {
                return false;
            }
            return true;
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

        public static string Stringify(this object obj, JsonSerializerSettings settings = null) => JsonConvert.SerializeObject(obj, settings);
        public static T ParseObject<T>(this string obj) => JsonConvert.DeserializeObject<T>(obj);
        public static T ParseXml<T>(this string objectData)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            object result;
            using (TextReader reader = new StringReader(objectData))
            {
                result = serializer.Deserialize(reader);
            }
            return (T)result;
        }
        public static bool IsEmpty<T>(List<T> list)
        {
            if (list == null) return true;

            return !list.Any();
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            try
            {
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                string DomainMapper(Match match)
                {
                    var idn = new IdnMapping();
                    string domainName = idn.GetAscii(match.Groups[2].Value);
                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static string GenerateRandom(int length)
        {
            Random generator = new Random();
            return generator.Next(0, 1000000).ToString($"D{length}");
        }

        public static string ExtractTagContent(this string responseString, string tagName)
        {
            string dataStartTag = "<" + tagName + ">";
            string dataEndTag = "</" + tagName + ">";
            int startIndex = responseString.IndexOf(dataStartTag, 0, StringComparison.CurrentCultureIgnoreCase);
            if (startIndex > 0)
            {
                int endIndex = responseString.IndexOf(dataEndTag, startIndex, StringComparison.CurrentCultureIgnoreCase);
                return responseString.Substring(startIndex + dataStartTag.Length, endIndex - dataStartTag.Length - startIndex).Trim();
            }
            return null;
        }

        public static int CheckPasswordStrength(string password)
        {
            //0 = blank, 1 = very weak, 2 weak, 3 medium, 4 strong >=5 Very strong
            int score = 0;

            if (password.Length < 1)
                return 0;
            if (password.Length < 4)
                return 1;

            if (password.Length >= 8)
                score++;
            if (password.Distinct().Count() >= 8)
                score++;
            if (password.Distinct().Count() >= 12)
                score++;
            if (password.Any(c => char.IsDigit(c)))
                score++;
            if (password.Any(c => char.IsUpper(c)) && password.Any(c => char.IsLower(c)))
                score++;
            if (password.IndexOfAny("!@#$%^&*?_~-£().,".ToCharArray()) >= 0)
                score++;

            return score;
        }

        public static double DiceCoefficient(string strA, string strB)
        {
            HashSet<string> setA = new HashSet<string>();
            HashSet<string> setB = new HashSet<string>();



            for (int i = 0; i < strA.Length - 1; ++i)
                setA.Add(strA.Substring(i, 2));


            for (int i = 0; i < strB.Length - 1; ++i)
                setB.Add(strB.Substring(i, 2));


            HashSet<string> intersection = new HashSet<string>(setA);
            intersection.IntersectWith(setB);


            return (2.0 * intersection.Count) / (setA.Count + setB.Count);
        }

        public static string EncryptSha256(string rawText)
        { 
            byte[] hash = new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(rawText));
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += string.Format("{0:x2}", x);
            }
            return hashString;
        }
        public static string ShuffleString(params string[] str)
        {
            var rnd = new Random(); 
            string unsuffled = string.Join("", str);
            return new string(unsuffled.OrderBy(r => rnd.Next()).ToArray()); 
        }

    }
}
