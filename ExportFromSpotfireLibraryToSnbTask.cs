using System.ComponentModel;
using System.IO;



namespace SpotfireLibraryToSNBExport
{
    using Spotfire.Dxp.Application;
    using Spotfire.Dxp.Automation.Extension;
    using Spotfire.Dxp.Automation.Framework;
    using Spotfire.Dxp.Framework.Library;
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using Spotfire.Dxp.Framework.ApplicationModel;
    using System.Xml.Serialization;
    using System.Text.Json.Nodes;
    using System.Threading;

    [XmlRoot(Namespace = Common.TaskSerializingNamespace)]
    public sealed class ExportFromSpotfireLibraryToSnbTask : Task
    {
        #region Constructors and Destructors

        public ExportFromSpotfireLibraryToSnbTask()
            : base(Properties.Resources.Title, Properties.Resources.Description)
        {
        }
        #endregion

        #region Properties

        // Path to the settings file is set here
        [Description("Library Folder")]
        public string LibraryFolder { get; set; }

        [Description("Log Output Path")]
        public string LogsPath { get; set; }

        [Description("SNB URL")]
        public string SnbUrl { get; set; }

        [Description("SNB API Key")]
        public string SnbApiKey { get; set; }

        [Description("Experiment Template ID")]
        public string ExperimentTemplateId { get; set; }

        [Description("Notebook Template ID")]
        public string NotebookTemplateId { get; set; }

        [Description("Spotfire API User Client ID")]
        public string SpiotfireApiUserClientId { get; set; }

        [Description("Spotfire API User Client Secret")]
        public string SpotfireApiUserClientSecret { get; set; }

        [Description("Spotfire Server URL")]
        public string SpotfireServerUrl { get; set; }
        #endregion

        #region Methods

        public void WriteToLogs(string path, string message)
        {
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(message);
            }
        }


        protected override TaskExecutionStatus ExecuteCore(TaskExecutionContext context)
        {
            string outPath = LogsPath;
            string pluginLogsFilePath = LogsPath.EndsWith("/") ? $"{LogsPath}export_plugin_logs.txt" : $"{LogsPath}/export_plugin_logs.txt";
            string errorLogsFilePath = LogsPath.EndsWith("/") ? $"{LogsPath}error_plugin_logs.txt" : $"{LogsPath}/error_plugin_logs.txt";
            string failedExportFileLog = LogsPath.EndsWith("/") ? $"{LogsPath}failed_export_files_logs.txt" : $"{LogsPath}/failed_export_files_logs.txt";
            string successExportFileLog = LogsPath.EndsWith("/") ? $"{LogsPath}successful_export_files_logs.txt" : $"{LogsPath}/successful_export_files_logs.txt";
            string spotfireAuthToken = null;
            DateTime tokenGenTime = DateTime.Now;
            int tokenExpiresIn = 7200;
            ProgressService.CurrentProgress.ExecuteSubtask("Starting Spotfrire Analysis file export to SNB");
            int totalFiles = 0;
            using (AnalysisApplication application = context.Application)
            {
                DateTime startTime = DateTime.Now;
                String path = "";
                int numProcessed = 0;
                try
                {
                    LibraryManager libraryManager = application.GetService<LibraryManager>();
                    string folderToProcess = LibraryFolder;
                    if (!File.Exists(pluginLogsFilePath))
                    {
                        File.Create(pluginLogsFilePath).Close();
                    }
                    if (!File.Exists(errorLogsFilePath))
                    {
                        File.Create(errorLogsFilePath).Close();
                    }
                    if (!File.Exists(failedExportFileLog))
                    {
                        File.Create(failedExportFileLog).Close();
                    }
                    if (!File.Exists(successExportFileLog))
                    {
                        File.Create(successExportFileLog).Close();
                    }
                    WriteToLogs(pluginLogsFilePath, string.Format("Library Folder to process: {0}", folderToProcess));
                    LibraryItemCollection analysisFiles = libraryManager.Search(string.Format("item_type:dxp", folderToProcess), LibraryItemRetrievalOption.IncludeProperties, LibraryItemRetrievalOption.IncludePath);

                    //string folderToProcessPath = "";
                    if (analysisFiles == null || analysisFiles.Count == 0)
                    {
                        WriteToLogs(pluginLogsFilePath, string.Format("No analysisFiles found in the LibraryPath '{0}'.", folderToProcess));
                    }
                    totalFiles = analysisFiles.Count;
                    WriteToLogs(pluginLogsFilePath, $"Total DXP files in Library: {analysisFiles.Count}");
                    List<string> libraryItems = new List<string>();
                    string[] processedFiles = File.ReadAllLines(successExportFileLog);

                    
                    
                    foreach (LibraryItem analysisFile in analysisFiles)
                    {
                        //LibraryItemType itemType = analysisFile.ItemType;
                        path = analysisFile.Path != null ? analysisFile.Path : "";
                        // Check if the array contains the value
                        bool valueExists = processedFiles.Contains(path);
                        if (valueExists)
                        {
                            WriteToLogs(pluginLogsFilePath, $"already procesed. skipping file {path}");
                            continue;
                        }
                        string name = analysisFile.Title != null ? analysisFile.Title : "";
                        string description = analysisFile.Description != null ? analysisFile.Description : "";
                        DateTime dateAnalysisFileCreated = analysisFile.Created;
                        DateTime dateAnalysisFileModified = analysisFile.LastModified;

                        Utils snbUtils = new Utils();
                        snbUtils.PluginLogsPath = pluginLogsFilePath;
                        snbUtils.ErrorLogsPath = errorLogsFilePath;
                        snbUtils.FailedExportLogsPath = failedExportFileLog;
                        snbUtils.SuccessfulExportsLogsPath = successExportFileLog;
                        snbUtils.SnbUrl = SnbUrl;
                        snbUtils.SnbApiKey = SnbApiKey;
                        snbUtils.SnbExperimentTemplateId = ExperimentTemplateId;
                        snbUtils.SnbNotebookTemplateId = NotebookTemplateId;
                        snbUtils.NewExperimentName = name;
                        snbUtils.NewExperimentDescription = description;
                        snbUtils.AnalysisFileCreateTime = dateAnalysisFileCreated != null ? dateAnalysisFileCreated.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") : "";
                        snbUtils.AnalysisFileModifiedTime = dateAnalysisFileModified != null ? dateAnalysisFileModified.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") : "";
                        snbUtils.AnalysisFileSpotfireLibraryPath = path != null ? path : "";
                        snbUtils.SpotfireServerUrl = SpotfireServerUrl;
                        snbUtils.SpotfireApiUserClientSecret = SpotfireApiUserClientSecret;
                        snbUtils.SpotfireApiUserClientId1 = SpiotfireApiUserClientId;
                        snbUtils.AnalysisFileId = analysisFile.Id.ToString();
                        snbUtils.SpotfireAccessToken1 = spotfireAuthToken;

                        if (path.StartsWith("/Project Folders/"))
                        {
                            string[] pathVals = path.Split('/');
                            snbUtils.Project = pathVals.Length > 3 ? pathVals[2] : "";
                            snbUtils.Department = pathVals.Length > 3 ? pathVals[3] : "";
                        }
                        else
                        {
                            snbUtils.Project = "";
                            snbUtils.Department = "";
                        }

                        ProgressService.CurrentProgress.ExecuteSubtask(string.Format($"Processing File {path}"));
                        WriteToLogs(pluginLogsFilePath, $"processing file: {path}");

                        string filePath = outPath.EndsWith("/") ? $"{outPath}{name}.dxp" : $"{outPath}/{name}.dxp";
                        //DocumentOpenSettings openSettings = new DocumentOpenSettings
                        //{
                        //    AutoConfigure = true,
                        //    AutoCreateFilters = true,
                        //    EnableUndoRedo = true
                        //};

                        // generate or renew spotfire auth token
                        if (string.IsNullOrWhiteSpace(spotfireAuthToken) || DateTime.Now >= tokenGenTime.AddSeconds(7200))
                        {
                            var tokenData = snbUtils.GetSpotfireAccessToken();
                            if (tokenData != null )
                            {
                                spotfireAuthToken = $"Bearer {tokenData["access_token"]}";
                                tokenExpiresIn = int.Parse(tokenData["expires_in"]?.ToString());
                                tokenGenTime = DateTime.Now;
                                snbUtils.SpotfireAccessToken1 = spotfireAuthToken;
                            }
                        }

                        // get info for user created the analysis file and user modified the analysis file
                        if (analysisFile.Id != null)
                        {
                            JsonObject analysisFileInfo = snbUtils.GetLibraryItemInfo(analysisFile.Id.ToString());
                            if (analysisFileInfo != null)
                            {
                                var createdByInfo = analysisFileInfo["createdBy"];
                                snbUtils.AnalysisFileCreatedBy = (createdByInfo != null && createdByInfo["displayName"] != null) ? createdByInfo["displayName"]?.ToString() : "";
                              

                                var modifiedByInfo = analysisFileInfo["modifiedBy"];
                                snbUtils.AnalysisFileModifiedBy = (modifiedByInfo != null && modifiedByInfo["displayName"] != null) ? modifiedByInfo["displayName"]?.ToString(): "";
                            }
                        }
                        
                        ExportToSnb(snbUtils);
                        numProcessed++;
                        //ProcessAnalysisFile(analysisFile, openSettings, application, snbUtils, filePath);
                    }
                }
                
                catch (Exception ex)
                {
                    WriteToLogs(pluginLogsFilePath, $"Error while exporting Analysis file.\n{ex.ToString()}");
                    WriteToLogs(errorLogsFilePath, $"Error while exporting Analysis file.\n{ex.Message}\n{ex.StackTrace}");
                    WriteToLogs(failedExportFileLog, path);
                    //application.Close();
                    //return new TaskExecutionStatus(true, ex.ToString());
                }
                DateTime endTime = DateTime.Now;
                TimeSpan difference = endTime - startTime;

                double duration = difference.TotalHours;
                WriteToLogs(pluginLogsFilePath, string.Format($"Processing complete.\nTotal Files: {totalFiles}\nProcessed files: {numProcessed}.\nTotal time: {duration} hours"));
                ProgressService.CurrentProgress.ExecuteSubtask(string.Format($"Processing complete.\nTotal Files: {{totalFiles}}\nProcessed files: {{numProcessed}}.\nTotal time: {{duration}} hours"));

                return new TaskExecutionStatus(true, string.Format($"Processing complete.\nTotal Files: {{totalFiles}}\nProcessed files: {{numProcessed}}.\nTotal time: {{duration}} hours"));
            }
        }

        

        private void ExportToSnb(Utils snbUtils)
        {
            byte[] libraryItem = snbUtils.GetLibraryItem();

            if (libraryItem != null && libraryItem.Length > 0)
            {
                string respVals = snbUtils.SendCreateSnbExperimentRequest();
                if (!string.IsNullOrWhiteSpace(respVals))
                {
                    string[] respValsSplit = !string.IsNullOrWhiteSpace(respVals) ? respVals.Split(',') : new string[0];

                    string experimentId = respValsSplit.Length == 2 ? respValsSplit[0] : null;
                    string dxpAttachmentId = respValsSplit.Length == 2 ? respValsSplit[1] : null;
                    //WriteToLogs(snbUtils.PluginLogsPath, filePath);
                    bool attachmentAdded = snbUtils.AddDxpAsAttachment(libraryItem);
                    if (!attachmentAdded)
                    {
                        WriteToLogs(snbUtils.ErrorLogsPath, $"failed to attach Analysis file to SNB experiment: {snbUtils.AnalysisFileSpotfireLibraryPath}");
                        WriteToLogs(snbUtils.FailedExportLogsPath, snbUtils.AnalysisFileSpotfireLibraryPath);
                    }
                    else
                    {
                        WriteToLogs(snbUtils.PluginLogsPath, $"successfully attached Analysis file to SNB experiment: {snbUtils.AnalysisFileSpotfireLibraryPath}");
                        WriteToLogs(snbUtils.SuccessfulExportsLogsPath, snbUtils.AnalysisFileSpotfireLibraryPath);
                        //snbUtils.deleteSavedAnalysisFile(filePath);
                    }
                }
                else
                {
                    WriteToLogs(snbUtils.ErrorLogsPath, $"failed to create SNB experiment for file {snbUtils.AnalysisFileSpotfireLibraryPath}");
                    WriteToLogs(snbUtils.FailedExportLogsPath, snbUtils.AnalysisFileSpotfireLibraryPath);
                }
            }
            else
            {
                WriteToLogs(snbUtils.FailedExportLogsPath, snbUtils.AnalysisFileSpotfireLibraryPath );
            }
        }


        //private bool SaveAnalysisFile(AnalysisApplication application, string filePath, Utils snbUtils, int attempts, int maxAttempts, bool isSuccess)
        //{
        //    string error = null;
        //    if (!isSuccess && attempts < maxAttempts)
        //    {
        //        attempts++;
        //        try
        //        {
        //            CustomNodes nodes = application.Document.CustomNodes;
        //            Type type = nodes.GetType();
        //            MethodInfo removeMethod = type.GetMethod("RemoveStandIns", BindingFlags.NonPublic | BindingFlags.Instance);
        //            removeMethod.Invoke(nodes, null);
        //            application.SaveAs(filePath, new DocumentSaveSettings());
        //            isSuccess = true;
        //            return true;
        //        }
        //        catch (SerializationException se)
        //        {
        //            SaveAnalysisFile(application, filePath, snbUtils, attempts, maxAttempts, isSuccess);
        //            error = se.ToString();
        //        }
        //        catch (NullReferenceException nex)
        //        {
        //            try
        //            {
        //                application.SaveAs(filePath, new DocumentSaveSettings());
        //            }
        //            catch (Exception se)
        //            {
        //                error = nex.ToString();
        //                return false;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            try
        //            {
        //                application.SaveAs(filePath, new DocumentSaveSettings());
        //            }
        //            catch (Exception se)
        //            {
        //                error = se.ToString();
        //                return false;
        //            }
        //        }
        //    }
        //    WriteToLogs(snbUtils.ErrorLogsPath, error);
        //    return false;
        //}



        //private void ProcessAnalysisFile(LibraryItem analysis, DocumentOpenSettings openSettings, AnalysisApplication application, Utils snbUtils, string filePath)
        //{
        //    bool isSuccess = false;
        //    int maxAttempts = 5;
        //    int attempts = 0;

        //    try
        //    {
        //        application.Open(analysis, openSettings);
        //    }
        //    catch (InvalidOperationException ioex)
        //    {
        //        WriteToLogs(snbUtils.ErrorLogsPath, $"{ioex.GetType()} while opening the file {snbUtils.AnalysisFileSpotfireLibraryPath}:\n{ioex.ToString().Substring(0, 500)}");
        //        WriteToLogs(snbUtils.ErrorLogsPath, $"attempting to save the file {snbUtils.AnalysisFileSpotfireLibraryPath} with errors");

        //    }
        //    catch (NullReferenceException nullEx)
        //    {
        //        WriteToLogs(snbUtils.ErrorLogsPath, $"{nullEx.GetType()} while opening the file {snbUtils.AnalysisFileSpotfireLibraryPath}:\n{nullEx.ToString().Substring(0, 500)}");
        //        WriteToLogs(snbUtils.ErrorLogsPath, $"attempting to save the file {snbUtils.AnalysisFileSpotfireLibraryPath} with errors");
        //    }
        //    catch (Exception ex)
        //    {

        //        WriteToLogs(snbUtils.ErrorLogsPath, $"{ex.GetType()} while opening the file {snbUtils.AnalysisFileSpotfireLibraryPath}:\n{ex.ToString().Substring(0, 500)}");
        //        WriteToLogs(snbUtils.ErrorLogsPath, $"attempting to save the file {snbUtils.AnalysisFileSpotfireLibraryPath} with errors");

        //    }

        //    if (application.Document != null)
        //    {
        //        bool isSaved = SaveAnalysisFile(application, filePath, snbUtils, attempts, maxAttempts, isSuccess);

        //        if (File.Exists(filePath) || isSaved)
        //        {
        //            ExportToSnb(snbUtils, filePath);
        //        }
        //        else
        //        {
        //            WriteToLogs(snbUtils.ErrorLogsPath, $"failed to export {snbUtils.AnalysisFileSpotfireLibraryPath}");
        //            WriteToLogs(snbUtils.FailedExportLogsPath, snbUtils.AnalysisFileSpotfireLibraryPath);
        //        }
        //    }
        //}
    }
    #endregion
}

    

