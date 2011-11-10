﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Dto;
using GroupGiving.Core.Services;
using Raven.Client;

namespace GroupGiving.Core.Actions.RefundPledge
{
    public class RefundPledgeAction
    {
        private IPaymentGateway _paymentGateway;

        public RefundPledgeAction(IPaymentGateway paymentGateway)
        {
            _paymentGateway = paymentGateway;
        }

        public RefundResponse Execute(IDocumentSession session, string eventId, string pledgeOrderNumber)
        {
                var @event = session.Load<GroupGivingEvent>(eventId);
                if (@event == null)
                {
                    throw new ArgumentException("No event could be found matching that id");
                }


                var pledge = @event.Pledges.Where(p => p.OrderNumber == pledgeOrderNumber).FirstOrDefault();

                if (pledge == null)
                {
                    throw new ArgumentException("No pledge could be found matching that order number");
                }

                RefundResponse refundResponse = null;
                try
                {
                    refundResponse = _paymentGateway.Refund(new RefundRequest()
                                                              {
                                                                  TransactionId = pledge.TransactionId,
                                                                  Receivers = new List<PaymentRecipient>()
                                                                                  {
                                                                                      new PaymentRecipient(
                                                                                          pledge.AccountEmailAddress,
                                                                                          pledge.Total, true)
                                                                                  }
                                                              });
                }
                catch (HttpChannelException exception)
                {
                    if (pledge.PaymentGatewayHistory == null)
                        pledge.PaymentGatewayHistory = new List<DialogueHistoryEntry>();
                    pledge.PaymentGatewayHistory.Add(((ResponseBase) exception.FaultMessage).Raw);
                    session.SaveChanges();
                    throw;
                }

                if (pledge.PaymentGatewayHistory == null)
                    pledge.PaymentGatewayHistory = new List<DialogueHistoryEntry>();
                pledge.PaymentGatewayHistory.Add(refundResponse.DialogueEntry);

                pledge.Notes = refundResponse.Message;
                if (refundResponse.Successful)
                {
                    pledge.DateRefunded = DateTime.Now;
                    pledge.PaymentStatus = PaymentStatus.Refunded;
                    pledge.Paid = false;
                }

                session.SaveChanges();

                return refundResponse;
        }
    }
}