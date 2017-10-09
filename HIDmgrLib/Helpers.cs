using System;


namespace FineOffsetLib.Helpers {

        public static class MyExtensions {
            public static short FIX_SIGN(this int v) {
                short MAGNITUDE_BITS = Convert.ToInt16(Convert.ToInt32(v) & 0x7fff);
                short MSB = Convert.ToInt16((Convert.ToInt32(v) >> 8) & 0xff);
                short SIGN_BIT = Convert.ToInt16(Convert.ToInt32(MSB) >> 7);

                short sign = Convert.ToInt16((Convert.ToBoolean(SIGN_BIT) ? -1 : 1) * MAGNITUDE_BITS);

                return sign;
            }
        }


}
