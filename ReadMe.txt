Spotfire Automation Task to migrate DXP files from Spotfire Library to Signals Notebook Experiments.

Pre-requisites:

   - Access to Signals Notebook (SNB)
   - Access to Spotfire
   - Create a Notebook in SNB and copy the Notebook eid.
   - Create a Experiment template in SNB and add the fields that will be updated by this tool. Copy the template eid.
   - Create SNB API key and copy the key value.
   - Create a Spotfire RESTful API user using following command
	      config register-api-client --name apiuser -Sapi.library.read
   - Note the Client ID and Client secret values returned by above command
   - Create a folder for tool logs on machine where this tool will be running (Local or Automation server).

Data FLow:

1. Using Spotfire API, Query Spotfire Library for Analysis files.

For each item in the result from step 1:
2. Compile metadata from the DXP file.
3. Create a SNB experiment:
4. Query Spotfire RESTful API to fetch DXP as byte[] array.
5. Query Spotfire RESTful API to fetch metadata on DXP not available through Spotfire API.
6. Use DXP metadata from step 3 and step 5 to update fields on SNB experiment.
7. Attach DXP file to the SNB experiment

Tool logs:
The Tool outputs following logs:
1. successful_exports_file_logs: documenting the files that were successfully exported.
2. export_plugin_logs: documenting logs from the plugin run.
3. failed_export_files_logs: documenting the files that failed export.
4. error_plugin_logs: documenting errors during plugin run including failed exports.