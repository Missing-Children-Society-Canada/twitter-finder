using System;

namespace MCSC
{
    public static class Utils
    {
        public static string GetEnvVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}