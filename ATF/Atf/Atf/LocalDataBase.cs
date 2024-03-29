﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data.Odbc;
using System.Globalization;
using System.Windows.Forms;

namespace Ming.Atf
{
    public static class LocalDataBase
    {
        #region Champs
        /*private static string MyConString = "DRIVER={MySQL ODBC 3.51 Driver};" +
                                     "SERVER=82.224.48.10;" +
                                     "DATABASE=velib;" +
                                     "UID=userCsharp;" +
                                     "PASSWORD=userPass11;" +
                                     "OPTION=3";  */

       private static string MyConString = "DRIVER={MySQL ODBC 3.51 Driver};" +
                                     "Port=33061;" +
                                     "SERVER=velib.lip6.fr;" +
                                     "DATABASE=velib;" +
                                     "UID=velib;" +
                                     "PASSWORD=j5G3dXa4fhND94Yr;";

        // City
        private static string City = "";

        // Initiale Date
        private static DateTime time = new DateTime(1970, 1, 1);

        // Connection to the ODBC
        private static OdbcConnection MyConnection;

        // Connection is available
        private static bool connection = false;

        // Taille pour les stations
        public static Dictionary<int, int> tailles = null;

        // Retourne hour si a deja etait calcule
        public static Dictionary<int, Dictionary<int, KeyValuePair<double, double>>> hour = null;

        // Retourne hour si a deja etait calcule
        public static Dictionary<int, Dictionary<int, KeyValuePair<double, double>>> day = null;

        // Retourne hour si a deja etait calcule
        public static Dictionary<int, Dictionary<int, KeyValuePair<double, double>>> week = null;

        //Représente chaque station sous forme d'histogramme avec la moyenne du taux de disponibilité par semaine
        public static Dictionary<int, Dictionary<int, KeyValuePair<double, double>>> statsTabSemaine = null;

        //Représente chaque station sous forme d'histogramme avec la moyenne du taux de disponibilité par Heure
        public static Dictionary<int, Dictionary<int, KeyValuePair<double, double>>> statsTabHeure = null;

        //Représente chaque station sous forme d'histogramme avec la moyenne du taux de disponibilité par Jour
        public static Dictionary<int, Dictionary<int, KeyValuePair<double, double>>> statsTabJour = null;

        //Représente chaque station sous forme d'histogramme avec la moyenne du taux de disponibilité par Heure
        public static Dictionary<int, Dictionary<int, KeyValuePair<double, double>>> statsTabHeureOuvre = null;
        
        #endregion

        #region Methodes
      
        // Renvoie le details des stations
        public static ArrayList getStationsDetails()
        {
            ArrayList result = new ArrayList();

            if (!connection)
            {
                setConnection();
                if (!connection)
                    return result;
            }

            //Desc de la table donnees
            OdbcCommand MyCommand = new OdbcCommand("desc stations;", MyConnection);
            OdbcDataReader MyDataReader;
            MyDataReader = MyCommand.ExecuteReader();
            ArrayList header = new ArrayList();

            while (MyDataReader.Read())
                if (string.Compare(MyConnection.Driver, "myodbc3.dll") == 0)
                    header.Add(MyDataReader.GetString(0)); //Supported only by MyODBC 3.5

            //Fetch
            MyCommand.CommandText = "select * from stations;";

            MyDataReader.Close();
            MyDataReader = MyCommand.ExecuteReader();

            Console.WriteLine("Executed : " + MyDataReader.RecordsAffected);

            while (MyDataReader.Read())
            {
                Dictionary<string, string> temp = new Dictionary<string, string>();
                if (string.Compare(MyConnection.Driver, "myodbc3.dll") == 0)
                    for (int i = 0; i < header.Count; i++)
                        if (MyDataReader.GetString(i) != "")
                            temp.Add(header[i] as string, MyDataReader.GetString(i));
                result.Add(temp);
            }

            //Close all resources
            MyDataReader.Close();
            //MyConnection.Close();

            return result;
        }

        // Renvoie le details des POIs
        public static ArrayList getPOIsDetails()
        {
            ArrayList result = new ArrayList();

            if (!connection)
            {
                setConnection();
                if (!connection)
                    return result;
            }

            //Desc de la table donnees
            OdbcCommand MyCommand = new OdbcCommand("desc pois;", MyConnection);
            OdbcDataReader MyDataReader;
            MyDataReader = MyCommand.ExecuteReader();
            ArrayList header = new ArrayList();

            while (MyDataReader.Read())
                if (string.Compare(MyConnection.Driver, "myodbc3.dll") == 0)
                    header.Add(MyDataReader.GetString(0)); //Supported only by MyODBC 3.5

            //Fetch
            MyCommand.CommandText = "select * from pois;";

            MyDataReader.Close();
            MyDataReader = MyCommand.ExecuteReader();

            Console.WriteLine("Executed : " + MyDataReader.RecordsAffected);

            while (MyDataReader.Read())
            {
                Dictionary<string, string> temp = new Dictionary<string, string>();
                if (string.Compare(MyConnection.Driver, "myodbc3.dll") == 0)
                    for (int i = 0; i < header.Count; i++)
                        if (MyDataReader.GetString(i) != "")
                            temp.Add(header[i] as string, MyDataReader.GetString(i));
                result.Add(temp);
            }

            //Close all resources
            MyDataReader.Close();
            //MyConnection.Close();

            return result;
        }

        // Initialize the connection to the DB
        private static void setConnection()
        {
            try
            {
                //Connect to MySQL using MyODBC
                MyConnection = new OdbcConnection(MyConString);
                MyConnection.Open();
                connection = true;

                tailles = new Dictionary<int, int>();

                OdbcCommand MyCommand = new OdbcCommand("select id,max(total) from stations group by id;", MyConnection);
                //OdbcCommand MyCommand = new OdbcCommand( "select station, max(free+available) from donnees where free !=\"\" group by station;", MyConnection );
                OdbcDataReader MyDataReader;
                MyDataReader = MyCommand.ExecuteReader();

                Console.WriteLine("Executed : " + MyDataReader.RecordsAffected);
                while (MyDataReader.Read())
                {
                    int valeur = 0;
                    int station = MyDataReader.GetInt32(0);
                    //int hour = MyDataReader.GetInt32(1);

                    try
                    {
                        valeur = MyDataReader.GetInt32(1);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Pas de base : " + e.Message);
                        continue;
                    }

                    tailles.Add(station, valeur);
                }

                //Close all resources
                MyDataReader.Close();

            }
            catch (OdbcException MyOdbcException)//Catch any ODBC exception ..
            {
                connection = false;
                for (int i = 0; i < MyOdbcException.Errors.Count; i++)
                {
                    MessageBox.Show("Aucune connection à la base de données n'a pu être établie");
                    Console.Write("ERROR #" + i + "\n" +
                      "Message: " + MyOdbcException.Errors[i].Message + "\n" +
                      "Native: " + MyOdbcException.Errors[i].NativeError.ToString() + "\n" +
                      "Source: " + MyOdbcException.Errors[i].Source + "\n" +
                      "SQL: " + MyOdbcException.Errors[i].SQLState + "\n");
                }
            }
        }

        // Effectue les requetes
        private static Dictionary<int, Dictionary<int, KeyValuePair<double, double>>> sendRequest(String query, int select)
        {
            Dictionary<int, Dictionary<int, KeyValuePair<double, double>>> result = new Dictionary<int, Dictionary<int, KeyValuePair<double, double>>>();

            if (!connection)
            {
                setConnection();
                if (!connection)
                    return result;
            }

            //Console.WriteLine("\n !!! success, connected successfully !!!\n");
            ////Display connection information
            //Console.WriteLine("Connection Information:");
            //Console.WriteLine("\tConnection String:" + MyConnection.ConnectionString);
            //Console.WriteLine("\tConnection Timeout:" + MyConnection.ConnectionTimeout);
            //Console.WriteLine("\tDatabase:" + MyConnection.Database);
            //Console.WriteLine("\tDataSource:" + MyConnection.DataSource);
            //Console.WriteLine("\tDriver:" + MyConnection.Driver);
            //Console.WriteLine("\tServerVersion:" + MyConnection.ServerVersion);

            //Desc de la table donnees
            OdbcCommand MyCommand = new OdbcCommand(query, MyConnection);
            OdbcDataReader MyDataReader;
            MyDataReader = MyCommand.ExecuteReader();

            Console.WriteLine("Executed : " + MyDataReader.RecordsAffected);
            while (MyDataReader.Read())
            {
                KeyValuePair<double, double> temp1 = new KeyValuePair<double, double>(MyDataReader.GetDouble(1), MyDataReader.GetDouble(2));
                Dictionary<int, KeyValuePair<double, double>> temp = new Dictionary<int, KeyValuePair<double, double>>();
                temp[select] = temp1;
                result[MyDataReader.GetInt32(0)] = temp;
                //result.Add(MyDataReader.GetInt32(0), temp);
            }

            //Close all resources
            MyDataReader.Close();
            //MyConnection.Close();

            return result;
        }

        // Retourne le total par station
        public static Dictionary<int,double> getMoyenne()
        {
            Dictionary<int, double> result = new Dictionary<int, double>();

            if (!connection)
            {
                setConnection();
                if (!connection)
                    return result;
            }

            //Desc de la table donnees
            OdbcCommand MyCommand = new OdbcCommand("select station, avg(available + free) from donnees where valid='1' group by station;", MyConnection);
            OdbcDataReader MyDataReader;
            MyDataReader = MyCommand.ExecuteReader();

            Console.WriteLine("Executed : " + MyDataReader.RecordsAffected);
            while (MyDataReader.Read())
            {
                result[MyDataReader.GetInt32(0)] = MyDataReader.GetDouble(1);
            }

            //Close all resources
            MyDataReader.Close();

            return result;
        }

        // Retourne le Remplissage par station par Heure (sur toute la periode de recolte)
        public static Dictionary<int, Dictionary<int,double>> getRemplissageByHour(DateTime start, DateTime end)
        {
            Dictionary<int, Dictionary<int, double>> result = new Dictionary<int, Dictionary<int, double>>();

            if (!connection)
            {
                setConnection();
                if (!connection)
                    return result;
            }

            //Desc de la table donnees
            string s = " and date >= " + convertToTimestamp(start) + " and date <= " + convertToTimestamp(end) + " ";
            OdbcCommand MyCommand = new OdbcCommand("select station as Station, hour as Hour, cast(avg(available) * 100 as unsigned) as Valeur from donnees where valid='1' and free!=\"\" " + s + "group by station, hour;", MyConnection);
            OdbcDataReader MyDataReader;
            MyDataReader = MyCommand.ExecuteReader();

            Console.WriteLine("Executed : " + MyDataReader.RecordsAffected);
            while (MyDataReader.Read())
            {
                int valeur = 0;
                int station = MyDataReader.GetInt32(0);
                int hour = MyDataReader.GetInt32(1);

                try
                {
                    valeur = MyDataReader.GetInt32(2);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error : " + e.Message);
                    continue;
                }

                if (!result.ContainsKey(station))
                    result[station] = new Dictionary<int, double>();

                result[station][hour] = valeur / 100.0;
                result[station][hour] = result[station][hour] / (double)tailles[station];
            }

            //Close all resources
            MyDataReader.Close();

            return result;
        }

        // Retourne la variance par station par Heure (sur toute la periode de recolte)
        public static Dictionary<int, Dictionary<int, double>> getVarianceByHour(DateTime start, DateTime end)
        {
            Dictionary<int, Dictionary<int, double>> result = new Dictionary<int, Dictionary<int, double>>();

            if (!connection)
            {
                setConnection();
                if (!connection)
                    return result;
            }

            //Desc de la table donnees
            string s = " and date >= " + convertToTimestamp(start) + " and date <= " + convertToTimestamp(end) + " ";
            OdbcCommand MyCommand = new OdbcCommand("select station as Station, hour as Hour, cast(std(available) * 100 as unsigned) as Valeur from donnees where valid='1' and free!=\"\" " + s + "group by station, hour;", MyConnection);
            OdbcDataReader MyDataReader;
            MyDataReader = MyCommand.ExecuteReader();

            Console.WriteLine("Executed : " + MyDataReader.RecordsAffected);
            while (MyDataReader.Read())
            {
                int valeur = 0;
                int station = MyDataReader.GetInt32(0);
                int hour = MyDataReader.GetInt32(1);

                try
                {
                    valeur = MyDataReader.GetInt32(2);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error : " + e.Message);
                    continue;
                }

                if (!result.ContainsKey(station))
                    result[station] = new Dictionary<int, double>();

                result[station][hour] = valeur / 100.0;
                result[station][hour] = result[station][hour] / (double)tailles[station];
            }

            //Close all resources
            MyDataReader.Close();

            return result;
        }

        // Retourne les vecteurs par heures pour les jours ouvres
        public static Dictionary<int, Dictionary<int, double>> getRemplissageByHourOuvres(DateTime start, DateTime end)
        {
            Dictionary<int, Dictionary<int, double>> result = new Dictionary<int, Dictionary<int, double>>();

            if (!connection)
            {
                setConnection();
                if (!connection)
                    return result;
            }

            //Desc de la table donnees
            string s = " and date >= " + convertToTimestamp(start) + " and date <= " + convertToTimestamp(end) + " ";
            s += "and day < 6 ";
            OdbcCommand MyCommand = new OdbcCommand("select station as Station, hour as Hour, cast(avg(available) * 100 as unsigned) as Valeur from donnees where valid='1' and free!=\"\" " + s + "group by station, hour;", MyConnection);
            OdbcDataReader MyDataReader;
            MyDataReader = MyCommand.ExecuteReader();

            Console.WriteLine("Executed : " + MyDataReader.RecordsAffected);
            while (MyDataReader.Read())
            {
                int valeur = 0;
                int station = MyDataReader.GetInt32(0);
                int hour = MyDataReader.GetInt32(1);

                try
                {
                    valeur = MyDataReader.GetInt32(2);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error : " + e.Message);
                    continue;
                }

                if (!result.ContainsKey(station))
                    result[station] = new Dictionary<int, double>();

                result[station][hour] = valeur / 100.0;
                result[station][hour] = result[station][hour] / (double)tailles[station];
            }

            //Close all resources
            MyDataReader.Close();

            return result;
        }

        // Retourne les vecteurs par demie-heure pour les jours ouvres
        public static Dictionary<int, Dictionary<int, double>> getRemplissageByHalfHour(DateTime start, DateTime end)
        {
            Dictionary<int, Dictionary<int, double>> result = new Dictionary<int, Dictionary<int, double>>();

            if (!connection)
            {
                setConnection();
                if (!connection)
                    return result;
            }

            //Desc de la table donnees
            string s = " and date >= " + convertToTimestamp(start) + " and date <= " + convertToTimestamp(end) + " ";
            s += "and minute < 30 ";
            OdbcCommand MyCommand = new OdbcCommand("select station as Station, hour as Hour, cast(avg(available) * 100 as unsigned) as Valeur from donnees where valid='1' and free!=\"\" " + s + "group by station, hour;", MyConnection);
            OdbcDataReader MyDataReader;
            MyDataReader = MyCommand.ExecuteReader();

            Console.WriteLine("Executed : " + MyDataReader.RecordsAffected);
            while (MyDataReader.Read())
            {
                int valeur = 0;
                int station = MyDataReader.GetInt32(0);
                int hour = MyDataReader.GetInt32(1);

                try
                {
                    valeur = MyDataReader.GetInt32(2);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error : " + e.Message);
                    continue;
                }

                if (!result.ContainsKey(station))
                    result[station] = new Dictionary<int, double>();

                result[station][hour] = valeur / 100.0;
                result[station][hour] = result[station][hour] / (double)tailles[station];
            }

            MyDataReader.Close();

            s = " and date >= " + convertToTimestamp(start) + " and date <= " + convertToTimestamp(end) + " ";
            s += "and minute > 30 ";
            
            MyCommand.CommandText = "select station as Station, hour as Hour, cast(avg(available) * 100 as unsigned) as Valeur from donnees where valid='1' and free!=\"\" " + s + "group by station, hour;";
            MyDataReader = MyCommand.ExecuteReader();

            Console.WriteLine("Executed : " + MyDataReader.RecordsAffected);
            while (MyDataReader.Read())
            {
                int valeur = 0;
                int station = MyDataReader.GetInt32(0);
                int hour = MyDataReader.GetInt32(1);

                try
                {
                    valeur = MyDataReader.GetInt32(2);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error : " + e.Message);
                    continue;
                }

                if (!result.ContainsKey(station))
                    result[station] = new Dictionary<int, double>();

                result[station][hour + 24] = valeur / 100.0;
                result[station][hour + 24] = result[station][hour + 24] / (double)tailles[station];
            }

            //Close all resources
            MyDataReader.Close();

            return result;
        }

        // Retourne les vecteurs pour le week-end
        public static Dictionary<int, Dictionary<int, double>> getRemplissageByHourWE(DateTime start, DateTime end)
        {
            Dictionary<int, Dictionary<int, double>> result = new Dictionary<int, Dictionary<int, double>>();

            if (!connection)
            {
                setConnection();
                if (!connection)
                    return result;
            }

            //Desc de la table donnees
            string s = " and date >= " + convertToTimestamp(start) + " and date <= " + convertToTimestamp(end) + " ";
            s += "and day >= 6 ";
            OdbcCommand MyCommand = new OdbcCommand("select station as Station, hour as Hour, cast(avg(available) * 100 as unsigned) as Valeur from donnees where valid='1' and free!=\"\" " + s + "group by station, hour;", MyConnection);
            OdbcDataReader MyDataReader;
            MyDataReader = MyCommand.ExecuteReader();

            Console.WriteLine("Executed : " + MyDataReader.RecordsAffected);
            while (MyDataReader.Read())
            {
                int valeur = 0;
                int station = MyDataReader.GetInt32(0);
                int hour = MyDataReader.GetInt32(1);

                try
                {
                    valeur = MyDataReader.GetInt32(2);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error : " + e.Message);
                    continue;
                }

                if (!result.ContainsKey(station))
                    result[station] = new Dictionary<int, double>();

                result[station][hour] = valeur / 100.0;
                result[station][hour] = result[station][hour] / (double)tailles[station];
            }

            //Close all resources
            MyDataReader.Close();

            return result;
        }

        // Retourne les vecteurs par jour
        public static Dictionary<int, Dictionary<int, double>> getRemplissageByDay(DateTime start, DateTime end)
        {
            Dictionary<int, Dictionary<int, double>> result = new Dictionary<int, Dictionary<int, double>>();

            if (!connection)
            {
                setConnection();
                if (!connection)
                    return result;
            }

            //Desc de la table donnees
            string s = " and date >= " + convertToTimestamp(start) + " and date <= " + convertToTimestamp(end) + " ";
            OdbcCommand MyCommand = new OdbcCommand("select station as Station, day as Day, cast(avg(available) * 100 as unsigned) as Valeur from donnees where valid='1' and free!=\"\" " + s + "group by station, day;", MyConnection);
            OdbcDataReader MyDataReader;
            MyDataReader = MyCommand.ExecuteReader();

            Console.WriteLine("Executed : " + MyDataReader.RecordsAffected);
            while (MyDataReader.Read())
            {
                int valeur = 0;
                int station = MyDataReader.GetInt32(0);
                int hour = MyDataReader.GetInt32(1);

                try
                {
                    valeur = MyDataReader.GetInt32(2);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error : " + e.Message);
                    continue;
                }

                if (!result.ContainsKey(station))
                    result[station] = new Dictionary<int, double>();

                result[station][hour] = valeur / 100.0;
                result[station][hour] = result[station][hour] / (double)tailles[station];
            }

            //Close all resources
            MyDataReader.Close();

            return result;
        }

        // Retourne toute la base
        public static Dictionary<int, ArrayList> getAllToFile()
        {
            Dictionary<int, ArrayList> result = new Dictionary<int, ArrayList>();

            if (!connection)
            {
                setConnection();
                if (!connection)
                    return result;
            }

            //Desc de la table donnees
            OdbcCommand MyCommand = new OdbcCommand("select station, available from donnees where valid='1' and free!=\"\";", MyConnection);
            OdbcDataReader MyDataReader;
            MyDataReader = MyCommand.ExecuteReader();

            Console.WriteLine("Executed : " + MyDataReader.RecordsAffected);
            while (MyDataReader.Read())
            {
                int station = MyDataReader.GetInt32(0);
                int hour = MyDataReader.GetInt32(1);

                if (!result.ContainsKey(station))
                    result[station] = new ArrayList();

                result[station].Add(hour);
            }

            //Close all resources
            MyDataReader.Close();

            return result;
        }

        // Transforme une DateTime en Timestamp
        private static double convertToTimestamp(DateTime value)
        {
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            return (double)span.TotalSeconds;
        }

        // Modifie la ville
        // Vide = Paris.
        // Le nom de la ville doit commencer par une MAJUSCULE
        public static void setCity(string _city)
        {
            City = _city;
        }
        #endregion

        #region DataSet

      // Retourne le Remplissage par station par Heure (sur toute la periode de recolte)
      public static void getRemplissageByHourHisto(  DateTime start, DateTime end ) {
        Dictionary<int, Dictionary<int, KeyValuePair<double, double>>> result = new Dictionary<int, Dictionary<int, KeyValuePair<double, double>>>();

        if ( !connection ) {
          setConnection();
        }

        //Desc de la table donnees
        string s = " and date >= " + convertToTimestamp( start ) + " and date <= " + convertToTimestamp( end ) + " ";
        OdbcCommand MyCommand = new OdbcCommand( "select station as Station, hour as Hour, cast(avg(available) * 100 as unsigned) as Valeur,cast(std(available) * 100 as unsigned) as Variance from donnees where valid='1' and free!=\"\" " + s + "group by station, hour;", MyConnection );
        OdbcDataReader MyDataReader;
        MyDataReader = MyCommand.ExecuteReader();

        Console.WriteLine( "Executed : " + MyDataReader.RecordsAffected );
        while ( MyDataReader.Read() ) {
          int valeur = 0;
          int station = MyDataReader.GetInt32( 0 );
          int hour = MyDataReader.GetInt32( 1 );
          int variance = 0;
          try {
            valeur = MyDataReader.GetInt32( 2 );
          }
          catch ( Exception e ) {
            Console.WriteLine( "Error : " + e.Message );
            continue;
          }

          try {
            variance = MyDataReader.GetInt32( 3 );
          }
          catch ( Exception e ) {
            Console.WriteLine( "Error : " + e.Message );
            continue;
          }

          if ( !result.ContainsKey( station ) )
            result[ station ] = new Dictionary<int, KeyValuePair<double, double>>();

          result[ station ][ hour ] = new KeyValuePair<double, double>( (valeur / 100.0) / (double) tailles[ station ], ((variance/100.0) / (double) tailles[ station ]));
        }

        //Close all resources
        MyDataReader.Close();
        statsTabHeure = result;
      }

      // Retourne les vecteurs pour les jours ouvres
      public static void getRemplissageByHourOuvresHisto( DateTime start, DateTime end ) {
        Dictionary<int, Dictionary<int, KeyValuePair<double, double>>> result = new Dictionary<int, Dictionary<int, KeyValuePair<double, double>>>();

        if ( !connection ) {
          setConnection();
        }

        //Desc de la table donnees
        string s = " and date >= " + convertToTimestamp( start ) + " and date <= " + convertToTimestamp( end ) + " ";
        s += "and day < 6 ";
        OdbcCommand MyCommand = new OdbcCommand( "select station as Station, hour as Hour, cast(avg(available) * 100 as unsigned) as Valeur, cast(std(available) * 100 as unsigned)  as Variance from donnees where valid='1' and free!=\"\" " + s + "group by station, hour;", MyConnection );
        OdbcDataReader MyDataReader;
        MyDataReader = MyCommand.ExecuteReader();

        Console.WriteLine( "Executed : " + MyDataReader.RecordsAffected );
        while ( MyDataReader.Read() ) {
          int valeur = 0;
          int station = MyDataReader.GetInt32( 0 );
          int hour = MyDataReader.GetInt32( 1 );
          int variance = 0;
          try {
            valeur = MyDataReader.GetInt32( 2 );
          }
          catch ( Exception e ) {
            Console.WriteLine( "Error : " + e.Message );
            continue;
          }

          try {
            variance = MyDataReader.GetInt32( 3 );
          }
          catch ( Exception e ) {
            Console.WriteLine( "Error : " + e.Message );
            continue;
          }

          if ( !result.ContainsKey( station ) )
            result[ station ] = new Dictionary<int, KeyValuePair<double, double>>();

          result[ station ][ hour ] = new KeyValuePair<double, double>( (valeur / 100.0) / (double) tailles[ station ], (variance /100.0) / (double) tailles[ station ] );
        }

        //Close all resources
        MyDataReader.Close();
        statsTabHeureOuvre = result;
      }

      // Retourne les vecteurs pour les jours de la semaine (we inclu)
      public static void getRemplissageByDayHisto( DateTime start, DateTime end ) {
        Dictionary<int, Dictionary<int, KeyValuePair<double, double>>> result = new Dictionary<int, Dictionary<int, KeyValuePair<double, double>>>();

        if ( !connection ) {
          setConnection();
        }

        //Desc de la table donnees
        string s = " and date >= " + convertToTimestamp( start ) + " and date <= " + convertToTimestamp( end ) + " ";
        OdbcCommand MyCommand = new OdbcCommand( "select station as Station, day as Day, cast(avg(available) * 100 as unsigned) as Valeur,cast(std(available) * 100 as unsigned) as Variance from donnees where valid='1' and free!=\"\" " + s + "group by station, day;", MyConnection );
        OdbcDataReader MyDataReader;
        MyDataReader = MyCommand.ExecuteReader();

        Console.WriteLine( "Executed : " + MyDataReader.RecordsAffected );
        while ( MyDataReader.Read() ) {
          int valeur = 0;
          int station = MyDataReader.GetInt32( 0 );
          int hour = MyDataReader.GetInt32( 1 );
          int variance = 0;
          try {
            valeur = MyDataReader.GetInt32( 2 );
          }
          catch ( Exception e ) {
            Console.WriteLine( "Error : " + e.Message );
            continue;
          }

          try {
            variance = MyDataReader.GetInt32( 3 );
          }
          catch ( Exception e ) {
            Console.WriteLine( "Error : " + e.Message );
            continue;
          }

          if ( !result.ContainsKey( station ) )
            result[ station ] = new Dictionary<int, KeyValuePair<double, double>>();

          result[ station ][ hour ] = new KeyValuePair<double, double>( (valeur / 100.0) / (double) tailles[ station ], (variance / 100.0) / (double) tailles[ station ] );
        }

      
        MyDataReader.Close();
        statsTabJour = result;
      }

      #endregion
    }
}
