using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SpotfireLibraryToSNBExport
{
    internal class Utils
    {
        private string snbUrl;
        private string snbApiKey;
        private string snbExperimentTemplateId;
        private string snbNotebookTemplateId;
        private string existingDxpAttachmentId;
        private string requestHeaders;
        private string newExperimentName;
        private string newExperimentDescription;
        private string analysisFileCreateTime;
        private string analysisFileCreatedBy;
        private string analysisFileModifiedTime;
        private string analysisFileModifiedBy;
        private string analysisFileSpotfireLibraryPath;
        private string analysisFileId;
        private string requestBody;
        private string apiResponse;
        private string errorLogsPath;
        private string pluginLogsPath;
        private string failedExportLogsPath;
        private string successfulExportsLogsPath;
        private string project;
        private string department;
        private string SpotfireApiUserClientId;
        private string spotfireApiUserClientSecret;
        private string spotfireServerUrl;
        private string SpotfireAccessToken;

        public Utils()
        {
        }

        public string SnbUrl { get => snbUrl; set => snbUrl = value; }

        public string SnbApiKey { get => snbApiKey; set => snbApiKey = value; }

        public string SnbExperimentTemplateId { get => snbExperimentTemplateId; set => snbExperimentTemplateId = value; }

        public string SnbNotebookTemplateId { get => snbNotebookTemplateId; set => snbNotebookTemplateId = value; }

        public string RequestHeaders { get => requestHeaders; set => requestHeaders = value; }
        public string NewExperimentName { get => newExperimentName; set => newExperimentName = value; }
        public string NewExperimentDescription { get => newExperimentDescription; set => newExperimentDescription = value; }
        public string AnalysisFileCreateTime { get => analysisFileCreateTime; set => analysisFileCreateTime = value; }
        public string AnalysisFileModifiedTime { get => analysisFileModifiedTime; set => analysisFileModifiedTime = value; }
        public string AnalysisFileSpotfireLibraryPath { get => analysisFileSpotfireLibraryPath; set => analysisFileSpotfireLibraryPath = value; }

        public string ApiResponse { get => apiResponse; set => apiResponse = value; }
        public string RequestBody { get => requestBody; set => requestBody = value; }
        public string ExistingDxpAttachmentId { get => existingDxpAttachmentId; set => existingDxpAttachmentId = value; }
        public string ErrorLogsPath { get => errorLogsPath; set => errorLogsPath = value; }
        public string PluginLogsPath { get => pluginLogsPath; set => pluginLogsPath = value; }
        public string FailedExportLogsPath { get => failedExportLogsPath; set => failedExportLogsPath = value; }
        public string SuccessfulExportsLogsPath { get => successfulExportsLogsPath; set => successfulExportsLogsPath = value; }
        public string Project { get => project; set => project = value; }
        public string Department { get => department; set => department = value; }
        public string SpotfireApiUserClientSecret { get => spotfireApiUserClientSecret; set => spotfireApiUserClientSecret = value; }
        public string SpotfireServerUrl { get => spotfireServerUrl; set => spotfireServerUrl = value; }
        public string AnalysisFileId { get => analysisFileId; set => analysisFileId = value; }
        public string SpotfireApiUserClientId1 { get => SpotfireApiUserClientId; set => SpotfireApiUserClientId = value; }
        public string SpotfireAccessToken1 { get => SpotfireAccessToken; set => SpotfireAccessToken = value; }
        public string AnalysisFileCreatedBy { get => analysisFileCreatedBy; set => analysisFileCreatedBy = value; }
        public string AnalysisFileModifiedBy { get => analysisFileModifiedBy; set => analysisFileModifiedBy = value; }

        public string CreateExperimentRequestBody()
        {
            try
            {
                var jsonData = new
                {
                    data = new
                    {
                        type = "experiment",
                        attributes = new
                        {
                            name = NewExperimentName
                        },
                        relationships = new
                        {
                            ancestors = new
                            {
                                data = new[]
                {
                    new
                    {
                        type = "journal",
                        id = SnbNotebookTemplateId,
                    }
                }
                            },
                            template = new
                            {
                                data = new
                                {
                                    type = "experiment",
                                    id = SnbExperimentTemplateId,
                                }
                            }
                        }
                    }
                };

                return System.Text.Json.JsonSerializer.Serialize(jsonData);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }


        public string SendCreateSnbExperimentRequest()
        {
            try
            {
                string url = SnbUrl.EndsWith("/") ? $"{SnbUrl}api/rest/v1.0/entities?force=true" : $"{SnbUrl}/api/rest/v1.0/entities?force=true";

                using (HttpClient client = new HttpClient())
                {
                    string requestBody = CreateExperimentRequestBody();

                    // Create StringContent with the JSON data
                    var content = new StringContent(requestBody, Encoding.UTF8, "application/vnd.api+json");


                    client.DefaultRequestHeaders.Add("x-api-key", SnbApiKey);
                    client.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");

                    HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        var responseData = JsonSerializer.Deserialize<JsonObject>(responseContent);

                        var data = responseData["data"] as JsonObject;
                        var attributes = data["attributes"] as JsonObject;
                        string eid = attributes?["eid"]?.ToString();
                        string digest = attributes?["digest"]?.ToString();
                        string existingAttachmentId = getExistingDxpAttachmentId(eid);

                        WriteToLogs(PluginLogsPath, $"created experiment: {eid}, analysis file path: {AnalysisFileSpotfireLibraryPath}");
                        string propertiesUrl = SnbUrl.EndsWith("/") ? $"{SnbUrl}api/rest/v1.0/entities/{eid}/properties?digest={digest}" : $"{SnbUrl}/api/rest/v1.0/entities/{eid}/properties?digest={digest}";
                        updateExperimentProperties(propertiesUrl, eid, digest);
                        return $"{eid},{ExistingDxpAttachmentId}";
                    }
                    else
                    {
                        WriteToLogs(ErrorLogsPath, "Request failed with status code: " + response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    }

                }

            }
            catch (Exception ex)
            {
                WriteToLogs(ErrorLogsPath, ex.ToString());
            }
            return null;
        }


        public string getExistingDxpAttachmentId(string experimentId)
        {
            try
            {
                string url = SnbUrl.EndsWith("/") ? $"{SnbUrl}api/rest/v1.0/entities/{experimentId}" : $"{SnbUrl}/api/rest/v1.0/entities/{experimentId}";

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-api-key", SnbApiKey);
                    client.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");

                    HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        var responseData = JsonSerializer.Deserialize<JsonObject>(responseContent);

                        var data = responseData["data"] as JsonObject;
                        var relationships = data?["relationships"] as JsonObject;
                        var children = relationships?["children"] as JsonObject;
                        var childData = children?["data"].AsArray();
                        foreach (var child in childData)
                        {
                            string id = child["id"]?.ToString();
                            if (id.StartsWith("signals_spotfiredxp:"))
                            {
                                ExistingDxpAttachmentId = id;
                                return ExistingDxpAttachmentId;
                            }
                        }

                    }
                    else
                    {
                        WriteToLogs(ErrorLogsPath, "Request failed with status code: " + response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    }

                }
            }
            catch (Exception ex)
            {
                WriteToLogs(ErrorLogsPath, ex.ToString());
            }
            return null;
        }

        public void updateExperimentProperties(string url, string experimentId, string digest)
        {
            try
            {
                var jsonData = new
                {
                    data = new[]
                    {
                        new { attributes = new {
                            name = "Analysis File Created By",
                            value = AnalysisFileCreatedBy
                            }
                        },
                        new { attributes = new {
                            name = "Analysis File Created Time",
                            value = AnalysisFileCreateTime
                            }
                        },

                        new { attributes = new {
                            name = "Analysis File Modified By",
                            value = AnalysisFileModifiedBy
                            }
                        },
                        new { attributes = new {
                            name = "Analysis File Modified Time",
                            value = AnalysisFileModifiedTime
                            }
                        },
                        new { attributes = new {
                            name = "Description",
                            value = NewExperimentDescription
                            }
                        },
                        new { attributes = new {
                            name = "Spotfire Library Path",
                            value = AnalysisFileSpotfireLibraryPath
                            }
                        },
                        new { attributes = new {
                            name = "Project",
                            value = Project
                            }
                        },
                        new { attributes = new {
                            name = "Department",
                            value = Department
                            }
                        },
                        new { attributes = new {
                            name = "File Description",
                            value = NewExperimentDescription
                            }
                        },
                    }
                };


                // Update properties
                string updatedData = System.Text.Json.JsonSerializer.Serialize(jsonData);

                // Create StringContent with the JSON data
                //var content = new StringContent(updatedData, Encoding.UTF8, "application/vnd.api+json");

                bool updatedValues = SendUpdatePropertiesRequest(url, updatedData);
                if (updatedValues)
                {
                    WriteToLogs(PluginLogsPath, $"updated properties for file: {AnalysisFileSpotfireLibraryPath}, {experimentId}");
                }
                else
                {
                    WriteToLogs(ErrorLogsPath, $"failed to update properties for file: {AnalysisFileSpotfireLibraryPath}, {experimentId}");
                }

            }
            catch (Exception ex)
            {
                WriteToLogs(ErrorLogsPath, ex.ToString());
            }
        }


        public bool SendUpdatePropertiesRequest(string url, string requestBody)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Create StringContent with the JSON data
                    var content = new StringContent(requestBody, Encoding.UTF8, "application/vnd.api+json");


                    client.DefaultRequestHeaders.Add("x-api-key", SnbApiKey);
                    client.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");

                    // Create the HttpRequestMessage for PATCH
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                    {
                        Content = content
                    };

                    HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        return true;
                    }
                    else
                    {
                        WriteToLogs(ErrorLogsPath, "Request failed with status code: " + response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        return false;
                    }

                }
            }
            catch (Exception ex)
            {
                WriteToLogs(ErrorLogsPath, ex.ToString());
            }

            return false;
        }


       
        public bool AddDxpAsAttachment(byte[] libraryItem)
        {
            string url = SnbUrl.EndsWith("/") ? $"{SnbUrl}/api/rest/v1.0/entities/{existingDxpAttachmentId}/attachment?force=true" : $"{SnbUrl}/api/rest/v1.0/entities/{existingDxpAttachmentId}/attachment?force=true";
            //string url = $"{SnbUrl}/api/rest/v1.0/entities/{experimentId}/children/{NewExperimentName}";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Read the file into a byte array
                    //byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    byte[] content = GetLibraryItem();

                    if (content == null)
                    {
                        return false;
                    }
                    // Create the ByteArrayContent
                    using (ByteArrayContent byteContent = new ByteArrayContent(content))
                    {
                        // Set the content type header
                        client.DefaultRequestHeaders.Add("x-api-key", SnbApiKey);
                        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.spotfire.dxp");

                        // Send the POST request
                        HttpResponseMessage response = client.PutAsync(url, byteContent).GetAwaiter().GetResult();

                        // Check the response status
                        if (response.IsSuccessStatusCode)
                        {
                            WriteToLogs(PluginLogsPath, $"Analysis file successfully added to SNB experiment, file name {newExperimentName}.dxp");
                            return true;
                        }
                        else
                        {
                            string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            WriteToLogs(ErrorLogsPath, $"error while adding Analysis file to SNB experiment {""}, file name {newExperimentName}.dxp ");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogs(ErrorLogsPath, ex.ToString());
                return false;
            }
        }



        public void WriteToLogs(string path, string message)
        {
            using (StreamWriter sw = System.IO.File.AppendText(path))
            {
                sw.WriteLine(message);
            }
        }

        public JsonObject GetSpotfireAccessToken()
        {
            string url = SpotfireServerUrl.EndsWith("/") ? $"{SpotfireServerUrl}spotfire/oauth2/token" : $"{SpotfireServerUrl}/spotfire/oauth2/token";
            string base64AuthCode = string.Empty;
            if (!string.IsNullOrWhiteSpace(SpotfireApiUserClientId1) && !string.IsNullOrWhiteSpace(SpotfireApiUserClientSecret))
            {
                string value = $"{SpotfireApiUserClientId1}:{spotfireApiUserClientSecret}";
                // Convert string to byte array
                byte[] byteArray = Encoding.UTF8.GetBytes(value);

                // Convert byte array to Base64 string
                string base64String = Convert.ToBase64String(byteArray);
                base64AuthCode = $"Basic {base64String}";
            }

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                client.DefaultRequestHeaders.Add("Authorization", base64AuthCode);
                //client.DefaultRequestHeaders.Add("Cookie", "JSESSIONID=2B3D72342DDADA2B925034E09EFA7C9E.ec2amaz-4l5o108-srv; XSRF-TOKEN=d0679e869987328e9b31efda0df6c9e6");
                var content = new StringContent("scope=api.library.read&grant_type=client_credentials", null, "application/x-www-form-urlencoded");
                request.Content = content;
                var response = client.SendAsync(request).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var responseData = JsonSerializer.Deserialize<JsonObject>(responseContent);
                    return responseData;
                }
                else
                {
                    WriteToLogs(pluginLogsPath, $"failed to get Spotfire API access token: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                }
                return null;
            }
        }


        public bool IsTokenExpired(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return true;
            }
            var handler = new JwtSecurityTokenHandler();
            token = token.Replace("Bearer", "");
            var jwtToken = handler.ReadJwtToken(token);
            DateTimeOffset exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwtToken.Claims.First(c => c.Type == "exp").Value));

            return exp < DateTimeOffset.UtcNow;
        }

        public byte[] GetLibraryItem()
        {
            byte[] fileBytes = null;
            string url = SpotfireServerUrl.EndsWith("/") ? $"{SpotfireServerUrl}spotfire/api/rest/library/v2/items/{AnalysisFileId}/contents" : $"{SpotfireServerUrl}/spotfire/api/rest/library/v2/items/{AnalysisFileId}/contents";

            try
            {
                using (var client = new HttpClient())
                {
                    //var request = new HttpRequestMessage(HttpMethod.Get, url);
                    client.DefaultRequestHeaders.Add("accept", "application/octet-stream");
                    client.DefaultRequestHeaders.Add("Authorization", SpotfireAccessToken1);
                    //client.DefaultRequestHeaders.Add("Cookie", "JSESSIONID=2B3D72342DDADA2B925034E09EFA7C9E.ec2amaz-4l5o108-srv; XSRF-TOKEN=d0679e869987328e9b31efda0df6c9e6");
                    var response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        return response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                    }
                    else
                    {
                        WriteToLogs(pluginLogsPath, $"failed to download file {AnalysisFileId} from Spotfire library: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                    }
                }
                return fileBytes;
            }
            catch (Exception ex)
            {
                WriteToLogs(pluginLogsPath, $"failed to download file {AnalysisFileId} from Spotfire library: {ex.ToString()}");
                return null;
            }
        }

        public JsonObject GetLibraryItemInfo(string libraryItemId)
        {
            string url = SpotfireServerUrl.EndsWith("/") ? $"{SpotfireServerUrl}spotfire/api/rest/library/v2/items/{AnalysisFileId}" : $"{SpotfireServerUrl}/spotfire/api/rest/library/v2/items/{AnalysisFileId}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                client.DefaultRequestHeaders.Add("accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", SpotfireAccessToken1);
                //client.DefaultRequestHeaders.Add("Cookie", "JSESSIONID=2B3D72342DDADA2B925034E09EFA7C9E.ec2amaz-4l5o108-srv; XSRF-TOKEN=d0679e869987328e9b31efda0df6c9e6");
                var response = client.SendAsync(request).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var responseData = JsonSerializer.Deserialize<JsonObject>(responseContent);
                    return responseData;
                }
                else
                {
                    WriteToLogs(pluginLogsPath, $"failed to get Library Item {libraryItemId} info: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                    return null;
                }

            }
        }


        //public void deleteSavedAnalysisFile(string filePath)
        //{
        //    try
        //    {
        //        // Check if the file exists
        //        if (File.Exists(filePath))
        //        {
        //            // Delete the file
        //            File.Delete(filePath);
        //            Console.WriteLine("File deleted successfully.");
        //        }
        //        else
        //        {
        //            Console.WriteLine("File not found.");
        //        }
        //    }
        //    catch (Exception ex)

        //    {
        //        WriteToLogs(ErrorLogsPath, $"failed to delete locally saved Analysis File: {filePath}");
        //        WriteToLogs(ErrorLogsPath, ex.ToString());
        //    }
        //}




        //public bool AddDxpAsAttachment(string existingDxpAttachmentId, string filePath)
        //{
        //    string url = SnbUrl.EndsWith("/") ? $"{SnbUrl}/api/rest/v1.0/entities/{existingDxpAttachmentId}/attachment?force=true" : $"{SnbUrl}/api/rest/v1.0/entities/{existingDxpAttachmentId}/attachment?force=true";
        //    //string url = $"{SnbUrl}/api/rest/v1.0/entities/{experimentId}/children/{NewExperimentName}";
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            // Read the file into a byte array
        //            //byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
        //            byte[] content = GetLibraryItem();

        //            if (content == null)
        //            {
        //                return false;
        //            }
        //            // Create the ByteArrayContent
        //            using (ByteArrayContent byteContent = new ByteArrayContent(content))
        //            {
        //                // Set the content type header
        //                client.DefaultRequestHeaders.Add("x-api-key", SnbApiKey);
        //                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.spotfire.dxp");

        //                // Send the POST request
        //                HttpResponseMessage response = client.PutAsync(url, byteContent).GetAwaiter().GetResult();

        //                // Check the response status
        //                if (response.IsSuccessStatusCode)
        //                {
        //                    WriteToLogs(PluginLogsPath, $"Analysis file successfully added to SNB experiment, file name {newExperimentName}.dxp");
        //                    return true;
        //                }
        //                else
        //                {
        //                    string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        //                    WriteToLogs(ErrorLogsPath, $"error while adding Analysis file to SNB experiment {""}, file name {newExperimentName}.dxp ");
        //                    return false;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteToLogs(ErrorLogsPath, ex.ToString());
        //        return false;
        //    }
        //}

    }
}
