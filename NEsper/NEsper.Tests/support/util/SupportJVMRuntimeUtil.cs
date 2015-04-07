///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.support.util
{
    public class SupportJVMRuntimeUtil {
    
        private string MemoryUse() {
            StringWriter writer = new StringWriter();
            Runtime runtime = Runtime.Runtime;
            int mb = 1024*1024;
    
            writer.Append("Used Memory MB:");
            writer.Append(Double.ToString((runtime.TotalMemory() - runtime.FreeMemory()) / mb));
    
            writer.Append("  Free Memory MB:");
            writer.Append(Double.ToString(runtime.FreeMemory() / mb));
    
            writer.Append("  Total Memory MB:");
            writer.Append(Double.ToString(runtime.TotalMemory() / mb));
    
            writer.Append("  Max Memory:");
            writer.Append(Double.ToString(runtime.MaxMemory() / mb));
    
            return writer.ToString();
        }
    
    }
}