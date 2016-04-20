using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DatabaseBackup.Presentation
{
    public class ContextObject
    {
        public string Address { get; set; }

        public ObservableCollection<string> DatabaseNameCollection
        {
            get;
            set;
        }

        public List<string> Loading
        {
            get
            {
                return new List<string> { "Loading", };
            }
        }

        public string Password { get; set; }
        public string Username { get; set; }

        public ContextObject()
        {
        }

        public ContextObject(string address) : this()
        {
            this.Address = address;
            try
            {
                this.DatabaseNameCollection = new ObservableCollection<string>(LogicKeeper.logic.ShowDatabasesLocalInstance(this.Address));
            }
            catch (SqlException)
            {
                return;
            }
        }

        public ContextObject(string address, string username, string password) : this(address)
        {
            this.Username = username;
            this.Password = password;
            try
            {
                this.DatabaseNameCollection = new ObservableCollection<string>(LogicKeeper.logic.ShowDatabases(this.Address, this.Username, this.Password));
            }
            catch (SqlException)
            {
                return;
            }
        }
    }
}