using EcoHuellaApp.Domain.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoHuellaApp.Data
{
    public class AppDatabase
    {
        private readonly SQLiteAsyncConnection _connection;

        public AppDatabase(string _dbPath)
        {
            try
            {
                _connection = new SQLiteAsyncConnection(_dbPath);

                _connection.CreateTableAsync<Casa>().Wait();
                _connection.CreateTableAsync<PuntoRecoleccion>().Wait();
                _connection.CreateTableAsync<Recoleccion>().Wait();
                _connection.CreateTableAsync<Compostaje>().Wait();
            }
            catch (Exception ex)
            {

                throw new Exception("Error:" + ex);
            }
        }

        //get
        public SQLiteAsyncConnection Database
        {
            get
            {
                return _connection;
            }
        }

    }
}
