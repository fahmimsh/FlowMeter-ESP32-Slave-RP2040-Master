using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
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
        public static bool IsLogChart { get; set; }
    }
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class HeaderColumnAttribute : Attribute
    {
        public string HeaderText { get; }

        public HeaderColumnAttribute(string headerText)
        {
            HeaderText = headerText;
        }
    }

    internal class data_log_entry : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private int ID { get; set; }
        [HeaderColumn("ID")]
        public int id { get => ID; set { ID = value; OnPropertyChanged(nameof(id)); } }
        private string FlowMeter { get; set; }
        [HeaderColumn("Nama Flow Meter")]
        public string flow_meter { get => FlowMeter; set { FlowMeter = value; OnPropertyChanged(nameof(FlowMeter)); } }
        private string Mode { get; set; }
        [HeaderColumn("Mode")]
        public string mode { get => Mode; set { Mode = value; OnPropertyChanged(nameof(Mode)); } }
        private double Set_Liter { get; set; }
        [HeaderColumn("Set Liter")]
        public double setLiter { get => Set_Liter; set { Set_Liter = value; OnPropertyChanged(nameof(Set_Liter)); } }
        private double Liter { get; set; }
        [HeaderColumn("Hasil Liter")]
        public double liter { get => Liter; set { Liter = value; OnPropertyChanged(nameof(Liter)); } }
        private double KFactor { get; set; }
        [HeaderColumn("K-Faktor")]
        public double k_factor { get => KFactor; set { KFactor = value; OnPropertyChanged(nameof(KFactor)); } }
        private string Batch { get; set; }
        [HeaderColumn("No Batch")]
        public string batch { get => Batch; set { Batch = value; OnPropertyChanged(nameof(Batch)); } }
        private string TransferTo { get; set; }
        [HeaderColumn("Transfer Ke")]
        public string transfer_to { get => TransferTo; set { TransferTo = value; OnPropertyChanged(nameof(TransferTo)); } }
        private DateTime Date_Time { get; set; }
        [HeaderColumn("Tanggal Dan Waktu")]
        public DateTime date_time { get => Date_Time; set { Date_Time = value; OnPropertyChanged(nameof(Date_Time)); } }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    internal class data_log_entry2 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private int ID { get; set; }
        [HeaderColumn("ID")]
        public int id { get => ID; set { ID = value; OnPropertyChanged(nameof(id)); } }
        private string Proses_mesin { get; set; }
        [HeaderColumn("Proses Mesin")]
        public string proses_mesin { get => Proses_mesin; set { Proses_mesin = value; OnPropertyChanged(nameof(Proses_mesin)); } }
        private string TransferTo { get; set; }
        [HeaderColumn("Transfer Ke MF")]
        public string transfer_to { get => TransferTo; set { TransferTo = value; OnPropertyChanged(nameof(TransferTo)); } }
        private string Batch { get; set; }
        [HeaderColumn("No Batch")]
        public string batch { get => Batch; set { Batch = value; OnPropertyChanged(nameof(Batch)); } }
        private string Produk { get; set; }
        [HeaderColumn("Produk")]
        public string produk { get => Produk; set { Produk = value; OnPropertyChanged(nameof(Produk)); } }
        private double Liter { get; set; }
        [HeaderColumn("Hasil Liter")]
        public double liter { get => Liter; set { Liter = value; OnPropertyChanged(nameof(Liter)); } }
        private double KFactor { get; set; }
        [HeaderColumn("K-Faktor")]
        public double k_factor { get => KFactor; set { KFactor = value; OnPropertyChanged(nameof(KFactor)); } }
        private DateTime Date_Time { get; set; }
        [HeaderColumn("Tanggal Dan Waktu")]
        public DateTime date_time { get => Date_Time; set { Date_Time = value; OnPropertyChanged(nameof(Date_Time)); } }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    internal static class flow_meter1
    {
        public static string label_transfer { get; set; }
        public static string label_batch{ get; set; }
    }
    internal static class flow_meter2
    {
        public static string label_transfer { get; set; }
        public static string label_batch { get; set; }
    }
    internal static class flow_meter3
    {
        public static string label_transfer { get; set; }
        public static string label_batch { get; set; }
        public static string label_proses_mesin { get; set; }
        public static string label_produk { get; set; }
    }
    internal static class flow_meter4
    {
        public static string label_transfer { get; set; }
        public static string label_batch { get; set; }
        public static string label_proses_mesin { get; set; }
        public static string label_produk { get; set; }
    }
    internal static class flow_meter5
    {
        public static string label_transfer { get; set; }
        public static string label_batch { get; set; }
        public static string label_proses_mesin { get; set; }
        public static string label_produk { get; set; }
    }
}
