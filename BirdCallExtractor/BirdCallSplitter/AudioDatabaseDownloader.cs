using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Data;
using System.Collections;
using MySql.Data.MySqlClient;
using MySql.Data;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

namespace BirdAudioAnalysis
{
	class AudioDatabaseDownloader : IAudioDatabaseDownloader
	{
		public static MySqlConnection dbConnection;
		private static string host = "featherprint.cvzffvtv4h1p.us-east-1.rds.amazonaws.com";
		private static string user = "MLBB";
		private string password;
		private static string defaultDatabase = "FeatherPrint";

		public AudioDatabaseDownloader(string password)
		{
			this.password = password;
			OpenSql();
		}

		public async Task<string[]> DownloadAudioForBird(string scientificName)
		{
			string pathString = "..\\..\\..\\DataSets\\" + scientificName + "\\";
			Directory.CreateDirectory(pathString);

			DataSet dataSet = ExecuteBirdCallQuery(scientificName);
			DataTable dataTable = dataSet.Tables[0];

			string[] audioPaths = new string[dataTable.Rows.Count];
			for (int i = 0; i < dataTable.Rows.Count; i++)
			{
				//audio is an object and must be converted to a byte array
				var audio = dataTable.Rows[i]["audio"];
				var byteArray = ObjectToByteArray(audio);

				//save mp3 file
				string audioPath = pathString + scientificName + i + ".mp3";
				audioPaths[i] = audioPath;
				File.WriteAllBytes(audioPath, byteArray);

			}
			return audioPaths;
		}

		public byte[] ObjectToByteArray(object obj)
		{
			if (obj == null)
				return null;
			BinaryFormatter bf = new BinaryFormatter();
			using (MemoryStream ms = new MemoryStream())
			{
				bf.Serialize(ms, obj);
				return ms.ToArray();
			}
		}

		public void OpenSql()
		{
			try
			{
				string connectionString = string.Format("Server = {0};port={4};Database = {1}; User ID = {2}; Password = {3};", host, defaultDatabase, user, password, "3306");
				dbConnection = new MySqlConnection(connectionString);
				dbConnection.Open();
			}
			catch (Exception e)
			{
				throw new Exception("error" + e.Message.ToString());

			}
		}

		public DataSet ExecuteBirdCallQuery(string scientificName)
		{
			string query = "select audio from audio a, information b where a.birdId = b.id && b.scientificName = \"" + scientificName + "\";";
			if (dbConnection.State == ConnectionState.Open)
			{
				DataSet ds = new DataSet();
				try
				{

					MySqlDataAdapter da = new MySqlDataAdapter(query, dbConnection);
					da.Fill(ds);

				}
				catch (Exception ee)
				{
					throw new Exception("SQL:" + query + "/n" + ee.Message.ToString());
				}
				return ds;
			}
			return null;
		}

		public void Close()
		{
			if (dbConnection != null)
			{
				dbConnection.Close();
				dbConnection.Dispose();
				dbConnection = null;
			}
		}
	}
}
