using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.DataTypes
{
    public class Byte
    {
        public static byte[] ToByteArray(byte value)
        {
            return new byte[] { value }; ;
        }

        /// <summary>
        /// Converts a byte array to byte
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte FromByteArray(byte[] bytes)
        {
            if (bytes.Length != 1)
            {
                throw new ArgumentException("Wrong number of bytes. Bytes array must contain 1 bytes.");
            }
            return bytes[0];
        }
    }
}
