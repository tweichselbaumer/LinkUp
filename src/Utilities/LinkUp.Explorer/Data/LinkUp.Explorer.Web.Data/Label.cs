namespace LinkUp.Explorer.WebService.DataContract
{
    public class Label
    {
        private string _Name;
        private object _Value;
        public string Name { get => _Name; set => _Name = value; }
        public object Value { get => _Value; set => _Value = value; }
    }
}