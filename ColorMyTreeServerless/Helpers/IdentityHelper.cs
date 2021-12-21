using System;

namespace ColorMyTree.Helpers
{
    public static class IdentityHelper
    {
        private static readonly Random Random = new();

        public static string GenerateId()
        {
            var buffer = new byte[8];
            Random.NextBytes(buffer);

            return BitConverter.ToString(buffer).ToLowerInvariant().Replace("-", "");
        }
    }
}
