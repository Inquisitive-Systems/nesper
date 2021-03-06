///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;
using com.espertech.esper.util.support;

using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.supportregression.events.SupportEventInfra;

namespace com.espertech.esper.regression.events.infra
{
    using Map = IDictionary<string, object>;

    public class ExecEventInfraPropertyNestedIndexed : RegressionExecution
    {
        private static readonly string BEAN_TYPENAME = TypeHelper.MaskTypeName(
            typeof(InfraNestedIndexPropTop).FullName);

        private static readonly ExecEventInfraPropertyNestedSimple.FunctionSendEvent4Int FBEAN = (epService, lvl1, lvl2, lvl3, lvl4) =>
        {
            var l4 = new InfraNestedIndexedPropLvl4(lvl4);
            var l3 = new InfraNestedIndexedPropLvl3(new[] {l4}, lvl3);
            var l2 = new InfraNestedIndexedPropLvl2(new[] {l3}, lvl2);
            var l1 = new InfraNestedIndexedPropLvl1(new[] {l2}, lvl1);
            var top = new InfraNestedIndexPropTop(new[] {l1});
            epService.EPRuntime.SendEvent(top);
        };

        private static readonly ExecEventInfraPropertyNestedSimple.FunctionSendEvent4Int FMAP = (epService, lvl1, lvl2, lvl3, lvl4) =>
        {
            var l4 = Collections.SingletonDataMap("lvl4", lvl4);
            IDictionary<string, object> l3 = TwoEntryMap("l4", new[] {l4}, "lvl3", lvl3);
            IDictionary<string, object> l2 = TwoEntryMap("l3", new[] {l3}, "lvl2", lvl2);
            IDictionary<string, object> l1 = TwoEntryMap("l2", new[] {l2}, "lvl1", lvl1);
            var top = Collections.SingletonDataMap("l1", new[] {l1});
            epService.EPRuntime.SendEvent(top, MAP_TYPENAME);
        };

        private static readonly ExecEventInfraPropertyNestedSimple.FunctionSendEvent4Int FOA = (epService, lvl1, lvl2, lvl3, lvl4) =>
        {
            var l4 = new object[] {lvl4};
            var l3 = new object[] {new object[] {l4}, lvl3};
            var l2 = new object[] {new object[] {l3}, lvl2};
            var l1 = new object[] {new object[] {l2}, lvl1};
            var top = new object[] {new object[] {l1}};
            epService.EPRuntime.SendEvent(top, OA_TYPENAME);
        };

        private static readonly ExecEventInfraPropertyNestedSimple.FunctionSendEvent4Int FXML = (epService, lvl1, lvl2, lvl3, lvl4) =>
        {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                      "<myevent>\n" +
                      "\t<l1 lvl1=\"${lvl1}\">\n" +
                      "\t\t<l2 lvl2=\"${lvl2}\">\n" +
                      "\t\t\t<l3 lvl3=\"${lvl3}\">\n" +
                      "\t\t\t\t<l4 lvl4=\"${lvl4}\">\n" +
                      "\t\t\t\t</l4>\n" +
                      "\t\t\t</l3>\n" +
                      "\t\t</l2>\n" +
                      "\t</l1>\n" +
                      "</myevent>";
            xml = xml.Replace("${lvl1}", Convert.ToString(lvl1));
            xml = xml.Replace("${lvl2}", Convert.ToString(lvl2));
            xml = xml.Replace("${lvl3}", Convert.ToString(lvl3));
            xml = xml.Replace("${lvl4}", Convert.ToString(lvl4));
            try
            {
                SupportXML.SendEvent(epService.EPRuntime, xml);
            }
            catch (Exception e)
            {
                throw new EPRuntimeException(e);
            }
        };

        private static readonly ExecEventInfraPropertyNestedSimple.FunctionSendEvent4Int FAVRO = (epService, lvl1, lvl2, lvl3, lvl4) =>
        {
            RecordSchema schema = GetAvroSchema();
            RecordSchema lvl1Schema = schema.GetField("l1").Schema.GetElementType().AsRecordSchema();
            RecordSchema lvl2Schema = lvl1Schema.GetField("l2").Schema.GetElementType().AsRecordSchema();
            RecordSchema lvl3Schema = lvl2Schema.GetField("l3").Schema.GetElementType().AsRecordSchema();
            RecordSchema lvl4Schema = lvl3Schema.GetField("l4").Schema.GetElementType().AsRecordSchema();

            var lvl4Rec = new GenericRecord(lvl4Schema);
            lvl4Rec.Put("lvl4", lvl4);
            var lvl3Rec = new GenericRecord(lvl3Schema);
            lvl3Rec.Put("l4", Collections.SingletonList(lvl4Rec));
            lvl3Rec.Put("lvl3", lvl3);
            var lvl2Rec = new GenericRecord(lvl2Schema);
            lvl2Rec.Put("l3", Collections.SingletonList(lvl3Rec));
            lvl2Rec.Put("lvl2", lvl2);
            var lvl1Rec = new GenericRecord(lvl1Schema);
            lvl1Rec.Put("l2", Collections.SingletonList(lvl2Rec));
            lvl1Rec.Put("lvl1", lvl1);
            var datum = new GenericRecord(schema);
            datum.Put("l1", Collections.SingletonList(lvl1Rec));
            epService.EPRuntime.SendEventAvro(datum, AVRO_TYPENAME);
        };

        public override void Configure(Configuration configuration)
        {
            AddXMLEventType(configuration);
        }

        public override void Run(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType(BEAN_TYPENAME, typeof(InfraNestedIndexPropTop));
            AddMapEventType(epService);
            AddOAEventType(epService);
            AddAvroEventType(epService);

            RunAssertion(
                epService, BEAN_TYPENAME, FBEAN, typeof(InfraNestedIndexedPropLvl1),
                typeof(InfraNestedIndexedPropLvl1).Name);
            RunAssertion(epService, MAP_TYPENAME, FMAP, typeof(Map), MAP_TYPENAME + "_1");
            RunAssertion(epService, OA_TYPENAME, FOA, typeof(object[]), OA_TYPENAME + "_1");
            RunAssertion(epService, XML_TYPENAME, FXML, typeof(XmlNode), "MyXMLEvent.l1");
            RunAssertion(epService, AVRO_TYPENAME, FAVRO, typeof(GenericRecord), AVRO_TYPENAME + "_1");
        }

        private void RunAssertion(
            EPServiceProvider epService, string typename, ExecEventInfraPropertyNestedSimple.FunctionSendEvent4Int send, Type nestedClass,
            string fragmentTypeName)
        {
            RunAssertionSelectNested(epService, typename, send);
            RunAssertionBeanNav(epService, typename, send);
            RunAssertionTypeValidProp(epService, typename, send, nestedClass, fragmentTypeName);
            RunAssertionTypeInvalidProp(epService, typename);
        }

        private void RunAssertionBeanNav(EPServiceProvider epService, string typename, ExecEventInfraPropertyNestedSimple.FunctionSendEvent4Int send)
        {
            var epl = "select * from " + typename;
            var statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;

            send.Invoke(epService, 1, 2, 3, 4);
            var @event = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(
                @event, "l1[0].lvl1,l1[0].l2[0].lvl2,l1[0].l2[0].l3[0].lvl3,l1[0].l2[0].l3[0].l4[0].lvl4".Split(','),
                new object[] {1, 2, 3, 4});
            SupportEventTypeAssertionUtil.AssertConsistency(@event);
            var isNative = typename.Equals(BEAN_TYPENAME);
            SupportEventTypeAssertionUtil.AssertFragments(
                @event, isNative, true, "l1,l1[0].l2,l1[0].l2[0].l3,l1[0].l2[0].l3[0].l4");
            SupportEventTypeAssertionUtil.AssertFragments(
                @event, isNative, false, "l1[0],l1[0].l2[0],l1[0].l2[0].l3[0],l1[0].l2[0].l3[0].l4[0]");

            RunAssertionEventInvalidProp(@event);

            statement.Dispose();
        }

        private void RunAssertionSelectNested(EPServiceProvider epService, string typename, ExecEventInfraPropertyNestedSimple.FunctionSendEvent4Int send)
        {
            var epl = "select " +
                      "l1[0].lvl1 as c0, " +
                      "exists(l1[0].lvl1) as exists_c0, " +
                      "l1[0].l2[0].lvl2 as c1, " +
                      "exists(l1[0].l2[0].lvl2) as exists_c1, " +
                      "l1[0].l2[0].l3[0].lvl3 as c2, " +
                      "exists(l1[0].l2[0].l3[0].lvl3) as exists_c2, " +
                      "l1[0].l2[0].l3[0].l4[0].lvl4 as c3, " +
                      "exists(l1[0].l2[0].l3[0].l4[0].lvl4) as exists_c3 " +
                      "from " + typename;
            var statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            var fields = "c0,exists_c0,c1,exists_c1,c2,exists_c2,c3,exists_c3".Split(',');

            foreach (var property in fields)
            {
                Assert.AreEqual(
                    property.StartsWith("exists") ? typeof(bool?) : typeof(int?),
                    statement.EventType.GetPropertyType(property).GetBoxedType());
            }

            send.Invoke(epService, 1, 2, 3, 4);
            var @event = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(@event, fields, new object[] {1, true, 2, true, 3, true, 4, true});
            SupportEventTypeAssertionUtil.AssertConsistency(@event);

            send.Invoke(epService, 10, 5, 50, 400);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {10, true, 5, true, 50, true, 400, true});

            statement.Dispose();
        }

        private void AddMapEventType(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType(
                MAP_TYPENAME + "_4", Collections.SingletonDataMap("lvl4", typeof(int)));
            epService.EPAdministrator.Configuration.AddEventType(
                MAP_TYPENAME + "_3", TwoEntryMap("l4", MAP_TYPENAME + "_4[]", "lvl3", typeof(int)));
            epService.EPAdministrator.Configuration.AddEventType(
                MAP_TYPENAME + "_2", TwoEntryMap("l3", MAP_TYPENAME + "_3[]", "lvl2", typeof(int)));
            epService.EPAdministrator.Configuration.AddEventType(
                MAP_TYPENAME + "_1", TwoEntryMap("l2", MAP_TYPENAME + "_2[]", "lvl1", typeof(int)));
            epService.EPAdministrator.Configuration.AddEventType(
                MAP_TYPENAME, Collections.SingletonDataMap("l1", MAP_TYPENAME + "_1[]"));
        }

        private void AddOAEventType(EPServiceProvider epService)
        {
            var type_4 = OA_TYPENAME + "_4";
            string[] names_4 = {"lvl4"};
            object[] types_4 = {typeof(int)};
            epService.EPAdministrator.Configuration.AddEventType(type_4, names_4, types_4);
            var type_3 = OA_TYPENAME + "_3";
            string[] names_3 = {"l4", "lvl3"};
            object[] types_3 = {type_4 + "[]", typeof(int)};
            epService.EPAdministrator.Configuration.AddEventType(type_3, names_3, types_3);
            var type_2 = OA_TYPENAME + "_2";
            string[] names_2 = {"l3", "lvl2"};
            object[] types_2 = {type_3 + "[]", typeof(int)};
            epService.EPAdministrator.Configuration.AddEventType(type_2, names_2, types_2);
            var type_1 = OA_TYPENAME + "_1";
            string[] names_1 = {"l2", "lvl1"};
            object[] types_1 = {type_2 + "[]", typeof(int)};
            epService.EPAdministrator.Configuration.AddEventType(type_1, names_1, types_1);
            string[] names = {"l1"};
            object[] types = {type_1 + "[]"};
            epService.EPAdministrator.Configuration.AddEventType(OA_TYPENAME, names, types);
        }

        private void AddXMLEventType(Configuration configuration)
        {
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:l1\" maxOccurs=\"unbounded\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"l1\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:l2\" maxOccurs=\"unbounded\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t\t<xs:attribute name=\"lvl1\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"l2\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:l3\" maxOccurs=\"unbounded\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t\t<xs:attribute name=\"lvl2\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"l3\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:l4\" maxOccurs=\"unbounded\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t\t<xs:attribute name=\"lvl3\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"l4\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:attribute name=\"lvl4\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.AddEventType(XML_TYPENAME, eventTypeMeta);
        }

        private void AddAvroEventType(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventTypeAvro(
                AVRO_TYPENAME, new ConfigurationEventTypeAvro(GetAvroSchema()));
        }

        private static RecordSchema GetAvroSchema()
        {
            Schema s4 = SchemaBuilder.Record(AVRO_TYPENAME + "_4", TypeBuilder.RequiredInt("lvl4"));
            Schema s3 = SchemaBuilder.Record(
                AVRO_TYPENAME + "_3",
                TypeBuilder.Field("l4", TypeBuilder.Array(s4)),
                TypeBuilder.RequiredInt("lvl3"));
            Schema s2 = SchemaBuilder.Record(
                AVRO_TYPENAME + "_2",
                TypeBuilder.Field("l3", TypeBuilder.Array(s3)),
                TypeBuilder.RequiredInt("lvl2"));
            Schema s1 = SchemaBuilder.Record(
                AVRO_TYPENAME + "_1",
                TypeBuilder.Field("l2", TypeBuilder.Array(s2)),
                TypeBuilder.RequiredInt("lvl1"));

            return SchemaBuilder.Record(
                AVRO_TYPENAME,
                TypeBuilder.Field("l1", TypeBuilder.Array(s1)));
        }

        private void RunAssertionEventInvalidProp(EventBean @event)
        {
            foreach (var prop in Collections.List("l2", "l2[0]", "l1[0].l3", "l1[0].l2[0].l3[0].x"))
            {
                SupportMessageAssertUtil.TryInvalidProperty(@event, prop);
                SupportMessageAssertUtil.TryInvalidGetFragment(@event, prop);
            }
        }

        private void RunAssertionTypeValidProp(
            EPServiceProvider epService,
            string typeName,
            ExecEventInfraPropertyNestedSimple.FunctionSendEvent4Int send,
            Type nestedClass,
            string fragmentTypeName)
        {
            var eventType = epService.EPAdministrator.Configuration.GetEventType(typeName);
            var arrayType = nestedClass == typeof(object[]) ? nestedClass : TypeHelper.GetArrayType(nestedClass);
            var expectedType = new[] {new object[] {"l1", arrayType, fragmentTypeName, true}};
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedType, eventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());

            EPAssertionUtil.AssertEqualsAnyOrder(new[] {"l1"}, eventType.PropertyNames);

            foreach (var prop in Collections.List("l1[0]", "l1[0].lvl1", "l1[0].l2", "l1[0].l2[0]", "l1[0].l2[0].lvl2"))
            {
                Assert.IsNotNull(eventType.GetGetter(prop));
                Assert.IsTrue(eventType.IsProperty(prop));
            }

            Assert.AreEqual(arrayType, eventType.GetPropertyType("l1"));
            foreach (var prop in Collections.List("l1[0].lvl1", "l1[0].l2[0].lvl2", "l1[0].l2[0].l3[0].lvl3"))
            {
                Assert.AreEqual(typeof(int?), TypeHelper.GetBoxedType(eventType.GetPropertyType(prop)));
            }

            var lvl1Fragment = eventType.GetFragmentType("l1");
            Assert.IsTrue(lvl1Fragment.IsIndexed);
            Assert.AreEqual(send == FBEAN, lvl1Fragment.IsNative);
            Assert.AreEqual(fragmentTypeName, lvl1Fragment.FragmentType.Name);

            var lvl2Fragment = eventType.GetFragmentType("l1[0].l2");
            Assert.IsTrue(lvl2Fragment.IsIndexed);
            Assert.AreEqual(send == FBEAN, lvl2Fragment.IsNative);

            Assert.AreEqual(
                new EventPropertyDescriptor("l1", arrayType, nestedClass, false, false, true, false, true),
                eventType.GetPropertyDescriptor("l1"));
        }

        private void RunAssertionTypeInvalidProp(EPServiceProvider epService, string typeName)
        {
            var eventType = epService.EPAdministrator.Configuration.GetEventType(typeName);

            foreach (var prop in Collections.List(
                "l2[0]", "l1[0].l3", "l1[0].lvl1.lvl1", "l1[0].l2.l4", "l1[0].l2[0].xx", "l1[0].l2[0].l3[0].lvl5"))
            {
                Assert.AreEqual(false, eventType.IsProperty(prop));
                Assert.AreEqual(null, eventType.GetPropertyType(prop));
                Assert.IsNull(eventType.GetPropertyDescriptor(prop));
            }
        }

        private class InfraNestedIndexPropTop
        {
            public InfraNestedIndexPropTop(InfraNestedIndexedPropLvl1[] l1)
            {
                L1 = l1;
            }

            [PropertyName("l1")]
            public InfraNestedIndexedPropLvl1[] L1 { get; private set; }
        }

        private class InfraNestedIndexedPropLvl1
        {
            public InfraNestedIndexedPropLvl1(InfraNestedIndexedPropLvl2[] l2, int lvl1)
            {
                L2 = l2;
                Lvl1 = lvl1;
            }

            [PropertyNameAttribute("l2")]
            public InfraNestedIndexedPropLvl2[] L2 { get; private set; }

            [PropertyNameAttribute("lvl1")]
            public int Lvl1 { get; private set; }
        }

        private class InfraNestedIndexedPropLvl2
        {
            public InfraNestedIndexedPropLvl2(InfraNestedIndexedPropLvl3[] l3, int lvl2)
            {
                L3 = l3;
                Lvl2 = lvl2;
            }

            [PropertyNameAttribute("l3")]
            public InfraNestedIndexedPropLvl3[] L3 { get; private set; }

            [PropertyNameAttribute("lvl2")]
            public int Lvl2 { get; private set; }
        }

        private class InfraNestedIndexedPropLvl3
        {
            public InfraNestedIndexedPropLvl3(InfraNestedIndexedPropLvl4[] l4, int lvl3)
            {
                L4 = l4;
                Lvl3 = lvl3;
            }

            [PropertyNameAttribute("l4")]
            public InfraNestedIndexedPropLvl4[] L4 { get; private set; }

            [PropertyNameAttribute("lvl3")]
            public int Lvl3 { get; private set; }
        }

        private class InfraNestedIndexedPropLvl4
        {
            public InfraNestedIndexedPropLvl4(int lvl4)
            {
                Lvl4 = lvl4;
            }

            [PropertyNameAttribute("lvl4")]
            public int Lvl4 { get; private set; }
        }
    }
} // end of namespace
