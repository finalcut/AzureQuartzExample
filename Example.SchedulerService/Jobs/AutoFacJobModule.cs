﻿using Example.Communication.Slack;
using Autofac;
using Autofac.Extras.Quartz;
using Quartz;
using System.Reflection;

namespace Example.SchedulerService.Jobs
 {
     public class AutoFacJobModule : Autofac.Module
     {
         public AutoFacJobModule()
         {
         }
 
         protected override void Load(ContainerBuilder builder)
         {
             base.Load(builder);
             builder.RegisterModule(new QuartzAutofacFactoryModule());

             /* extracted this registration from:
                    QuartzAutofacJobsModule
              * https://github.com/alphacloud/Autofac.Extras.Quartz/blob/master/src/Autofac.Extras.Quartz/QuartzAutofacJobsModule.cs
                so that I could add PropertiesAutowired
             */

             Assembly assemblyToScan = typeof(AutoFacJobModule).Assembly;

             builder.RegisterAssemblyTypes(assemblyToScan)
               .Where(type => !type.IsAbstract && typeof(IJob).IsAssignableFrom(type))
               .PropertiesAutowired()
               .AsSelf().InstancePerLifetimeScope();

         }
     }
 }
