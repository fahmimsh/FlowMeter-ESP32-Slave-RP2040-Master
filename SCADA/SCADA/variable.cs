using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace SCADA
{
    internal class OPCData1 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private int _ClientHandle;
        private string _itemName;
        private object _value;
        private Type _tipeReq;
        private string _quality;
        private string _timestamp;
        private int _counter;
        private bool _flag;
        public int ClientHandle { get => _ClientHandle; set { _ClientHandle = value; PropertyChanged(this, new PropertyChangedEventArgs(nameof(ClientHandle))); } }
        public string ItemName { get => _itemName; set { _itemName = value; PropertyChanged(this, new PropertyChangedEventArgs(nameof(ItemName))); } }
        public object Value { get => _value; set { _value = value; PropertyChanged(this, new PropertyChangedEventArgs(nameof(Value))); } }
        public string Quality { get => _quality; set { _quality = value; PropertyChanged(this, new PropertyChangedEventArgs(nameof(Quality))); } }
        public string Timestamp { get => _timestamp; set { _timestamp = value; PropertyChanged(this, new PropertyChangedEventArgs(nameof(Timestamp))); } }
        public int Counter { get => _counter; set { _counter = value; PropertyChanged(this, new PropertyChangedEventArgs(nameof(Counter))); } }
        public bool Flag { get => _flag; set { _flag = value; PropertyChanged(this, new PropertyChangedEventArgs(nameof(Flag))); } }
        public Type TipeReq { get => _tipeReq; set { _tipeReq = value; PropertyChanged(this, new PropertyChangedEventArgs(nameof(TipeReq))); } }
        public OPCData1(string itemName, int clientHandle, double value = 0.0, string quality = null, string timestamp = null, int counter = 0, bool flag = false, Type tipeReq = null)
        {
            _ClientHandle = clientHandle;
            _itemName = itemName;
            _value = value;
            _quality = quality;
            _timestamp = timestamp;
            _counter = counter;
            _flag = flag;
        }
    }
    internal class TagData1 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private int _id;
        public int Id { get => _id; set { _id = value; OnPropertyChanged(nameof(Id)); } }
        private string _tag;
        public string Tag { get => _tag; set { _tag = value; OnPropertyChanged(nameof(Tag)); } }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    internal static class OPCStatus1
    {
        public static bool Connected { get; set; }
        public static bool IsLogData { get; set; }
    }
    internal static class flow_meter1
    {
        public static string label_transfer { get; set; }
        public static string label_sumber{ get; set; }
    }
}
