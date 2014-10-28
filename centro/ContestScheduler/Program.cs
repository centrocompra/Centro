using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Handler;

namespace ContestScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            //ContestHandler.Winners();

            SellersHandler.ExpireJobs(7);
        }
    }
}
