using System;
using System.Collections;
using System.Data;
using System.Web;
using System.Linq;
using System.Xml.XPath;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;

using umbraco;
using umbraco.DataLayer;
using System.Reflection;
using Lecoati.uMirror.Pocos;
using Lecoati.uMirror.Bll;
using umbraco.BusinessLogic;
using Umbraco.Core.Logging;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Models;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using umbraco.cms.businesslogic.media;

namespace Lecoati.uMirror.Core
{

    public static class Util
    {

        public enum LogType
        {
            warm,
            error,
            info
        }

        public static void UpdateStateAndLogs(String projectName, LogType type, String message, bool addToLog)
        {
            UpdateStateAndLogs(projectName, type, message, false, null, addToLog);
        }

        public static void UpdateStateAndLogs(String projectName, LogType type, String message, bool isCompleted, Exception exception, bool addToLog)
        {

            Synchronizer.appState = message;

            string completed = string.Empty;
            string result = string.Empty;
            string exMessage = string.Empty;

            if (isCompleted)
            {
                completed = "[completed]";
                result = " / skipped:" + Synchronizer.appNumSki.ToString() +
                         " Updated:" + Synchronizer.appNumUpd.ToString() +
                         " Added:" + Synchronizer.appNumAdd.ToString() +
                         " Deleted:" + Synchronizer.appNumDel.ToString() +
                         " Error:" + Synchronizer.appNumErr.ToString() + " ";
            }

            if (exception != null) exMessage = " " + Util.getExceptionnMessage(exception);

            if (addToLog)
            {
                switch (type)
                {
                    case LogType.info:
                        LogHelper.Info(MethodBase.GetCurrentMethod().DeclaringType, String.Format("[uMirror][{0}]{1} {2}{3}{4}", projectName, completed, message, result, exMessage));
                        break;

                    case LogType.error:
                        LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, String.Format("[uMirror][{0}]{1} {2}{3}{4}", projectName, completed, message, result, exMessage), exception);
                        break;

                    case LogType.warm:
                        LogHelper.Warn(MethodBase.GetCurrentMethod().DeclaringType, String.Format("[uMirror][{0}]{1} {2}{3}{4}", projectName, completed, message, result, exMessage));
                        break;
                }
            }

        }

        public static string getExceptionnMessage(Exception ex)
        {
            string Message = "";
            if (ex.Message != null) { Message = ex.Message; }
            if (ex.InnerException != null && ex.InnerException.Message != null)
            {
                Message = Message + " " + ex.InnerException.Message;
                if (ex.InnerException.InnerException != null && ex.InnerException.InnerException.Message != null)
                { Message = Message + " " + ex.InnerException.InnerException.Message; }
            }
            return Message;
        }

        public static string truncStr(string str, int num)
        {
            if (num > 0 && str.Length > num)
            {
                return str.Substring(0, num - 1);
            }
            else
            {
                return str;
            }
        }

        public static int SaveMedia(String imagePath, int mediaParent, Umbraco.Core.Models.Media media = null)
        {

            IMediaService ms = ApplicationContext.Current.Services.MediaService;

            if (mediaParent == 0) mediaParent = -1;
            if (mediaParent > -1 && ms.GetById(mediaParent) == null)
                throw new Exception("Media parent folder not found");

            string fileName = Path.GetFileName(imagePath);
            string fileExtension = Path.GetExtension(imagePath).Replace(".", "");
            string fileWithoutExtension = fileName.Replace(Path.GetExtension(imagePath), "");

            Umbraco.Core.Models.Media existFile = (Umbraco.Core.Models.Media)ms.GetChildren(mediaParent).FirstOrDefault(r => r.HasProperty("umbracoFile")
                && Path.GetFileName(r.GetValue("umbracoFile").ToString()).ToLower() == fileName.ToLower().Replace(" ", "-")
                && FileCompare(HttpContext.Current.Server.MapPath(r.GetValue("umbracoFile").ToString()), imagePath));

            if (existFile != null) 
                return existFile.Id;

            var mediaType = Constants.Conventions.MediaTypes.File;
            if (Umbraco.Core.Configuration.UmbracoConfig.For.UmbracoSettings().Content.ImageFileTypes.Contains(fileExtension))
                mediaType = Constants.Conventions.MediaTypes.Image;

            if (media == null)
            {
                media = (Umbraco.Core.Models.Media)ms.CreateMedia(fileWithoutExtension, mediaParent, mediaType);
            }
            else
            {
                if (media.GetValue("umbracoFile") != null && !string.IsNullOrEmpty(media.GetValue("umbracoFile").ToString()))
                {
                    String filePath = HttpContext.Current.Server.MapPath(media.GetValue("umbracoFile").ToString());
                    System.IO.Directory.Delete(filePath.Substring(0, filePath.LastIndexOf("\\")), true);
                }
            }

            try
            {
                using (var fstr = System.IO.File.OpenRead(imagePath))
                {
                    media.SetValue(Constants.Conventions.Media.File, fileName, fstr);
                }
            }
            catch
            {
                throw new Exception("File not found");
            }

            ms.Save(media);
            return media.Id;

        }

        public static bool FileCompare(string file1, string file2)
        {
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;

            if (!System.IO.File.Exists(file1) || !System.IO.File.Exists(file2))
                return false;

            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // Open the two files.
            fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read);
            fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read);

            // Check the file sizes. If they are not the same, the files 
            // are not the same.
            if (fs1.Length != fs2.Length)
            {
                // Close the file
                fs1.Close();
                fs2.Close();

                // Return false to indicate files are different
                return false;
            }

            // Read and compare a byte from each file until either a
            // non-matching set of bytes is found or until the end of
            // file1 is reached.
            do
            {
                // Read one byte from each file.
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1));

            // Close the files.
            fs1.Close();
            fs2.Close();

            // Return the success of the comparison. "file1byte" is 
            // equal to "file2byte" at this point only if the files are 
            // the same.
            return ((file1byte - file2byte) == 0);
        }

        public static int GetMediaParentId(string mediaFolderPath)
        {
             
            IMediaService ms = ApplicationContext.Current.Services.MediaService;

            int parentFolderId = -1;
            if (!string.IsNullOrEmpty(mediaFolderPath))
            {
                foreach (string indexFolder in mediaFolderPath.Replace(@"\\", @"/").Replace(@"\", @"/").Split('/'))
                {
                    IMedia folder = null;
                    if (parentFolderId == -1)
                        folder = ms.GetRootMedia().FirstOrDefault(r => r.Name.ToLower() == indexFolder.ToLower() && !r.Path.Contains("-21") && !r.Trashed);
                    else
                        folder = ms.GetChildren(parentFolderId).FirstOrDefault(r => r.Name.ToLower() == indexFolder.ToLower() && !r.Path.Contains("-21") && !r.Trashed);

                    if (folder == null)
                    {
                        if (parentFolderId == -1)
                            folder = ms.CreateMedia(indexFolder, -1, "folder");
                        else
                            folder = ms.CreateMedia(indexFolder, ms.GetById(parentFolderId), "folder");
                        ms.Save(folder);
                    }
                    parentFolderId = folder.Id;
                }
            }
            return parentFolderId;

        }

        public static int GetDtIdByValue(int DataTypeDefinitionId, string value)
        {
            IDataTypeService ds = ApplicationContext.Current.Services.DataTypeService;

            PreValueCollection pvc = ds.GetPreValuesCollectionByDataTypeId(DataTypeDefinitionId);

            if (!pvc.IsDictionaryBased)
            {
                var pvaa = pvc.PreValuesAsArray;
                if (pvaa.Any())
                    return pvaa.Where(v => v.Value == value).FirstOrDefault().Id;
                else
                    return -1;
            }
            else
            {
                var pvaa = pvc.PreValuesAsDictionary;
                if (pvaa.Any())
                    return pvaa.Where(v => v.Value.Value == value).FirstOrDefault().Value.Id;
                else
                    return -1;
            }


        }

    }
}