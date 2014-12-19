using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookQuotes
{
    public class BookQuote //Has a custom string indexer??
    {
        Dictionary<string, object> _propBag;

        public BookQuote()
        {
            _propBag = new Dictionary<string, object>();
        }

        public string Header{ get; set; }
        public string Content{ get; set; }

        // this is how you can create a custom indexer in c#  ??
        public object this[string indexer]
        {
            get
            {
                return _propBag[indexer];
            }
            set
            {
                _propBag[indexer] = value;
            }
        }

        public void Insert(string key, object value)
        {
            _propBag.Add(key, value);
        }
    }

    public class BookQuotes: List<BookQuote>
    {
        public BookQuotes()
        {
            //Add(new BookQuote() { Header = "Evil Plans", Content = "làm cho nhiều người ghét mình thì dễ, bạn chỉ cần thực sự thành công khi làm điều gì đó mình thích" });
            //Add(new BookQuote() { Header = "David And Goliath", Content = "lý thuyết về đường cong hình chuông: bao nhiêu là đủ và bao nhiêu là thừa?" });
            //Add(new BookQuote() { Header = "The Tipping Point", Content = "nhóm thương cảm 12, số 7 kì diệu, quy tắc nhóm 150" });
            //Add(new BookQuote() { Header = "The New Digital Age", Content = "có 4 thứ con người từng tạo ra mà không hiểu hết về chúng: hơi nước, điện, vũ khí hạt nhân và internet" });
        }
    }
}
