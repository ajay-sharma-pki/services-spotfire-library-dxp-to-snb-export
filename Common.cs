// <copyright file="Common.cs" company="Cloud Software Group, Inc.">
//   Copyright © 2006 - 2023 Cloud Software Group, Inc.
// All rights reserved.
// This software is the confidential and proprietary information
// of Cloud Software Group, Inc. ("Confidential Information"). You shall not
// disclose such Confidential Information and may not use it in any way,
// absent an express written license agreement between you and
// Cloud Software Group, Inc. that authorizes such use.
// </copyright>

using System.Globalization;

namespace SpotfireLibraryToSNBExport
{
    internal class Common
    {
        #region Constants and Fields

        /// <summary>The namespace to use when serializing the tasks.</summary>
        internal const string TaskSerializingNamespace = "urn:spotfirelibrarytosnbexport";

        #endregion

        #region Methods

        /// <summary>
        /// A utility method that formats a string using the current thread's
        /// <see cref="System.Globalization.CultureInfo">CurrentCulture</see>.
        /// </summary>
        /// <param name="spec">The specification.</param>
        /// <param name="args">An <see cref="System.object">Object</see>
        /// array containing zero or more objects to format.</param>
        /// <returns>
        /// A copy of the format argument in which the format items
        /// have been replaced by the <b>String</b> equivalent of the
        /// corresponding instances of <b>Object</b> in args.
        /// </returns>
        internal static string Format(string spec, params object[] args)
        {
            return string.Format(System.Threading.Thread.CurrentThread.CurrentCulture, spec, args);
        }

        /// <summary>A utility method that formats a string using the 
        /// <see cref="System.Globalization.CultureInfo">InvariantCulture</see>.
        /// </summary>
        /// <param name="format">A <see cref="System.string">String</see> 
        /// containing zero or more format items.</param>
        /// <param name="args">An <see cref="System.object">Object</see> 
        /// array containing zero or more objects to format.</param>
        /// <returns>A copy of the format argument in which the format items 
        /// have been replaced by the <b>String</b> equivalent of the 
        /// corresponding instances of <b>Object</b> in args.</returns>
        internal static string FormatInvariant(string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }

        #endregion
    }
}