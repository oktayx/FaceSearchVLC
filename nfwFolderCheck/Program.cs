using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace nfwFolderCheck
{
    class Program
    {
        static FileSystemWatcher watcher;
        static string SnapshotFolder;
        //Required to send captured photo & get face recognition result
        static string apiAddress = "http://127.0.0.1:5555/api/face/SearchFace?treshold=45";
        static ConsoleToast.Program toast;

        static void Main(string[] args)
        {
            //Customize snapshot folder to watch. Default is VLC snapshot folder
            SnapshotFolder = GetVLCCaptureFolder();
            if(SnapshotFolder == "")
            {
                Console.WriteLine("Watch folder not found");
                Environment.Exit(0);
            }

            //Initiate notification module
            toast = new ConsoleToast.Program();

            //Start to watch folder to get captured images 
            WatchFolder();

            new System.Threading.AutoResetEvent(false).WaitOne();

        }

        public static void WatchFolder()
        {
            watcher = new FileSystemWatcher();
            watcher.Path = SnapshotFolder;
            watcher.NotifyFilter = NotifyFilters.Attributes |
                                    NotifyFilters.CreationTime |
                                    NotifyFilters.FileName |
                                    NotifyFilters.LastAccess |
                                    NotifyFilters.LastWrite |
                                    NotifyFilters.Size |
                                    NotifyFilters.Security;
            watcher.Filter = "*.*";
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }

        public static void OnChanged(object source, FileSystemEventArgs e)
        {
            string json = UploadFile(e.FullPath);
            Console.WriteLine(json);

            //My json format: {"AD":"Türkan Soray","LISTE":"Sinema","SKOR":0.475338876}
            //Customize return data properties from face service
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            string title = data.AD==null ? "Person not found!" : data.AD;
            string message = data.LISTE==null ? "" : data.LISTE;

            toast.ShowImageToastM(
                    "FaceSearch.App",
                    title,
                    message,
                    e.FullPath);
        }

        public static string UploadFile(string filePath)
        {
            FileInfo file = new FileInfo(filePath);

            while (IsFileLocked(file))
                System.Threading.Thread.Sleep(500);

            using (var wc = new WebClient())
            {
                var taskResult = wc.UploadFileTaskAsync(apiAddress, filePath);
                taskResult.Wait();

                return System.Text.Encoding.UTF8.GetString(taskResult.Result);
            }
        }

        protected static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public static string GetVLCCaptureFolder()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string vlcConfigFile = Path.Combine(appDataFolder, "vlc", "vlcrc");
            string folder = "";
            try
            {
                folder = File.ReadAllLines(vlcConfigFile).Where(l => l.StartsWith("snapshot-path")).Select(f => f.Split('=').ElementAt(1)).Single();
            }
            catch (Exception)
            {
                Console.WriteLine("VLC config not found");
            }

            return folder;
        }
    }

}
