using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using GroupGiving.PayPal.Model;
using NUnit.Framework;

namespace GroupGiving.PayPal.Tests.Unit
{
    [TestFixture]
    public class Serialization_Tests
    {
        [Test]
        public void Can_deserialize_a_successful_payment_response()
        {
            string response = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>   <ns:PayResponse xmlns:ns=""http://svcs.paypal.com/types/ap"">
  <responseEnvelope>
  <timestamp>2011-11-05T06:11:04.060-07:00</timestamp> 
  <ack>Success</ack> 
  <correlationId>c3197811b8a7c</correlationId> 
  <build>2228340</build> 
  </responseEnvelope>
  <payKey>AP-97V71823G7502461S</payKey> 
  <paymentExecStatus>CREATED</paymentExecStatus> 
  </ns:PayResponse>";

            var responseObject = DeserializeObject<PayResponse>(response);

            Assert.That(responseObject, Is.Not.Null);
            Assert.That(responseObject.paymentExecStatus, Is.StringMatching("CREATED"));
            Assert.That(responseObject.payKey, Is.Not.Empty);
        }

        private static T DeserializeObject<T>(string response)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof (T), "ns");
            byte[] data = ASCIIEncoding.ASCII.GetBytes(response);
            MemoryStream responseStream = new MemoryStream(data);
            var responseObject = (T)deserializer.Deserialize(responseStream);
            return responseObject;
        }

        [Test]
        public void Can_deserialize_a_fault_message()
        {
            string response = @"<?xml version='1.0' encoding='UTF-8'?>
<ns3:FaultMessage xmlns:ns3=""http://svcs.paypal.com/types/common"" xmlns:ns2=""http://svcs.paypal.com/types/ap"">
<responseEnvelope>
<timestamp>2011-11-05T07:28:21.064-07:00</timestamp>
<ack>Failure</ack>
<correlationId>44f6c00005fd5</correlationId>
<build>2228340</build>
</responseEnvelope>
<error>
<errorId>580022</errorId>
<domain>PLATFORM</domain>
<subdomain>Application</subdomain>
<severity>Error</severity>
<category>Application</category>
<message>Invalid request parameter: requestEnvelope cannot be null</message>
<parameter>requestEnvelope</parameter>
<parameter>null</parameter>
</error>
</ns3:FaultMessage>";

            var responseObject = DeserializeObject<FaultMessage>(response);
            Assert.That(responseObject.Error, Is.Not.Null);
            Assert.That(responseObject.Error.Domain, Is.StringMatching("PLATFORM"));
        }

        [Test]
        public void Can_deserialize_a_refund_response()
        {
            string response = File.ReadAllText("refund_response.xml");

            var responseObject = DeserializeObject<RefundResponse>(response);
            Assert.That(responseObject, Is.Not.Null);
            Assert.That(responseObject.refundInfoList, Has.Length.GreaterThan(0));
            Assert.That(responseObject.refundInfoList[0].refundStatus, Is.StringMatching("ALREADY_REVERSED_OR_REFUNDED"));
        }

        [Test]
        public void Can_Deserialise_a_requestpermissions_response()
        {
            string response = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ns2:RequestPermissionsResponse xmlns:ns2=""http://svcs.paypal.com/types/perm"">
  <responseEnvelope>
    <timestamp>2012-04-14T13:53:49.323-07:00</timestamp>
    <ack>Success</ack>
    <correlationId>7fa15f5b2152a</correlationId>
    <build>2210301</build>
  </responseEnvelope>
  <token>AAAAAAAVsI4iWopwYseS</token>
</ns2:RequestPermissionsResponse>";

            var responseObject = DeserializeObject<RequestPermissionsResponse>(response);
            Assert.That(responseObject, Is.Not.Null);
            Assert.That(responseObject.Token, Is.Not.Null);
        }

        [Test]
        public void GetACcessTokenResponse()
        {
            var response = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ns2:GetAccessTokenResponse xmlns:ns2=""http://svcs.paypal.com/types/perm"">
  <responseEnvelope>
    <timestamp>2012-04-14T14:20:57.614-07:00</timestamp>
    <ack>Success</ack>
    <correlationId>009acfd8ae9c2</correlationId>
    <build>2210301</build>
  </responseEnvelope>
  <scope>REFUND</scope>
  <scope>DIRECT_PAYMENT</scope>
  <scope>TRANSACTION_DETAILS</scope>
  <scope>MASS_PAY</scope>
  <token>8pg3FVMyS1dPieo5xkH29I53SWfUDk5tgnMzHkFLQX8W2DV1jQyz3Q</token>
  <tokenSecret>qUQFPszLr4ErefVceHuT9fnA.Ec</tokenSecret>
</ns2:GetAccessTokenResponse>";

            var responseObject = DeserializeObject<GetAccessTokenResponse>(response);
            Assert.That(responseObject, Is.Not.Null);
            Assert.That(responseObject.Token,Is.Not.Null);
            Assert.That(responseObject.TokenSecret, Is.Not.Null);
        }
    }
}
