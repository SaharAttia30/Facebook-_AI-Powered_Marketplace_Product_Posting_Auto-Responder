using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using ChatBlaster.Models;

namespace ChatBlaster.Utilities
{
    public class StaticHelpers
    {
         [Flags]
        public enum AssocF
        {
            InitNoRemapClsid = 0x1,
            InitByExeName = 0x2,
            OpenByExeName = 0x2,
            InitDefaultToStar = 0x4,
            InitDefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200
        }
        public enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DdeCommand,
            DdeIfExec,
            DdeApplication,
            DdeTopic
        }
        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, ref uint pcchOut);
        public static Dictionary<string,string> CreateAvatarFolders(string avatar_id)
        {
            Dictionary<string, string> dictionary_path = new();
            string path = Directory.GetCurrentDirectory();
            string chrome_path = Path.Combine(path, "chrome-win64", "chrome.exe");
            string avatar_path = Path.Combine(path, "user_data", avatar_id);
            string user_path = Path.Combine(avatar_path, "MyProfiles");
            var photoDirectory = Path.Combine(avatar_path, "PhotosForPosts");
            dictionary_path.Add("_userPath", user_path);
            dictionary_path.Add("_chromePath", chrome_path);
            dictionary_path.Add("_photoDirectory", photoDirectory);
            dictionary_path.Add("_avatarFolderPath", avatar_path);
            List<string> folderNames = new List<string>
                        {
                            "user_data",
                            avatar_path,
                            user_path,
                            photoDirectory
                        };
            CreateFolders(folderNames, path);
            return dictionary_path;
        }
        
        
        /***************************************************************************************************************************************************************/
        public static void LogException(Exception ex, string additional = "")
        {
            // Get detailed stack trace information
            var stackTrace = new StackTrace(ex, true); // true to capture file info
            var frame = stackTrace.GetFrame(0); // Get the top frame of the stack
            var fileName = frame?.GetFileName();
            var lineNumber = frame?.GetFileLineNumber();
            var methodName = frame?.GetMethod()?.Name;

            // Create log message
            string logMessage = $"Exception: {ex.Message}\n" +
                                $"Source: {ex.Source}\n" +
                                $"Method: {methodName}\n" +
                                $"File: {fileName}\n" +
                                $"Line: {lineNumber}\n" +
                                $"Stack Trace:\n{ex.StackTrace}";

            // Log to a file
            string logFilePath = "error_log.txt";
            File.AppendAllText(logFilePath, $"{DateTime.Now}: {logMessage}\n\n");

            // Optional: Print to console for immediate debugging
            Console.WriteLine(logMessage);
        }
        /***************************************************************************************************************************************************************/
        public static string GetAssociatedExecutablePath(string extension)
        {
            uint pcchOut = 0;
            AssocQueryString(AssocF.Verify, AssocStr.Executable, extension, null, null, ref pcchOut);
            StringBuilder pszOut = new StringBuilder((int)pcchOut);
            AssocQueryString(AssocF.Verify, AssocStr.Executable, extension, null, pszOut, ref pcchOut);
            return pszOut.ToString();
        }
        /***************************************************************************************************************************************************************/
        public static Dictionary<string, string> GetPaths(string id)
        {
            Dictionary<string, string> paths_dict = new Dictionary<string, string>();
            paths_dict["path"] = Directory.GetCurrentDirectory();
            paths_dict["chrome_path"] = Path.Combine(paths_dict["path"], "chrome-win64", "chrome.exe");
            paths_dict["avatar_path"] = Path.Combine(paths_dict["path"], "user_data", id);
            paths_dict["user_path"] = Path.Combine(paths_dict["path"], "user_data", id, "MyProfiles");
            paths_dict["bot_state_path"] = Path.Combine(paths_dict["avatar_path"], "botStates");
            paths_dict["bot_posts_path"] = Path.Combine(paths_dict["avatar_path"], "botPosts");
            paths_dict["randome_response_file_path"] = Path.Combine(paths_dict["avatar_path"], "responses.json");
            paths_dict["responses_path"] = Path.Combine(paths_dict["avatar_path"], "auto_responses.json");
            return paths_dict;
        }
        /***************************************************************************************************************************************************************/
        public static void CreateFolders(List<string> folderNames, string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                foreach (string name in folderNames)
                {
                    string fullPath = Path.Combine(path, name);
                    if (!Directory.Exists(fullPath))
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        /***************************************************************************************************************************************************************/
        public static void CopyAllFoldeContent(string target_folder_path, string destination_folder_path)
        {
            try
            {
                if (!Directory.Exists(target_folder_path))
                    return;
                Directory.CreateDirectory(destination_folder_path);
                foreach (var filePath in Directory.GetFiles(target_folder_path))
                {
                    var destFile = Path.Combine(destination_folder_path, Path.GetFileName(filePath));
                    File.Copy(filePath, destFile, overwrite: true);
                }
                foreach (var dirPath in Directory.GetDirectories(target_folder_path))
                {
                    var destDir = Path.Combine(destination_folder_path, Path.GetFileName(dirPath));
                    CopyAllFoldeContent(dirPath, destDir);
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***************************************************************************************************************************************************************/
        public static void CopyJsonFileIfNotExists(string jsonFilePath, string destinationFolderPath)
        {
            try
            {
                if (!File.Exists(jsonFilePath))
                {
                    return;
                }

                if (!Directory.Exists(destinationFolderPath))
                {
                    Directory.CreateDirectory(destinationFolderPath);
                }

                string fileName = Path.GetFileName(jsonFilePath);
                string destinationFilePath = Path.Combine(destinationFolderPath, fileName);

                if (!File.Exists(destinationFilePath))
                {
                    File.Copy(jsonFilePath, destinationFilePath);
                }
            }
            catch (Exception ex)
            {
            }
        }
/***************************************************************************************************************************************************************/
        public static DateTime ParseLastMessageTime(string timeStr)
        {
            if (string.IsNullOrWhiteSpace(timeStr))
                return DateTime.MaxValue;
            timeStr = timeStr.ToLower().Trim();
            var match = Regex.Match(timeStr, @"^(\d+)([mhdw])$");
            if (match.Success)
            {
                int value = int.Parse(match.Groups[1].Value);
                string unit = match.Groups[2].Value;
                switch (unit)
                {
                    case "m": return DateTime.Now - TimeSpan.FromMinutes(value);
                    case "h": return DateTime.Now - TimeSpan.FromHours(value);
                    case "d": return DateTime.Now - TimeSpan.FromDays(value);
                    case "w": return DateTime.Now - TimeSpan.FromDays(value * 7);
                    default: return DateTime.MaxValue;
                }
            }
            if (timeStr == "just now")
                return DateTime.Now;
            if (timeStr.EndsWith("minutes ago"))
            {
                int minutes = int.Parse(timeStr.Split(' ')[0]);
                return DateTime.Now - TimeSpan.FromMinutes(minutes);
            }
            if (timeStr.EndsWith("hours ago"))
            {
                int hours = int.Parse(timeStr.Split(' ')[0]);
                return DateTime.Now - TimeSpan.FromHours(hours);
            }
            if (timeStr == "yesterday")
                return DateTime.Now.Date - TimeSpan.FromDays(1);
            if (timeStr.EndsWith("days ago"))
            {
                int days = int.Parse(timeStr.Split(' ')[0]);
                return DateTime.Now - TimeSpan.FromDays(days);
            }
            return DateTime.MaxValue;
        }
/**********************************************************************************************************************************************/
        public static Process StartProcessLink(string chromePath, string userPath, string port, string base_url)
        {
            Random rand = new Random();
            int width = rand.Next(800, 1601); // Random width between 800 and 1600
            int height = rand.Next(800, 1001);
            ProcessStartInfo psi;
            psi = new ProcessStartInfo
            {
                FileName = chromePath,
                Arguments =
                    $"--remote-debugging-port={port} " +
                    $"--user-data-dir=\"{userPath}\" " +
                    $"--window-size={width},{height} " +
                    "--new-window " +
                    "--disable-blink-features=AutomationControlled " +
                    "--disable-infobars " +
                    "--disable-dev-shm-usage " +
                    "--no-default-browser-check " +
                    "--no-first-run " +
                    "--disable-extensions " +
                    "--disable-background-networking " +
                    "--disable-popup-blocking " +
                    "--disable-sync " +
                    "--disable-notifications " +
                    "--disable-translate " +
                    "--safebrowsing-disable-auto-update " +
                    "--disable-save-password-bubble " +
                    "--disable-password-generation " +
                    "--disable-autofill-keyboard-accessory-view " +
                    "--disable-single-click-autofill " +
                    "--disable-prompt-on-repost " +
                    $"\"{base_url}\"",
                UseShellExecute = true
            };
            return Process.Start(psi);
        }
        //public static Process StartProcessLink(string chromePath, string userPath, string port, string base_url)
        //{
        //    ProcessStartInfo psi;
        //    psi = new ProcessStartInfo
        //    {
        //        FileName = chromePath,
        //        Arguments = $"--remote-debugging-port={port} --user-data-dir={userPath} --start-maximized --new-window {base_url}",
        //        UseShellExecute = true
        //    };
        //    return Process.Start(psi);
        //}
/***************************************************************************************************************************************************************/
        public static bool IsContainingDisabled(Dictionary<string, string> attributes)
        {
            return attributes.ContainsKey("disabled");
        }
        public static bool VerifyContantListForClick(JObject response_data, out List<int> contant_list)
        {
            if (response_data.TryGetValue("result", out var result_value) &&
                result_value is JObject resultObject &&
                resultObject.TryGetValue("model", out var modelToken) &&
                modelToken is JObject modelObject &&
                modelObject.TryGetValue("content", out var contentToken) &&
                contentToken is JArray contentArray &&
                contentArray.Count > 5 &&
                CheckIfNumber(contentArray[0], out int content_0) &&
                CheckIfNumber(contentArray[1], out int content_1) &&
                CheckIfNumber(contentArray[2], out int content_2) &&
                CheckIfNumber(contentArray[5], out int content_5)
               )
            {
                contant_list = new List<int> { content_0, content_1, content_2, content_5 };
                return true;
            }
            contant_list = null;
            return false;
        }
        /***************************************************************************************************************************************************************/
        public static bool CheckIfNumber(object value, out int value_as_int)
        {
            value_as_int = -1;
            try
            {
                decimal res_float = value as decimal? ?? Convert.ToDecimal(value);
                value_as_int = decimal.ToInt32(res_float);
                return value != null;
            }
            catch (OverflowException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
