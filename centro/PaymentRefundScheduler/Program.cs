using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Handler;

namespace PaymentRefundScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            PaymentHandler.RefundPaymentScheduler(4320); // 72 hours
            //PaymentHandler.test();
        }
    }
}
