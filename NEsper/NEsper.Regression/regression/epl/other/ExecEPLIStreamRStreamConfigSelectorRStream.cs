///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLIStreamRStreamConfigSelectorRStream : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.StreamSelection.DefaultStreamSelector = StreamSelector.RSTREAM_ONLY;
        }
    
        public override void Run(EPServiceProvider epService) {
            string stmtText = "select * from " + typeof(SupportBean).FullName + "#length(3)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            Object theEvent = SendEvent(epService, "a");
            SendEvent(epService, "b");
            SendEvent(epService, "c");
            Assert.IsFalse(testListener.IsInvoked);
    
            SendEvent(epService, "d");
            Assert.IsTrue(testListener.IsInvoked);
            Assert.AreSame(theEvent, testListener.LastNewData[0].Underlying);    // receive 'a' as new data
            Assert.IsNull(testListener.LastOldData);  // receive no more old data
        }
    
        private Object SendEvent(EPServiceProvider epService, string stringValue) {
            var theEvent = new SupportBean(stringValue, 0);
            epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    }
} // end of namespace
