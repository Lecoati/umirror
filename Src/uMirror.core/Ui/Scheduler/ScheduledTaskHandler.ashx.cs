using System;
using System.Collections.Generic;
using System.Web;
using Lecoati.uMirror.Core;
using Lecoati.uMirror.DataStore;
using Umbraco.Core.Logging;
using System.Reflection;
using static Lecoati.uMirror.DataStore.Store;

namespace Lecoati.uMirror
{
    public class ScheduledTaskHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            IList<Project> synList = Store.GetAllProjects();

            DateTime currentDateTime = DateTime.Now; 

            foreach (Project project in synList)
            {

                Boolean start = false;

                if (project.Period != null)
                {

                    switch ((int)project.Period)
                    {
                        case (int)PeriodType.hourly:
                            start = currentDateTime.Minute == new DateTime(2010, 01, 01, (int)project.StartHour, (int)project.StartMinute, 0).Minute;
                            break;
                        case (int)PeriodType.daily:
                            start = currentDateTime.Minute == new DateTime(2010, 01, 01, (int)project.StartHour, (int)project.StartMinute, 0).Minute &
                                    currentDateTime.Hour == new DateTime(2010, 01, 01, (int)project.StartHour, (int)project.StartMinute, 0).Hour;
                            break;
                        case (int)PeriodType.weekly:
                            start = currentDateTime.Minute == new DateTime(2010, 01, 01, (int)project.StartHour, (int)project.StartMinute, 0).Minute &
                                    currentDateTime.Hour == new DateTime(2010, 01, 01, (int)project.StartHour, (int)project.StartMinute, 0).Hour &
                                    currentDateTime.DayOfWeek == GetDatOFWeek(project.Dayofweek);
                            break;
                        case (int)PeriodType.monthly:
                            start = currentDateTime.Minute == new DateTime(2010, 01, 01, (int)project.StartHour, (int)project.StartMinute, 0).Minute &
                                    currentDateTime.Hour == new DateTime(2010, 01, 01, (int)project.StartHour, (int)project.StartMinute, 0).Hour &
                                    currentDateTime.Day == int.Parse(project.Dayofmonth);
                            break;
                        case (int)PeriodType.none:
                            break;
                    }

                    if (start)
                    {
                        try
                        {
                            LogHelper.Info(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror - task] Start task " + DateTime.Now.ToShortTimeString() + " " + project.Name);
                            Synchronizer sync = new Synchronizer();
                            //Thread standardTCPServerThread = new Thread(sync.start);
                            sync.context = HttpContext.Current;
                            sync.projectid = project.id;
                            sync.currentUser = umbraco.BusinessLogic.User.GetCurrent();
                            sync.start();
                            //standardTCPServerThread.Start();
                            break;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror - task][error] " + DateTime.Now.ToShortTimeString() + " " + ex.Message, ex);
                        }
                    }
                }

            }

            LogHelper.Info(MethodBase.GetCurrentMethod().DeclaringType, "[synchronizer - task] task trigger " + DateTime.Now.ToShortTimeString());

            HttpContext.Current.Response.ContentType = "text/plain";
            HttpContext.Current.Response.Write("ok");

        }

        private DayOfWeek GetDatOFWeek(string day)
        {
            if (day == "monday")
                return DayOfWeek.Monday;
            else if (day == "tuesday")
                return DayOfWeek.Tuesday;
            else if (day == "wednesday")
                return DayOfWeek.Wednesday;
            else if (day == "thrusday")
                return DayOfWeek.Thursday;
            else if (day == "friday")
                return DayOfWeek.Friday;
            else if (day == "saturday")
                return DayOfWeek.Saturday;
            else 
                return DayOfWeek.Sunday;
        }

        public bool IsDayToStart(DateTime now, DateTime next)
        {
            return now.DayOfWeek == next.DayOfWeek;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

    }
}