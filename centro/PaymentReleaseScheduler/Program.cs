using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Handler;

namespace PaymentReleaseScheduler
{
    class Program
    {
        static void Main(string[] args) 
        {
             PaymentHandler.ReleasePaymentScheduler();
        }
    }
}
