using Android.App;
using Android.OS;
using Android.Widget;
using LinkUp.Explorer.WebService.DataContract;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;

namespace LinkUp.Explorer
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private ListView _ListView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);

            _ListView = FindViewById<ListView>(Resource.Id.NodeList);
            _ListView.Adapter = new MainActivityAdapter(this, GetItems());
        }

        private List<Node> GetItems()
        {
            RestClient client = new RestClient("http://192.168.1.232:5000");
            RestRequest request = new RestRequest("api/node", Method.GET);
            IRestResponse response = client.Execute(request);

            Node master = JsonConvert.DeserializeObject<Node>(response.Content);

            List<Node> list = new List<Node>();

            GetNodesRecursive(master, list, "");

            return list;
        }

        private void GetNodesRecursive(Node master, List<Node> list, string parentName)
        {
            if (master != null)
            {
                foreach (Node node in master.Children)
                {
                    list.Add(node);
                    node.Name = string.Format("{0}/{1}", parentName, node.Name);
                    GetNodesRecursive(node, list, node.Name);
                }
            }
        }
    }
}