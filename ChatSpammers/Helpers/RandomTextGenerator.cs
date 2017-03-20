using System;

namespace Helpers
{
    public static class RandomTextGenerator
    {
        public const string charsArr = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        static Random rnd = new Random();
        public static string Generate(int length)
        {

            string res = @"";
            for (int i = 0; i < length; i++)
                res += charsArr[rnd.Next(0, charsArr.Length - 1)];
            return res;

        }
    }
}
