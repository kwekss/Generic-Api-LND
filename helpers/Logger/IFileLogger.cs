using System;
using System.Collections.Generic;
using System.Text;

namespace helper.Logger
{
    public interface IFileLogger
    {
        void LogError(string errorMessage);
        void LogError(Exception errorMessage);
        void LogInfo(string infoMessage);
        void LogInfo(StringBuilder infoMessage);
        void LogInfoBuilder(List<StringBuilder> infoMessage);
        void LogWarning(string warningMessage);
    }
}