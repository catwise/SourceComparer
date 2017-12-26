// <copyright file="PrimitiveInfo.cs" company="Public Domain">
//     Copyright (c) 2017 Samuel Carliles.
// </copyright>

namespace nom.tam.util
{
    using System;
    using System.Collections;

    /// <summary>This interface collects some information about Java primitives.
    /// </summary>
    public class PrimitiveInfo
    {
        public readonly static Hashtable sizes;

        static PrimitiveInfo()
        {
            sizes = new Hashtable
            {
                [typeof(byte)] = 1, // BitConverter.GetBytes((byte)0).Length;
                [typeof(sbyte)] = BitConverter.GetBytes((sbyte)0).Length,
                [typeof(bool)] = BitConverter.GetBytes(true).Length,
                [typeof(char)] = 1,//BitConverter.GetBytes('a').Length;
                [typeof(short)] = BitConverter.GetBytes((short)0).Length,
                [typeof(ushort)] = BitConverter.GetBytes((ushort)0).Length,
                [typeof(int)] = BitConverter.GetBytes((int)0).Length,
                [typeof(uint)] = BitConverter.GetBytes((uint)0).Length,
                [typeof(long)] = BitConverter.GetBytes((long)0).Length,
                [typeof(ulong)] = BitConverter.GetBytes((ulong)0).Length,
                [typeof(float)] = BitConverter.GetBytes((float)0.0).Length,
                [typeof(double)] = BitConverter.GetBytes((double)0.0).Length
            };
        }

        /// <summary>Suffixes used for the classnames for primitive arrays.
        /// </summary>
        /// <summary>Classes of the primitives. These should be in windening order
        /// (char is as always a problem).
        /// </summary>
        /// <summary>Is this a numeric class
        /// </summary>
        /// <summary>Full names
        /// </summary>
        /// <summary>Sizes
        /// </summary>
        /// <summary>Index of first element of above arrays referring to a numeric type
        /// </summary>
        /// <summary>Index of last element of above arrays referring to a numeric type
        /// </summary>
    }
}
