using System.IO;

namespace OROMDataBinToJson
{
    public static class Utils
    {
        public static byte LookAhead(this Stream stream)
        {
            var byteAhead = (byte)stream.ReadByte();
            stream.Seek(-1, SeekOrigin.Current);
            return byteAhead;
        }
    }
}