namespace Rocket.Libraries.Qurious.Helpers
{
    internal static class DerivedTableWrapperNameResolver
    {
        public static string GetWrapperName(string derivedTableName)
        {
            return derivedTableName + "Wrapper";
        }
    }
}