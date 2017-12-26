// <copyright file="RandomGroupsData.cs" company="Public Domain">
//     Copyright (c) 2017 Samuel Carliles.
// </copyright>

namespace nom.tam.fits
{
    /* Copyright: Thomas McGlynn 1998.
	* This code may be used for any purpose, non-commercial
	* or commercial so long as this copyright notice is retained
	* in the source code or included in or referred to in any
	* derived software.
	* Many thanks to David Glowacki (U. Wisconsin) for substantial
	* improvements, enhancements and bug fixes.
	*/

    using System;
    using System.IO;
    using nom.tam.util;

    /// <summary>This class instantiates FITS Random Groups data.
    /// Random groups are instantiated as a two-dimensional
    /// array of objects.  The first dimension of the array
    /// is the number of groups.  The second dimension is 2.
    /// The first object in every row is a one dimensional
    /// parameter array.  The second element is the n-dimensional
    /// data array.
    /// </summary>
    public class RandomGroupsData : Data
    {
        #region Properties

        /// <summary>Get the size of the actual data element.
        /// </summary>
        override internal int TrueSize
        {
            get
            {
                if (dataArray != null && dataArray.Length > 0)
                {
                    //return (ArrayFuncs.ComputeSize(dataArray[0][0]) + ArrayFuncs.ComputeSize(dataArray[0][1])) * dataArray.Length;
                    return (ArrayFuncs.ComputeSize(dataArray.GetValue(0, 0)) +
                      ArrayFuncs.ComputeSize(dataArray.GetValue(0, 1))) * dataArray.Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        override public object DataArray
        {
            get
            {
                return dataArray;
            }
        }

        #endregion Properties

        //private Object[][] dataArray;
        private Array dataArray;

        #region Constructors

        /// <summary>Create the equivalent of a null data element.
        /// </summary>
        public RandomGroupsData()
        {
            dataArray = new object[0][];
        }

        /// <summary>Create a RandomGroupsData object using the specified object to
        /// initialize the data array.
        /// </summary>
        /// <param name="x">The initial data array.  This should a two-d
        /// array of objects as described above.
        ///
        /// </param>
        //		public RandomGroupsData(System.Object[][] x)
        public RandomGroupsData(Array x)
        {
            dataArray = x;
        }

        #endregion Constructors

        /// <summary>Read the RandomGroupsData</summary>
        public override void Read(ArrayDataIO str)
        {
            SetFileOffset(str);

            try
            {
                str.ReadArray(dataArray);
            }
            catch (IOException e)
            {
                throw new FitsException("IO error reading Random Groups data " + e);
            }
            var pad = FitsUtil.Padding(TrueSize);
            try
            {
                //System.IO.BinaryReader temp_BinaryReader;
                System.Int64 temp_Int64;

                //temp_BinaryReader = str;
                temp_Int64 = str.Position;  //temp_BinaryReader.BaseStream.Position;
                temp_Int64 = str.Seek(pad) - temp_Int64;  //temp_BinaryReader.BaseStream.Seek(pad, System.IO.SeekOrigin.Current) - temp_Int64;
                var generatedAux = (int)temp_Int64;
            }
            catch (IOException)
            {
                throw new FitsException("IO error reading padding.");
            }
        }

        /// <summary>Write the RandomGroupsData</summary>
        public override void Write(ArrayDataIO str)
        {
            try
            {
                str.WriteArray(dataArray);
                var padding = new byte[FitsUtil.Padding(TrueSize)];
                str.Write(padding);
                str.Flush();
            }
            catch (IOException e)
            {
                throw new FitsException("IO error writing random groups data " + e);
            }
        }

        internal override void FillHeader(Header h)
        {
            var dims = ArrayFuncs.GetDimensions(dataArray);

            //if (dataArray.Length <= 0 || dataArray[0].Length != 2)
            if (dims.Length != 2)
            {
                throw new FitsException("Data not conformable to Random Groups");
            }

            var gcount = dataArray.Length;

            //Object paraSamp = dataArray[0][0];
            //Object dataSamp = dataArray[0][1];
            var paraSamp = dataArray.GetValue(0, 0);
            var dataSamp = dataArray.GetValue(0, 1);

            var pbase = ArrayFuncs.GetBaseClass(paraSamp);
            var dbase = ArrayFuncs.GetBaseClass(dataSamp);

            if (pbase != dbase)
            {
                throw new FitsException("Data and parameters do not agree in type for random group");
            }

            var pdims = ArrayFuncs.GetDimensions(paraSamp);
            var ddims = ArrayFuncs.GetDimensions(dataSamp);

            if (pdims.Length != 1)
            {
                throw new FitsException("Parameters are not 1 d array for random groups");
            }

            // Got the information we need to build the header.

            h.Simple = true;
            if (dbase == typeof(byte))
            {
                h.Bitpix = 8;
            }
            else if (dbase == typeof(short))
            {
                h.Bitpix = 16;
            }
            else if (dbase == typeof(int))
            {
                h.Bitpix = 32;
            }
            else if (dbase == typeof(long))
            {
                // Non-standard
                h.Bitpix = 64;
            }
            else if (dbase == typeof(float))
            {
                h.Bitpix = -32;
            }
            else if (dbase == typeof(double))
            {
                h.Bitpix = -64;
            }
            else
            {
                throw new FitsException("Data type:" + dbase + " not supported for random groups");
            }

            h.Naxes = ddims.Length + 1;
            h.AddValue("NAXIS1", 0, "");
            for (var i = 2; i <= ddims.Length + 1; i += 1)
            {
                h.AddValue("NAXIS" + i, ddims[i - 2], "");
            }

            h.AddValue("GROUPS", true, "");
            h.AddValue("GCOUNT", dataArray.Length, "");
            h.AddValue("PCOUNT", pdims[0], "");
        }
    }
}
