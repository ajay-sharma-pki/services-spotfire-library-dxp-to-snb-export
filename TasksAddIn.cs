// <copyright file="TasksAddIn.cs" company="Cloud Software Group, Inc.">
//   Copyright © 2006 - 2023 Cloud Software Group, Inc.
// All rights reserved.
// This software is the confidential and proprietary information
// of Cloud Software Group, Inc. ("Confidential Information"). You shall not
// disclose such Confidential Information and may not use it in any way,
// absent an express written license agreement between you and
// Cloud Software Group, Inc. that authorizes such use.
// </copyright>

namespace SpotfireLibraryToSNBExport
{
    using Spotfire.Dxp.Automation.Extension;

    /// <summary>This AddIn class handles registration of all the base tasks to the Task registry.</summary>
    public sealed class TasksAddIn : RegisterTasksAddIn
    {
        /// <summary>Gets called once the framework is up. Implement this to register your tasks and their executors.</summary>
        /// <param name="registrar">The registrar helper</param>
        public override void RegisterTasks(TaskRegistrar registrar)
        {
            registrar.Register(new ExportFromSpotfireLibraryToSnbTask());
        }

        // View is registered in SpotfireDeveloper.AutomationServicesExample.Forms.ViewAddIn
    }
}