using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace helpers
{
    public static class Utility
    {
        public static string FormatNumber10Digits(string phoneNumber, bool returnIfInvalid = false)
        {
            phoneNumber = phoneNumber.Trim();
            //check for valid phone number
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
                    throw new InvalidOperationException("Invalid Number");
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
                throw new InvalidOperationException("Invalid Number");
            }

        }

        public static string FormatNumber9Digits(string phoneNumber)
        {
            phoneNumber = phoneNumber.Trim();
            //check for valid phone number
            if (phoneNumber == null ||
                phoneNumber.Trim() == "" ||
                !Regex.IsMatch(phoneNumber, @"^\d+$"))
            {
                throw new InvalidOperationException("Invalid Number");
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
                throw new InvalidOperationException("Invalid Number");
            }
            if (phoneNumber.Length != 9)
            {
                throw new InvalidOperationException("Invalid Number");
            }
            return phoneNumber;
        }

        public static string FormatNumber12Digits(string phoneNumber)
        {
            phoneNumber = phoneNumber.Trim();
            //check for valid phone number
            if (phoneNumber == null ||
                phoneNumber.Trim() == "" ||
                !Regex.IsMatch(phoneNumber, @"^\d+$"))
            {
                throw new InvalidOperationException("Invalid Number");
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
                throw new InvalidOperationException("Invalid Number");
            }
            if (phoneNumber.Length != 12)
            {
                throw new InvalidOperationException("Invalid Number");
            }
            return phoneNumber;
        }

        public static string Stringify(this object obj) => JsonConvert.SerializeObject(obj);
        public static T ParseObject<T>(this string obj) => JsonConvert.DeserializeObject<T>(obj);
    }
}
