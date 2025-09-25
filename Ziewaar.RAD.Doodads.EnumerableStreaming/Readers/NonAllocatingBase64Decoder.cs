namespace Ziewaar.RAD.Doodads.EnumerableStreaming.Readers
{
    public static class NonAllocatingBase64Decoder
    {
        private static readonly sbyte[] DecodingMap = CreateDecodingMap();
        public static bool TryDecode(char[] base64chars, int base64count, byte[] output, out int bytesWritten)
        {
            bytesWritten = 0;

            int len = base64count;
            if ((len & 3) != 0)
                return false; // base64 must be a multiple of 4

            int padding = 0;
            if (len >= 2)
            {
                if (base64chars[len - 1] == '=') padding++;
                if (base64chars[len - 2] == '=') padding++;
            }

            int requiredLen = ((len >> 2) * 3) - padding;
            if (output.Length < requiredLen)
                return false;

            int outputIndex = 0;
            for (int i = 0; i < len; i += 4)
            {
                int b0 = DecodeChar(base64chars[i]);
                int b1 = DecodeChar(base64chars[i + 1]);
                int b2 = DecodeChar(base64chars[i + 2]);
                int b3 = DecodeChar(base64chars[i + 3]);

                if ((b0 | b1 | b2 | b3) < 0)
                    return false; // invalid char

                output[outputIndex++] = (byte)((b0 << 2) | (b1 >> 4));
                if (base64chars[i + 2] != '=')
                    output[outputIndex++] = (byte)((b1 << 4) | (b2 >> 2));
                if (base64chars[i + 3] != '=')
                    output[outputIndex++] = (byte)((b2 << 6) | b3);
            }

            bytesWritten = outputIndex;
            return true;
        }

        private static int DecodeChar(char c)
        {
            if (c >= 0 && c < 128)
                return DecodingMap[c];
            return -1;
        }

        private static sbyte[] CreateDecodingMap()
        {
            var map = new sbyte[128];
            for (int i = 0; i < map.Length; i++) map[i] = -1;

            for (char c = 'A'; c <= 'Z'; c++) map[c] = (sbyte)(c - 'A');
            for (char c = 'a'; c <= 'z'; c++) map[c] = (sbyte)(26 + c - 'a');
            for (char c = '0'; c <= '9'; c++) map[c] = (sbyte)(52 + c - '0');
            map['+'] = 62;
            map['/'] = 63;
            map['='] = 0; // handled separately
            return map;
        }
    }
}

