﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    class ResponseBodyStream : IDisposable
    {
        private readonly BinaryReader _stream;

        public ResponseBodyStream(Stream stream)
        {
            _stream = new BinaryReader(stream);
            OperationResult = (OperationResult)ReadShort();
        }
        public short ReadShort()
        {
            return (short)(ReadInt() & 255);
        }

        public int ReadInt()
        {
            int b = _stream.ReadInt16();
            if (b == -1)
                throw new Exception("End of stream");
            return b;
        }

        public OperationResult OperationResult { get; }

        public int ReadIntSpl()
        {
            return (ReadInt() & 255) | ((ReadInt() & 255) << 8);
        }

        public byte[] ReadNBytes(int count)
        {
            byte[] bArr = new byte[count];
            for (int i = 0; i < count; i++)
            {
                bArr[i] = (byte)ReadInt();
            }
            return bArr;
        }

        public ulong ReadULong()
        {
            int i = 0;
            byte[] buffer = new byte[8];
            int b;
            do
            {
                b = ReadInt();
                int lo = b & 127;
                int rem = 7 - (i / 8);
                int div = i % 8;
                buffer[rem] = (byte) (buffer[rem] | ((lo << div) & 255));
                lo >>= 8 - div;
                if (lo == 0 || rem != 0)
                {
                    if (lo != 0 && div > 0)
                    {
                        rem--;
                        buffer[rem] = (byte)(lo | buffer[rem]);
                    }
                    i = (byte) (i + 7);
                }
                else
                    throw new Exception("Pu64 error");
            } while ((b & 128) != 0);
            return Convert.ToUInt64(buffer);
        }

        public long ReadPu32()
        {
            long j = 0;
            int shift = 0;
            int b;
            do
            {
                b = ReadInt();
                j |= (long) (b & 127) << shift;
                shift = (byte) (shift + 7);
            } while ((b & 128) != 0);

            return j;
        }


        public byte[] ReadBytesByLength()
        {
            return ReadNBytes((int)ReadPu32());
        }

        public string ReadNBytesAsString(int bytesCount)
        {
            var data = ReadNBytes(bytesCount);
            string res = Encoding.UTF8.GetString(data);
            return res;
        }



        public byte[] ReadAllBytes()
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = _stream.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }

        }

        public TreeId ReadTreeId()
        {
            return TreeId.FromStream(this);
        }

        public DateTime ReadDate()
        {
            return ReadULong().ToDateTime();
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}