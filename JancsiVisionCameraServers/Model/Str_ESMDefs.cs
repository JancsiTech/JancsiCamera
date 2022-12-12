using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace JancsiVisionCameraServers.Model
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct ESMHeader
    {
        /* Offset */
        /*   0    */
        public UInt32 magicNumber;
        /*   4    */
        public UInt16 headerSize;
        /*   6    */
        public byte elementSize;
        /*   7    */
        public byte alignment;
        /*   8    */
        public UInt32 width;
        /*  12    */
        public UInt32 height;
        /*  16    */
        public UInt64 acquisitionBeginTime;
        /*  24    */
        public UInt64 reconstructionEndTime;
        /*  32    */
        private fixed byte other1[120];
        /* 152    */
        public UInt64 pointsOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct ESMPoint
    {
        /* Offset */
        /*  0     */
        public float X;
        /*  4     */
        public float Y;
        /*  8     */
        public float Z;
        /* 12     */
        private fixed byte other1[3];
        /* 15     */
        public byte Flags;
    }

    [Flags]
    enum ESMPointFlags : byte
    {
        Mask3D = 1,
    }

}
