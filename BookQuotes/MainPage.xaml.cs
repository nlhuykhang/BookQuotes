using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Json;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using Windows.Storage;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;


namespace BookQuotes
{


    static class MyExtensions
    {

        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        
    }

    public sealed partial class MainPage : Page
    {

        private ObservableCollection<BookQuote> ocBookQuotes;
        private NotificationHelper notificationHelper;
        private Frame frame;
        private BackgroundTaskBuilder taskBuilder;
        private BackgroundTaskRegistration registration;
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;


            frame = Window.Current.Content as Frame;
            
            InitListQuotes(null, null);
        }

        #region Data
        private async Task<BookQuotes> ReadData()
        {
            BookQuotes result = null;
            var notesFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            using(Stream stream = await notesFolder.OpenStreamForReadAsync("bookquotes"))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(BookQuotes));

                result = serializer.ReadObject(stream) as BookQuotes;
            }
            return result;
        }

        private async void CreateFileFirstTime()
        {
            var notesFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            await notesFolder.CreateFileAsync("bookquotes", CreationCollisionOption.ReplaceExisting);
            //using (Stream stream = await notesFolder.OpenStreamForWriteAsync("bookquotes", CreationCollisionOption.OpenIfExists))
            //{
            //    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(BookQuotes));

            //    serializer.WriteObject(stream, bookquotes);
            //}
        }

        private List<BookQuote> GetOriginalQuotes()
        {
            List<BookQuote> re = new List<BookQuote>();
            //var notesFolder = Windows.Storage.;
            //var xElem = XElement.Load(@"originalQuotes.xml");
            XDocument doc = XDocument.Load(@"originalQuotes.xml");
            
            IEnumerable<XElement> list = doc.Descendants("Header").ToList();

            foreach (var i in list)
            {
                BookQuote temp = new BookQuote();
                temp.Header = i.Value;
                temp.Content = i.Parent.Element("Content").Value;
                re.Add(temp);
            }

            return re.Distinct().ToList();
        }

        private async void WriteData(List<BookQuote> bookquotes)
        {
            var notesFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            using(Stream stream = await notesFolder.OpenStreamForWriteAsync("bookquotes",CreationCollisionOption.ReplaceExisting))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(BookQuotes));

                serializer.WriteObject(stream, bookquotes);
            }        
        }
        #endregion
        

        private async void InitListQuotes(object sender, RoutedEventArgs e)
        {
            BookQuotes x = new BookQuotes();

            if (ocBookQuotes != null)
            {
                ocBookQuotes.CollectionChanged -= ocBookQuotes_CollectionChanged;
            }


            if(!Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("FirstLaunch"))
            {
                CreateFileFirstTime();
                foreach (var i in GetOriginalQuotes())                //cập nhật từ file xml
                {
                    x.Add(i);
                }
                WriteData(x.ToList());
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["FirstLaunch"] = "Launched";
            }
            else
            {
                try
                {
                    x = await ReadData();
                }
                catch (Exception)
                {

                    //throw;
                }

                if (x == null)
                {
                    x = new BookQuotes();
                }
            }

            MyExtensions.Shuffle(x);
            
            //ocBookQuotes = new ObservableCollection<BookQuote>(x.OrderBy(a => a.Header).ToList());
            ocBookQuotes = new ObservableCollection<BookQuote>(x);
            
            ocBookQuotes.CollectionChanged += ocBookQuotes_CollectionChanged;

            BookQuotesCVS.Source = ocBookQuotes;
            QuoteCount.Text = "Count: " + ocBookQuotes.Count;
            //BookQuotesCVS = ocBookQuotes.OrderBy(a => a.Header).ToList();
        }

        private void ocBookQuotes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            BookQuotesCVS.Source = ocBookQuotes;
        }


        

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {

            if(e.NavigationMode == NavigationMode.New)
            {
                var storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///VoiceCommandDefinition1.xml"));
                await Windows.Media.SpeechRecognition.VoiceCommandManager.InstallCommandSetsFromStorageFileAsync(storageFile);
            }

            if(e.SourcePageType.GetType() == typeof(AddQuotePage))
            {
                ocBookQuotes = new ObservableCollection<BookQuote>(null);
            }
            

            //if(e.SourcePageType.)


            if(e.Parameter != null && e.Parameter.GetType() == typeof(BookQuote))
            {
                BookQuote temp = e.Parameter as BookQuote;
                ocBookQuotes.Add(temp);
                WriteData(ocBookQuotes.ToList());
            }
            
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddBarButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> temp = ocBookQuotes.Select(a => a.Header).Distinct().ToList();
            this.Frame.Navigate(typeof(AddQuotePage),temp);
        }

        private async void startNotification_Click(object sender, RoutedEventArgs e)
        {
            notificationHelper = new NotificationHelper(this.Frame);
            startNot.IsEnabled = false;
            stopNot.IsEnabled = true;
            await RegisterBackgroundTask();
        }

        private void DeleteBarButton_Click(object sender, RoutedEventArgs e)
        {
            List<BookQuote> items = lvBookQuotes.SelectedItems.Cast<BookQuote>().ToList();

            foreach (var item in items)
            {
                ocBookQuotes.Remove(item);
            }
            WriteData(ocBookQuotes.ToList());
            lvBookQuotes.ItemsSource = ocBookQuotes;
        }

        private void EditBarButton_Click(object sender, RoutedEventArgs e)
        {
            notificationHelper.showGhostNotification(null);
        }


        private void UnregisterBackgroundTask()
        {
            var taskRegistration = BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault();
            if (taskRegistration != null)
            {
                taskRegistration.Unregister(true);
                if (registration != null)
                {
                    registration.Completed -= registration_Completed;
                }
                
            }   
        }
        private async Task RegisterBackgroundTask()
        {
            UnregisterBackgroundTask();
            var access = await BackgroundExecutionManager.RequestAccessAsync();

            if(access == BackgroundAccessStatus.Denied)
            {
                return;
            }
            else
            {
                taskBuilder = new BackgroundTaskBuilder();
                taskBuilder.Name = "BookQuotesBackgroundTask";

                taskBuilder.SetTrigger(new TimeTrigger(15,false));
                
                //taskBuilder.AddCondition(new SystemCondition(SystemConditionType.UserNotPresent));

                taskBuilder.TaskEntryPoint = typeof(myBackgroundTask.myTask).FullName;

                registration = taskBuilder.Register();
                registration.Completed += registration_Completed;
            }
            
        }


        void registration_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Random r = new Random();
            
            BookQuote temp = ocBookQuotes[r.Next(0, ocBookQuotes.Count / 3)];
            showGhostNotification(temp);

            temp = ocBookQuotes[r.Next(ocBookQuotes.Count / 3, ocBookQuotes.Count * 2 / 3)];
            showGhostNotification(temp);

            temp = ocBookQuotes[r.Next(ocBookQuotes.Count * 2 / 3, ocBookQuotes.Count)];
            showGhostNotification(temp);
        }

        public void showGhostNotification(BookQuote bq)
        {

            var toastDescriptor = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

            var txtNodes = toastDescriptor.GetElementsByTagName("text");

            txtNodes[0].AppendChild(toastDescriptor.CreateTextNode(bq.Header));
            txtNodes[1].AppendChild(toastDescriptor.CreateTextNode(bq.Content));

            ((XmlElement)toastDescriptor.SelectSingleNode("/toast")).SetAttribute("launch", "0/"+bq.Header.ToString()+"/"+bq.Content.ToString());

            var toast = new ToastNotification(toastDescriptor);
            // add tag/group is needed
            Random r = new Random();
            toast.Group = r.Next().ToString();
            toast.Tag = r.Next().ToString();
            
            // Ghost toast
            //toast.SuppressPopup = true;

            //toast.Activated += toast_Activated;
            
            var toastNotifier = ToastNotificationManager.CreateToastNotifier();

            toastNotifier.Show(toast);

        }

        void toast_Activated(ToastNotification sender, object args)
        {
            //sender.Content.LastChild.
            //Frame frame = Window.Current.Content as Frame;
            string header = sender.Content.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.NodeValue.ToString();
            frame.Navigate(typeof(DetailQuotePage), new BookQuote() { Header = header, Content = sender.Content.InnerText });
        }

        private void randomQuotes_Click(object sender, RoutedEventArgs e)       //not used
        {

        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)                   //random click
        {
            MyExtensions.Shuffle(ocBookQuotes);
        }

        private void lvBookQuotes_ItemClick(object sender, ItemClickEventArgs e)                //item click
        {
            frame.Navigate(typeof(DetailQuotePage), e.ClickedItem);
        }

        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            if(chooseButton.IsChecked == true)
            {
                lvBookQuotes.SelectionMode = ListViewSelectionMode.Multiple;
                lvBookQuotes.IsItemClickEnabled = false;
                deleteButton.IsEnabled = true;
            }
            else
            {
                lvBookQuotes.SelectionMode = ListViewSelectionMode.Single;
                lvBookQuotes.IsItemClickEnabled = true;
                deleteButton.IsEnabled = false;
            }
        }

        private void stopNotifcation_Click(object sender, RoutedEventArgs e)
        {
            startNot.IsEnabled = true;
            stopNot.IsEnabled = false;
            UnregisterBackgroundTask();
        }

    }
}
