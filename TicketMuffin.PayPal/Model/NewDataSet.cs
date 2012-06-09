using System.Xml.Serialization;

namespace TicketMuffin.PayPal.Model
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/ap")]
    [XmlRoot(Namespace = "http://svcs.paypal.com/types/ap", IsNullable = false)]
    public partial class NewDataSet
    {

        private PayResponse[] itemsField;

        /// <remarks/>
        [XmlElement("PayResponse")]
        public PayResponse[] Items
        {
            get { return this.itemsField; }
            set { this.itemsField = value; }
        }
    }
}