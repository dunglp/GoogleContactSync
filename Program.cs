using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleContactSync
{
    class Program
    {
        private static string Email = "seta.crm.demo@gmail.com";
        
        static void Main(string[] args)
        {
            var Emails = new List<string>();
            Emails.Add("seta.crm.demo@gmail.com");
            Emails.Add("dung.le@stacinq.vn");
            var synchronizer = new Synchronizer();
            //synchronizer.LoginToGoogle(Emails[1]);
            //synchronizer.LoadContacts();    

            foreach (var email in Emails)
            {
                synchronizer.LoginToGoogle(email);
                synchronizer.LoadContacts();
            }
        }
    }
}
