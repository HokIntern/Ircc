using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.BitConverter;

namespace Ircc
{
    public struct Header
    {
        public short comm;
        public short code;
        public int size;
        public int reserved;

        public Header(short comm, short code, int size, int reserved)
        {
            this.comm = comm;
            this.code = code;
            this.size = size;
            this.reserved = reserved;
        }
    }
    public struct Packet
    {
        public Header header;
        public byte[] data;

        public Packet(Header header, byte[] data)
        {
            this.header = header;
            this.data = data;
        }
    }
    public static class IrccHelper
    {
        public class FieldIndex
        {
            public const int COMM = 0;
            public const int CODE = 2;
            public const int SIZE = 4;
            public const int RSVD = 8;
            public const int DATA = 12;
        }

        public static byte[] packetToBytes(Packet p)
        {
            /*
            byte[] msg = new byte[size];
            
            msg[0-1] = BitConverter.GetBytes(p.header.comm);
            msg[2-4] = 

            Array.Copy(GetBytes(p.header.comm), 0, msg, 0, 2);
            Array.Copy(GetBytes(p.header.code), 0, msg, 2, 2);
            Array.Copy(GetBytes(p.header.size), 0, msg, 4, 4);
            Array.Copy(GetBytes(p.header.reserved), 0, msg, 8, 4);
            Array.Copy(p.data, 0, msg, 12, p.data.size);
           
            //////////////////// 
            comm type (2byte)
            msg[0] = p.header.comm & 11110000b;
            msg[1] = p.header.comm & 00001111b;

            code (2byte)
            msg[2] = p.header.code & 11110000b;
            msg[3] = p.header.code & 00001111b;

            size (4byte)
            msg[4] = p.header.size
            msg[5] = p.header.size
            msg[6] = p.header.size
            msg[7] = p.header.size

            reserved (4byte)
            msg[] = p.header.reserved
            msg[] = p.header.reserved
            msg[] = p.header.reserved
            msg[] = p.header.reserved
            */

            byte[] bComm = GetBytes(p.header.comm);
            byte[] bCode = GetBytes(p.header.code);
            byte[] bSize = GetBytes(p.header.size);
            byte[] bRsvd = GetBytes(p.header.reserved);

            return bComm.Concat(bCode).Concat(bSize).Concat(bRsvd).Concat(p.data).ToArray();
        }

        /*
        public static HeaderToByte //send this first
        public static BodyToByte   //process this while sending and send when finished
        public static ByteToHeader //get this first
        public static ByteToPacket //get buffer size from header and receive (give header as argument)
        */

        public static Header bytesToHeader(byte[] b)
        {
            Header h = new Header();

            h.comm = ToInt16(b, FieldIndex.COMM);
            h.code = ToInt16(b, FieldIndex.CODE);
            h.size = ToInt32(b, FieldIndex.SIZE);
            h.reserved = ToInt32(b, FieldIndex.RSVD);

            return h;
        }
        public static Packet bytesToPacket(byte[] b)
        {
            Packet p = new Packet();

            p.header.comm = ToInt16(b, FieldIndex.COMM);
            p.header.code = ToInt16(b, FieldIndex.CODE);
            p.header.size = ToInt32(b, FieldIndex.SIZE);
            p.header.reserved = ToInt32(b, FieldIndex.RSVD);
            Array.Copy(b, FieldIndex.DATA, p.data, 0, p.header.size);

            return p;
        }

        /*
        private static byte[] SubArray(this byte[] data, int index, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
        */
    }
}
