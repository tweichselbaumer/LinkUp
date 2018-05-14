using Android.App;
using Android.Views;
using Android.Widget;
using LinkUp.Explorer.WebService.DataContract;
using System.Collections.Generic;

namespace LinkUp.Explorer
{
    public class MainActivityAdapter : BaseAdapter<Node>
    {
        private Activity context;
        private List<Node> items;

        public MainActivityAdapter(Activity context, List<Node> items)
            : base()
        {
            this.context = context;
            this.items = items;
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override Node this[int position]
        {
            get { return items[position]; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            if (view == null)
                view = context.LayoutInflater.Inflate(Resource.Layout.ListViewNodeRow, null);
            view.FindViewById<TextView>(Resource.Id.Name).Text = item.Name;
            return view;
        }
    }
}