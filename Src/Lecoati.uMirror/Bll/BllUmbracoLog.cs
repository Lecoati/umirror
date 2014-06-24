using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.presentation;
using Umbraco.Core;
using System.ComponentModel.DataAnnotations;
using Lecoati.uMirror.Pocos;
using umbraco.BusinessLogic;
using Umbraco.Core.Logging;
using System.Reflection;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Text;
using Lecoati.uMirror.Core;
using umbraco.cms.businesslogic.web;
using System.Text.RegularExpressions;

using System.Web.UI;
using System.Web.UI.WebControls;

namespace Lecoati.uMirror.Bll
{

    public class LogItem
    {
        public DateTime Date { get; set; }

        public string ThreadNo { get; set; }

        public string Level { get; set; }

        public string Logger { get; set; }

        public string ThreadId { get; set; }

        public string Message { get; set; }

        public string LoggerTruncated
        {
            get
            {
                if (this.Logger.Length > 40)
                    return this.Logger.Substring(0, 40) + "...";
                else
                    return this.Logger;
            }
        }

        public string LevelCssClass
        {
            get
            {
                return this.Level.ToLower();
            }
        }
    }

    public class BllUmbracoLog
    {

        public LogItem GetLastComleted(string projectAlias)
        {
            try
            {
                return GetAllLogItems(projectAlias, 10).Where(r => r.Logger.Contains("Lecoati.uMirror") && r.Message.Contains("[" + projectAlias + "][completed]")).OrderByDescending(r => r.Date).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                return null;
            }
        }

        public IList<LogItem> GetLogs(string projectAlias)
        {
            try
            {
                return GetAllLogItems(projectAlias, 100).Where(r => r.Logger.Contains("Lecoati.uMirror") && r.Message.Contains("[" + projectAlias + "]")).OrderByDescending(r => r.Date).ToList();
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                return null;
            }
        }

        private IEnumerable<KeyValuePair<DateTime, string>> GetLogFilesList()
        {
            string path = HttpContext.Current.Server.MapPath("~/App_Data/Logs/");
            Dictionary<DateTime, string> dictionary = new Dictionary<DateTime, string>();
            foreach (string input in Directory.GetFiles(path, "UmbracoTraceLog.*"))
            {
                DateTime dateTime = DateTime.Now;
                Match match = Regex.Match(input, ".txt.(\\d{4}-\\d{2}-\\d{2})");
                if (match.Success)
                    dateTime = DateTime.Parse(match.Groups[1].Value);
                dictionary.Add(dateTime.Date, input);
            }
            return (IEnumerable<KeyValuePair<DateTime, string>>)Enumerable.OrderByDescending<KeyValuePair<DateTime, string>, DateTime>((IEnumerable<KeyValuePair<DateTime, string>>)dictionary, (Func<KeyValuePair<DateTime, string>, DateTime>)(x => x.Key));
        }

        private List<LogItem> GetAllLogItems(string projectAlias, int max)
        {
            IEnumerable<KeyValuePair<DateTime, string>> fileList = GetLogFilesList();
            List<LogItem> results = new List<LogItem>();
            foreach (KeyValuePair<DateTime, string> index in fileList)
            {
                if (results.Count() >= max)
                {
                    break;
                }
                else
                {
                    max = max - results.Count();
                    results = results.Concat(GetLogItemsFromLogFile(projectAlias, index.Value, max)).ToList();
                }
            }
            return results;
        }

        private List<LogItem> GetLogItemsFromLogFile(string projectAlias, string logFile, int max)
        {
            string[] strArray = Regex.Split(File.ReadAllText(logFile), "(\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2},\\d{3} )");
            List<string> list1 = new List<string>();
            int index = 1;
            while (index < strArray.Length - 1)
            {
                list1.Add(strArray[index] + strArray[index + 1]);
                index += 2;
            }
            List<LogItem> list2 = new List<LogItem>();
            string pattern = "^.+(\\[\\d+\\]) (\\w+) {1,2}(.+) - (\\[Thread \\d+\\]) (.+)";
            foreach (string input in list1.OrderByDescending(r => list1.IndexOf(r)))
            {
                LogItem logItem = new LogItem();
                logItem.Date = DateTime.Parse(input.Substring(0, 19));
                logItem.Level = input;
                
                Match match = Regex.Match(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (match.Success)
                {
                    logItem.ThreadNo = match.Groups[1].Value;
                    logItem.Level = match.Groups[2].Value;
                    logItem.Logger = match.Groups[3].Value;
                    logItem.ThreadId = match.Groups[4].Value;
                    logItem.Message = match.Groups[5].Value;

                    if (logItem.Logger.Contains("Lecoati.uMirror") && logItem.Message.Contains("[" + projectAlias + "]"))
                        list2.Add(logItem);

                    if (list2.Count >= max) break;
                }
            }
            return list2;
        }

    }
}