using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Scoping;

namespace UmbracoDeploy.Contrib.Export
{
    internal static class ApplicationContextExtensions
    {
        public static IScopeProvider GetScopeProvider(this ApplicationContext applicationContext)
           => applicationContext.GetType().GetProperty("ScopeProvider", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(applicationContext) as IScopeProvider;
    }
}