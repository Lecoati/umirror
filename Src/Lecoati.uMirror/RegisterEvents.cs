using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.presentation;
using Umbraco.Core;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Persistence;
using Lecoati.uMirror.Pocos;

namespace Lecoati.uMirror
{
    public class RegisterEvents : ApplicationEventHandler
    {
        //This happens everytime the Umbraco Application starts
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {

            /// TODO: resolve nullable column issue
            ////Get the Umbraco Database context
            //var db = applicationContext.DatabaseContext.Database;

            ////Check if the DB table does NOT exist
            //if (!db.TableExist("Project"))
            //{
            //    db.CreateTable<Project>(false);
            //}

            //if (!db.TableExist("Node"))
            //{
            //    db.CreateTable<Node>(false);
            //}

            //if (!db.TableExist("Property"))
            //{
            //    db.CreateTable<Property>(false);
            //}
        }
    }
}