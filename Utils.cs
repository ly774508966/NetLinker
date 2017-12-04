using System;

namespace Meow.NetLinker
{
    public static class Utils
    {
        private static readonly DateTime UtcTimeBegin = new DateTime(1970, 1, 1);

        public static uint GetClockMs()
        {
            return (uint)(Convert.ToInt64(DateTime.UtcNow.Subtract(UtcTimeBegin).TotalMilliseconds) & 0xffffffff);
        }
    }
}