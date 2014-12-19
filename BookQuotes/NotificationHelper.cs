using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;

namespace BookQuotes
{
    class NotificationHelper
    {

        private Frame Frame;
        private BookQuote quote;

        public NotificationHelper(Frame f)
        {
            Frame = f;
        }

        public void showGhostNotification(BookQuote bq)
        {
            quote = bq;

            var toastDescriptor = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

            var txtNodes = toastDescriptor.GetElementsByTagName("text");

            txtNodes[0].AppendChild(toastDescriptor.CreateTextNode(bq.Header));
            txtNodes[1].AppendChild(toastDescriptor.CreateTextNode(bq.Content));

            var toast = new ToastNotification(toastDescriptor);
            // add tag/group is needed
            Random r = new Random();
            toast.Group = r.Next().ToString();
            toast.Tag = r.Next().ToString() ;

            // Ghost toast
            //toast.SuppressPopup = true;

            toast.Activated += toast_Activated;
            
            var toastNotifier = ToastNotificationManager.CreateToastNotifier();
            
            toastNotifier.Show(toast);
            
        }

        void toast_Activated(ToastNotification sender, object args)
        {
            Frame.Navigate(typeof(DetailQuotePage), quote);
        }
    }
}
