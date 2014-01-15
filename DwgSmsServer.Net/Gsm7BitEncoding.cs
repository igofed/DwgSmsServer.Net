using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DwgSmsServerNet
{
    /// <summary>
    /// GSM 7bit encoder/decoder
    /// Idea of encoding was taken here: http://stackoverflow.com/questions/17218874/convert-string-to-gsm-7-bit-using-c-sharp
    /// Idea of decoding was taken here: http://stackoverflow.com/questions/13130935/decode-7-bit-gsm
    /// </summary>
    class Gsm7BitEncoding : Encoding
    {
        public override int GetByteCount(char[] chars, int index, int count)
        {
            var newChars = new char[count];
            Array.Copy(chars, index, newChars, 0, count);

            var bytes = GetGsm7BitBytes(newChars);

            return bytes.Length;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            var newChars = new char[charCount];
            Array.Copy(chars, charIndex, newChars, 0, charCount);

            var encodedBytes = GetGsm7BitBytes(newChars);
            Array.Copy(encodedBytes, 0, bytes, byteIndex, encodedBytes.Length);

            return encodedBytes.Length;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            var newBytes = new byte[count];
            Array.Copy(bytes, index, newBytes, 0, count);

            var chars = GetGsm7BitChars(newBytes);

            return chars.Length;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            var newBytes = new byte[byteCount];
            Array.Copy(bytes, byteIndex, newBytes, 0, byteCount);

            var decodedChars = GetGsm7BitChars(newBytes);
            Array.Copy(decodedChars, 0, chars, charIndex, decodedChars.Length);

            return decodedChars.Length;
        }

        public override int GetMaxByteCount(int charCount)
        {
            //max 2 bytes per one char
            //if all chars from ExtensionSet
            return charCount * 2;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            //min 1 char per one byte
            return byteCount;
        }


        // Basic Character Set
        private const string BasicSet =
                "@£$¥èéùìòÇ\nØø\rÅåΔ_ΦΓΛΩΠΨΣΘΞ\x1bÆæßÉ !\"#¤%&'()*+,-./0123456789:;<=>?¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ`¿abcdefghijklmnopqrstuvwxyzäöñüà";

        // Basic Character Set Extension 
        private const string ExtensionSet =
                "````````````````````^```````````````````{}`````\\````````````[~]`|````````````````````````````````````€``````````````````````````";

        // If the character is in the extension set, it must be preceded
        // with an 'ESC' character whose index is '27' in the Basic Character Set
        private const int EscIndex = 27;

        private byte[] GetGsm7BitBytes(char[] chars)
        {
            // Use this list to store the index of the character in 
            // the basic/extension character sets
            var indicies = new List<byte>();

            foreach (var c in chars)
            {
                var index = BasicSet.IndexOf(c);
                if (index != -1)
                {
                    indicies.Add((byte)index);
                    continue;
                }

                index = ExtensionSet.IndexOf(c);
                if (index != -1)
                {
                    // Add the 'ESC' character index before adding 
                    // the extension character index
                    indicies.Add(EscIndex);
                    indicies.Add((byte)index);
                    continue;
                }

                throw new NotSupportedException("Input is not a GSM 7bit string");
            }

            return indicies.ToArray();
        }

        private char[] GetGsm7BitChars(byte[] bytes)
        {
            var chars = new List<char>();

            var enumerator = bytes.AsEnumerable().GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current != EscIndex)
                {
                    if (enumerator.Current >= BasicSet.Length)
                        throw new NotSupportedException("Input is not a GSM 7bit string");
                    chars.Add(BasicSet[enumerator.Current]);
                }
                else
                {
                    if (enumerator.MoveNext())
                    {
                        if (enumerator.Current >= ExtensionSet.Length)
                            throw new NotSupportedException("Input is not a GSM 7bit string");
                        chars.Add(ExtensionSet[enumerator.Current]);
                    }
                }
            }

            return chars.ToArray();
        }
    } 
}
