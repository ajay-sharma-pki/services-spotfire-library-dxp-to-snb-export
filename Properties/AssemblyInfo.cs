// <copyright file="AssemblyInfo.cs" company="Cloud Software Group, Inc.">
//   Copyright © 2006 - 2023 Cloud Software Group, Inc.
// All rights reserved.
// This software is the confidential and proprietary information
// of Cloud Software Group, Inc. ("Confidential Information"). You shall not
// disclose such Confidential Information and may not use it in any way,
// absent an express written license agreement between you and
// Cloud Software Group, Inc. that authorizes such use.
// </copyright>

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("SpotfireLibraryToSNBExport")]
[assembly: AssemblyDescription("Spotfire Automation Job to move DXP files from Spotfire Library to SNB")]
[assembly: AssemblyDefaultAlias("SpotfireLibraryToSNBExport.dll")]
[assembly: AssemblyCompany("Revvity, Inc.")]
[assembly: AssemblyProduct("Spotfire Automation Services")]
[assembly: AssemblyCopyright("\x00a9 TIBCO Software Inc. All rights reserved.")]
[assembly: CLSCompliant(true)]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM componenets.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.MainAssembly)]

#if DEBUG

[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
// If you modify the version, make sure you also modify the module.xml file used by
// the TIBCO Spotfire client application to load your extension assembly
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
