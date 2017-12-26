// <copyright file="BasicHDU.cs" company="Public Domain">
//     Copyright (c) 2017 Samuel Carliles.
// </copyright>

namespace nom.tam.fits
{
    /*
	* Copyright: Thomas McGlynn 1997-1998.
	* This code may be used for any purpose, non-commercial
	* or commercial so long as this copyright notice is retained
	* in the source code or included in or referred to in any
	* derived software.
	*
	* Many thanks to David Glowacki (U. Wisconsin) for substantial
	* improvements, enhancements and bug fixes.
	*/

    using System;
    using System.IO;
    using nom.tam.util;

    /// <summary>This abstract class is the parent of all HDU types.
    /// It provides basic functionality for an HDU.
    /// </summary>
    public abstract class BasicHDU : FitsElement
    {
        #region Properties

        /// <summary>Indicate whether HDU can be primary HDU.
        /// This method must be overriden in HDU types which can
        /// appear at the beginning of a FITS file.</summary>
        internal virtual bool CanBePrimary
        {
            get
            {
                return false;
            }
        }

        /// <summary>Get the associated header</summary>
        virtual public Header Header
        {
            get
            {
                return myHeader;
            }
        }

        /// <summary>Get the starting offset of the HDU</summary>
        virtual public long FileOffset
        {
            get
            {
                return myHeader.FileOffset;
            }
        }

        /// <summary>Get the associated Data object</summary>
        virtual public Data Data
        {
            get
            {
                return myData;
            }
        }

        /// <summary>Get the non-FITS data object</summary>
        virtual public object Kernel
        {
            get
            {
                try
                {
                    return myData.Kernel;
                }
                catch (FitsException)
                {
                    return null;
                }
            }
        }

        /// <summary>Get the total size in bytes of the HDU.</summary>
        /// <returns>The size in bytes.</returns>
        virtual public long Size
        {
            get
            {
                var size = 0;

                if (myHeader != null)
                {
                    size = (int)(size + myHeader.Size);
                }
                if (myData != null)
                {
                    size = (int)(size + myData.Size);
                }
                return size;
            }
        }

        virtual public int BitPix
        {
            get
            {
                var bitpix = myHeader.GetIntValue("BITPIX", -1);
                switch (bitpix)
                {
                case BITPIX_BYTE:
                case BITPIX_SHORT:
                case BITPIX_INT:
                case BITPIX_FLOAT:
                case BITPIX_DOUBLE:
                break;

                default:
                throw new FitsException("Unknown BITPIX type " + bitpix);
                }

                return bitpix;
            }
        }

        virtual public int[] Axes
        {
            get
            {
                var nAxis = myHeader.GetIntValue("NAXIS", 0);
                if (nAxis < 0)
                {
                    throw new FitsException("Negative NAXIS value " + nAxis);
                }
                if (nAxis > 999)
                {
                    throw new FitsException("NAXIS value " + nAxis + " too large");
                }

                if (nAxis == 0)
                {
                    return null;
                }

                var axes = new int[nAxis];
                for (var i = 1; i <= nAxis; i++)
                {
                    axes[nAxis - i] = myHeader.GetIntValue("NAXIS" + i, 0);
                }

                return axes;
            }
        }

        virtual public int ParameterCount
        {
            get
            {
                return myHeader.GetIntValue("PCOUNT", 0);
            }
        }

        virtual public int GroupCount
        {
            get
            {
                return myHeader.GetIntValue("GCOUNT", 1);
            }
        }

        virtual public double BScale
        {
            get
            {
                return myHeader.GetDoubleValue("BSCALE", 1.0);
            }
        }

        virtual public double BZero
        {
            get
            {
                return myHeader.GetDoubleValue("BZERO", 0.0);
            }
        }

        virtual public string BUnit
        {
            get
            {
                return GetTrimmedString("BUNIT");
            }
        }

        virtual public int BlankValue
        {
            get
            {
                if (!myHeader.ContainsKey("BLANK"))
                {
                    throw new FitsException("BLANK undefined");
                }
                return myHeader.GetIntValue("BLANK");
            }
        }

        /// <summary> Get the FITS file creation date as a <CODE>Date</CODE> object.</summary>
        /// <returns>	either <CODE>null</CODE> or a Date object</returns>
        virtual public DateTime CreationDate
        {
            get
            {
                object result = null;

                try
                {
                    result = new FitsDate(myHeader.GetStringValue("DATE")).ToDate();
                }
                catch (FitsException)
                {
                    result = null;
                }

                return (DateTime)result;
            }
        }

        /// <summary> Get the FITS file observation date as a <CODE>Date</CODE> object.</summary>
        /// <returns>	either <CODE>null</CODE> or a Date object</returns>
        virtual public DateTime ObservationDate
        {
            get
            {
                object result = null;

                try
                {
                    result = new FitsDate(myHeader.GetStringValue("DATE-OBS")).ToDate();
                }
                catch (FitsException)
                {
                    result = null;
                }

                return (DateTime)result;
            }
        }

        /// <summary> Get the name of the organization which created this FITS file.</summary>
        /// <returns>	either <CODE>null</CODE> or a String object</returns>
        virtual public string Origin
        {
            get
            {
                return GetTrimmedString("ORIGIN");
            }
        }

        /// <summary> Get the name of the telescope which was used to acquire the data in this FITS file.</summary>
        /// <returns>	either <CODE>null</CODE> or a String object</returns>
        virtual public string Telescope
        {
            get
            {
                return GetTrimmedString("TELESCOP");
            }
        }

        /// <summary> Get the name of the instrument which was used to acquire the data in this FITS file.</summary>
        /// <returns>	either <CODE>null</CODE> or a String object</returns>
        virtual public string Instrument
        {
            get
            {
                return GetTrimmedString("INSTRUME");
            }
        }

        /// <summary>Get the name of the person who acquired the data in this FITS file.</summary>
        /// <returns>	either <CODE>null</CODE> or a String object</returns>
        virtual public string Observer
        {
            get
            {
                return GetTrimmedString("OBSERVER");
            }
        }

        /// <summary> Get the name of the observed object in this FITS file.</summary>
        /// <returns>	either <CODE>null</CODE> or a String object</returns>
        virtual public string Object
        {
            get
            {
                return GetTrimmedString("OBJECT");
            }
        }

        /// <summary> Get the equinox in years for the celestial coordinate system in which
        /// positions given in either the header or data are expressed.</summary>
        /// <returns>	either <CODE>null</CODE> or a String object</returns>
        virtual public double Equinox
        {
            get
            {
                return myHeader.GetDoubleValue("EQUINOX", -1.0);
            }
        }

        /// <summary> Get the equinox in years for the celestial coordinate system in which
        /// positions given in either the header or data are expressed.</summary>
        /// <returns>	either <CODE>null</CODE> or a String object</returns>
        /// <deprecated>	Replaced by getEquinox</deprecated>
        /// <seealso cref="">#getEquinox()</seealso>
        virtual public double Epoch
        {
            get
            {
                return myHeader.GetDoubleValue("EPOCH", -1.0);
            }
        }

        /// <summary> Return the name of the person who compiled the information in
        /// the data associated with this header.</summary>
        /// <returns>	either <CODE>null</CODE> or a String object</returns>
        virtual public string Author
        {
            get
            {
                return GetTrimmedString("AUTHOR");
            }
        }

        /// <summary> Return the citation of a reference where the data associated with
        /// this header are published.</summary>
        /// <returns>	either <CODE>null</CODE> or a String object</returns>
        virtual public string Reference
        {
            get
            {
                return GetTrimmedString("REFERENC");
            }
        }

        /// <summary> Return the minimum valid value in the array.</summary>
        /// <returns>	minimum value.</returns>
        virtual public double MaximumValue
        {
            get
            {
                return myHeader.GetDoubleValue("DATAMAX");
            }
        }

        /// <summary> Return the minimum valid value in the array.</summary>
        /// <returns>	minimum value.</returns>
        virtual public double MinimumValue
        {
            get
            {
                return myHeader.GetDoubleValue("DATAMIN");
            }
        }

        /// <summary>Indicate that an HDU is the first element of a FITS file.</summary>
        virtual internal bool PrimaryHDU
        {
            set
            {
                if (value && !CanBePrimary)
                {
                    throw new FitsException("Invalid attempt to make HDU of type:" + GetType().FullName + " primary.");
                }
                else
                {
                    isPrimary = value;
                }

                // Some FITS readers don't like the PCOUNT and GCOUNT keywords
                // in a primary array or they EXTEND keyword in extensions.

                if (isPrimary && !myHeader.GetBooleanValue("GROUPS", false))
                {
                    myHeader.DeleteKey("PCOUNT");
                    myHeader.DeleteKey("GCOUNT");
                }

                if (isPrimary)
                {
                    var card = myHeader.FindCard("EXTEND");
                    if (card == null)
                    {
                        myHeader.NextCard();
                        myHeader.AddValue("EXTEND", true, "Allow extensions");
                    }
                }

                if (!isPrimary)
                {
                    var c = myHeader.GetCursor();

                    var pcount = myHeader.GetIntValue("PCOUNT", 0);
                    var gcount = myHeader.GetIntValue("GCOUNT", 1);
                    var naxis = myHeader.GetIntValue("NAXIS", 0);
                    myHeader.DeleteKey("EXTEND");

                    //HeaderCard card;
                    var pcard = myHeader.FindCard("PCOUNT");
                    var gcard = myHeader.FindCard("GCOUNT");

                    myHeader.GetCard(2 + naxis);
                    if (pcard == null)
                    {
                        myHeader.AddValue("PCOUNT", pcount, "Required value");
                    }
                    if (gcard == null)
                    {
                        myHeader.AddValue("GCOUNT", gcount, "Required value");
                    }
                    c = myHeader.GetCursor();
                }
            }
        }

        public static BasicHDU DummyHDU
        {
            get
            {
                try
                {
                    return FitsFactory.HDUFactory(new int[0]);
                }
                catch (FitsException)
                {
                    Console.Error.WriteLine("Impossible exception in GetDummyHDU");
                    return null;
                }
            }
        }

        /// <summary>Is the HDU rewriteable</summary>
        public virtual bool Rewriteable
        {
            get
            {
                return myHeader.Rewriteable && myData.Rewriteable;
            }
        }

        #endregion Properties

        #region Class Variables

        public const int BITPIX_BYTE = 8;
        public const int BITPIX_SHORT = 16;
        public const int BITPIX_INT = 32;
        public const int BITPIX_LONG = 64;
        public const int BITPIX_FLOAT = -32;
        public const int BITPIX_DOUBLE = -64;

        #endregion Class Variables

        #region Instance Variables

        /// <summary>The associated header.
        /// </summary>
        protected internal Header myHeader = null;

        /// <summary>The associated data unit.
        /// </summary>
        protected internal Data myData = null;

        /// <summary>Is this the first HDU in a FITS file?
        /// </summary>
        protected internal bool isPrimary = false;

        #endregion Instance Variables

        /// <summary>Create a Data object to correspond to the header description.</summary>
        /// <returns> An unfilled Data object which can be used to read in the data for this HDU.</returns>
        /// <exception cref=""> FitsException if the Data object could not be created
        /// from this HDU's Header</exception>
        internal abstract Data ManufactureData();

        /// <summary>Skip the Data object immediately after the given Header object on
        /// the given stream object.</summary>
        /// <param name="stream">the stream which contains the data.</param>
        /// <param name="Header">template indicating length of Data section</param>
        /// <exception cref=""> IOException if the Data object could not be skipped.</exception>
        public static void SkipData(ArrayDataIO stream, Header hdr)
        {
            //System.IO.BinaryReader temp_BinaryReader;
            long temp_Int64;

            //temp_BinaryReader = stream;
            temp_Int64 = stream.Position; //temp_BinaryReader.BaseStream.Position;
            temp_Int64 = stream.Seek((int)hdr.DataSize) - temp_Int64; //temp_BinaryReader.BaseStream.Seek((int) hdr.DataSize, System.IO.SeekOrigin.Current) - temp_Int64;
            var generatedAux = (int)temp_Int64;
        }

        /// <summary>Skip the Data object for this HDU.</summary>
        /// <param name="stream">the stream which contains the data.</param>
        /// <exception cref=""> IOException if the Data object could not be skipped.</exception>
        public virtual void SkipData(ArrayDataIO stream)
        {
            SkipData(stream, myHeader);
        }

        /// <summary>Read in the Data object for this HDU.</summary>
        /// <param name="stream">the stream from which the data is read.</param>
        /// <exception cref=""> FitsException if the Data object could not be created from this HDU's Header</exception>
        public virtual void ReadData(ArrayDataIO stream)
        {
            myData = null;
            try
            {
                myData = ManufactureData();
            }
            finally
            {
                // if we cannot build a Data object, skip this section
                if (myData == null)
                {
                    try
                    {
                        SkipData(stream, myHeader);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            myData.Read(stream);
        }

        /// <summary>Check that this is a valid header for the HDU.</summary>
        /// <param name="header">to validate.</param>
        /// <returns> <CODE>true</CODE> if this is a valid header.</returns>
        public static bool IsHeader(Header header)
        {
            return false;
        }

        /// <summary>Print out some information about this HDU.</summary>
        public abstract void Info();

        /// <summary>Check if a field is present and if so print it out.</summary>
        /// <param name="name">The header keyword.</param>
        /// <returns>Was it found in the header?</returns>
        internal virtual bool CheckField(string name)
        {
            var value_Renamed = myHeader.GetStringValue(name);
            if (value_Renamed == null)
            {
                return false;
            }

            return true;
        }

        /* Read out the HDU from the data stream.  This
		* will overwrite any existing header and data components.
		*/

        public virtual void Read(ArrayDataIO stream)
        {
            myHeader = Header.ReadHeader(stream);
            myData = myHeader.MakeData();
            myData.Read(stream);
        }

        /* Write out the HDU
		* @param stream The data stream to be written to.
		*/

        public virtual void Write(ArrayDataIO stream)
        {
            if (myHeader != null)
            {
                myHeader.Write(stream);
            }
            if (myData != null)
            {
                myData.Write(stream);
            }
            try
            {
                stream.Flush();
            }
            catch (IOException e)
            {
                throw new FitsException("Error flushing at end of HDU: " + e.Message);
            }
        }

        /// <summary>Rewrite the HDU</summary>
        public virtual void Rewrite()
        {
            if (Rewriteable)
            {
                myHeader.Rewrite();
                myData.Rewrite();
            }
            else
            {
                throw new FitsException("Invalid attempt to rewrite HDU");
            }
        }

        /// <summary> Get the String value associated with <CODE>keyword</CODE>.</summary>
        /// <param name="hdr	the">header piece of an HDU</param>
        /// <param name="keyword	the">FITS keyword</param>
        /// <returns>	either <CODE>null</CODE> or a String with leading/trailing blanks stripped.</returns>
        public virtual string GetTrimmedString(string keyword)
        {
            var s = myHeader.GetStringValue(keyword);
            if (s != null)
            {
                s = s.Trim();
            }
            return s;
        }

        /// <summary>Add information to the header</summary>
        public virtual void AddValue(string key, bool val, string comment)
        {
            myHeader.AddValue(key, val, comment);
        }

        public virtual void AddValue(string key, int val, string comment)
        {
            myHeader.AddValue(key, val, comment);
        }

        public virtual void AddValue(string key, double val, string comment)
        {
            myHeader.AddValue(key, val, comment);
        }

        public virtual void AddValue(string key, string val, string comment)
        {
            myHeader.AddValue(key, val, comment);
        }
    }
}
