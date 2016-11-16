﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Text;
using System.IO;
using Xunit;

namespace System.Text.Formatting.Tests
{
    struct Age : IBufferFormattable
    {
        int _age;
        bool _inMonths;

        public Age(int age, bool inMonths = false)
        {
            _age = age;
            _inMonths = inMonths;
        }

        public bool TryFormat(Span<byte> buffer, out int bytesWritten, TextFormat format, EncodingData encoding)
        {
            if (!PrimitiveFormatter.TryFormat(_age, buffer, out bytesWritten, format, encoding)) return false;


            char symbol = _inMonths ? 'm' : 'y';
            int symbolBytes;
            if (!PrimitiveFormatter.TryEncode(symbol, buffer.Slice(bytesWritten), out symbolBytes, encoding.TextEncoding)) return false;

            bytesWritten += symbolBytes;
            return true;
        }

        public override string ToString()
        {
            return _age.ToString() + (_inMonths ? "m" : "y");
        }
    }

    public class CustomTypeFormatting
    {
        static ArrayPool<byte> pool = ArrayPool<byte>.Shared;

        [Fact]
        public void CustomTypeToStreamUtf16()
        {
            byte[] buffer = new byte[1024];
            MemoryStream stream = new MemoryStream(buffer);
            using(var writer = new StreamFormatter(stream, pool)) {
                writer.Append(new Age(56));
                writer.Append(new Age(14, inMonths: true));

                var writtenText = Encoding.Unicode.GetString(buffer, 0, (int)stream.Position);
                Assert.Equal(writtenText, "56y14m");
            }
        }

        [Fact]
        public void CustomTypeToStreamUtf8()
        {
            byte[] buffer = new byte[1024];
            MemoryStream stream = new MemoryStream(buffer);
            using(var writer = new StreamFormatter(stream, EncodingData.InvariantUtf8, pool)) {
                writer.Append(new Age(56));
                writer.Append(new Age(14, inMonths: true));
                var writtenText = Encoding.UTF8.GetString(buffer, 0, (int)stream.Position);
                Assert.Equal(writtenText, "56y14m");
            }
        }

        [Fact]
        public void CustomTypeToString()
        {
            var sb = new StringFormatter();
            sb.Append(new Age(56));
            sb.Append(new Age(14, inMonths: true));
            Assert.Equal(sb.ToString(), "56y14m");
        }
    }
}
