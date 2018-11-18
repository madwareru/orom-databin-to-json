using System;
using System.Collections.Generic;
using System.IO;

namespace OROMDataBinToJson
{
    public static class Make
    {
        public static KeyValuePair<TKey, TValue> Pair<TKey, TValue>(TKey key, TValue value) 
            => new KeyValuePair<TKey, TValue>(key, value);
    }
    
    public static class Utils
    {
        public static byte LookAhead(this Stream stream)
        {
            var byteAhead = (byte)stream.ReadByte();
            stream.Seek(-1, SeekOrigin.Current);
            return byteAhead;
        }
        
        public static bool FoundToken<T>(this IList<T> collection, 
            Func<T, string> memberLambda,
            string testString,
            out int resultId)
        {
            resultId = -1;
            for (var i = 0; i < collection.Count; ++i)
            {
                var member = memberLambda(collection[i]);
                if (testString.IndexOf(member, StringComparison.Ordinal) != -1)
                {
                    if (resultId == -1)
                    {
                        resultId = i;
                        continue;
                    }

                    if (memberLambda(collection[resultId]).Length >= member.Length)
                        continue;

                    resultId = i;
                }
            }
            return resultId != -1;
        }

        public static int IndexOf<T>(this IList<T> collection, Func<T, bool> predicate)
        {
            var id = -1;
            for (var i = 0; i < collection.Count; ++i)
            {
                if (predicate(collection[i]))
                {
                    id = i;
                    break;
                }
            }
            return id;
        }
    }
}