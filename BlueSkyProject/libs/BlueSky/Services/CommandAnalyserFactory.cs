using System;
using BSky.Interfaces.Commands;
using BSky.Interfaces.Model;

namespace BlueSky.Services
{
    public static class CommandAnalyserFactory
    {
        public static ICommandAnalyser GetClientAnalyser(AnalyticsData data)
        {
            string typename = "BSky.Commands.DyVIBlendAnalyser,BSky.Commands";
            Type typeObj = Type.GetType(typename);
            object obj = Activator.CreateInstance(typeObj);
            return obj as ICommandAnalyser;
        }

    }
}
