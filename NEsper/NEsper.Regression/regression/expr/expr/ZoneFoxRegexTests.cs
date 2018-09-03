using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ZoneFoxRegexTests : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            RunAssertionTestRegexStartsWith(epService);
            RunAssertionTestRegexSlashU(epService);
        }

        public void RunAssertionTestRegexStartsWith(EPServiceProvider epService)
        {
            string caseExpr = @"select p00 as result from " + typeof(SupportBean_S0).FullName +
                              @" where p00.StartsWith('\\user\\bob', StringComparison.InvariantCultureIgnoreCase)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_S0(-1, @"\user\bob"));

            Assert.AreEqual(@"\user\bob", listener.AssertOneGetNewAndReset().Get("result"));
        }

        public void RunAssertionTestRegexSlashU(EPServiceProvider epService)
        {
            string caseExpr = @"select p00 regexp '.*\\user\\.*' as result from " + typeof(SupportBean_S0).FullName;

            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_S0(-1, @"\\user\\bob"));
            Assert.IsTrue(listener.AssertOneGetNewAndReset().Get("result").AsBoolean());

            epService.EPRuntime.SendEvent(new SupportBean_S0(-1, "TBT-BC"));
            Assert.IsFalse(listener.AssertOneGetNewAndReset().Get("result").AsBoolean());
        }
    }
}