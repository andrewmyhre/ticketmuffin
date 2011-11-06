﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;
using GroupGiving.Core.Dto;

// 
// This source code was auto-generated by xsd, Version=4.0.30319.1.
// 

namespace GroupGiving.PayPal.Model
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/ap")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://svcs.paypal.com/types/ap", IsNullable = false)]
    public partial class PayResponse
    {

        private string payKeyField;

        private string paymentExecStatusField;

        private PayResponseResponseEnvelope responseEnvelopeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string payKey
        {
            get { return this.payKeyField; }
            set { this.payKeyField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string paymentExecStatus
        {
            get { return this.paymentExecStatusField; }
            set { this.paymentExecStatusField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public PayResponseResponseEnvelope responseEnvelope
        {
            get { return this.responseEnvelopeField; }
            set { this.responseEnvelopeField = value; }
        }

        public PayPalError Error { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/common")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://svcs.paypal.com/types/common", IsNullable = false)]
    public partial class FaultMessage
    {

        private string payKeyField;

        private string paymentExecStatusField;

        private PayResponseResponseEnvelope responseEnvelopeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string payKey
        {
            get { return this.payKeyField; }
            set { this.payKeyField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string paymentExecStatus
        {
            get { return this.paymentExecStatusField; }
            set { this.paymentExecStatusField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public PayResponseResponseEnvelope responseEnvelope
        {
            get { return this.responseEnvelopeField; }
            set { this.responseEnvelopeField = value; }
        }

        [XmlElement("error", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public PayPalError Error { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace="http://svcs.paypal.com/types/ap")]
    [System.Serializable]
    public class PayPalError
    {
        [XmlElement("errorId", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ErrorId { get; set; }

        [XmlElement("domain", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Domain { get; set; }

        [XmlElement("subDomain", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SubDomain { get; set; }

        [XmlElement("severity", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Severity { get; set; }

        [XmlElement("category", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Category { get; set; }

        [XmlElement("message", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Message { get; set; }

        [XmlElement("parameter", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Parameter { get; set; }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/ap")]
    public partial class PayResponseResponseEnvelope
    {

        private string timestampField;

        private string ackField;

        private string correlationIdField;

        private string buildField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string timestamp
        {
            get { return this.timestampField; }
            set { this.timestampField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ack
        {
            get { return this.ackField; }
            set { this.ackField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string correlationId
        {
            get { return this.correlationIdField; }
            set { this.correlationIdField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string build
        {
            get { return this.buildField; }
            set { this.buildField = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/ap")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://svcs.paypal.com/types/ap", IsNullable = false)]
    public partial class NewDataSet
    {

        private PayResponse[] itemsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PayResponse")]
        public PayResponse[] Items
        {
            get { return this.itemsField; }
            set { this.itemsField = value; }
        }
    }

}