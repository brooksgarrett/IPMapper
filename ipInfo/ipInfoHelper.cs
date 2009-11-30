/*  This class provides simple access to the http://ipinfodb.com/ API.
 *  Brooks Garrett
 *  com.brooksgarrett.com.brooksgarrett.ipInfo.ipInfoHelper
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.IO;
using System.Data.SqlClient;

namespace com.brooksgarrett.ipInfo
{
    class ipInfoHelper
    {
        private string URL = @"http://ipinfodb.com/ip_query2.php?ip=";
        private DataTable ipInformation = new DataTable("ipInformation");
        private Dictionary<string, Int32> countryHits = new Dictionary<string,Int32>();
        private DataSet ipdb = new DataSet("iplocation");
        Dictionary<string, string> dict = new Dictionary<string, string>();
        string inputPath = @"C:\ip.txt";

        public ipInfoHelper()
        {
            // Constructor
            createTables();
        }

        private List<string> buildRequests(List<string> ipAddressList)
        {
            /* First, sorts the list and creates a new copy with only distinct IP's (No redundant requests.
             * Next, it breaks the list of IP's into groups of 25 (Hard limit of API) and build the URL strings to be returned.
             */

            StringBuilder requestBuilder = new StringBuilder(URL);
            List<string> requestList = new List<string>();

            // Clean up the List
            ipAddressList.Sort();
            ipAddressList = ipAddressList.Distinct<string>().ToList();

                for (int level = 0; level < ipAddressList.Count / 25; level++)
                {
                    for (int i = 0; i < 25; i++)
                    {
                        requestBuilder.Append(ipAddressList[i + (level * 25)] + ",");
                    }
                    requestBuilder.Remove(requestBuilder.Length - 1, 1);
                    requestList.Add(requestBuilder.ToString());
                    requestBuilder = new StringBuilder(@"http://ipinfodb.com/ip_query2.php?ip=");
                }
                if (ipAddressList.Count % 25 != 0)
                {
                    for (int i = ipAddressList.Count % 25; i > 0; i--)
                    {
                        requestBuilder.Append(ipAddressList[ipAddressList.Count - i] + ",");
                    }
                    requestBuilder.Remove(requestBuilder.Length - 1, 1);
                    requestList.Add(requestBuilder.ToString());
                }
            return requestList;
        }
        private void createTables()
        {
            // A simple initialization method. Creates the DataTable to store results.
            ipInformation.Columns.Add("IP", typeof(string));
            ipInformation.Columns.Add("Status", typeof(string));
            ipInformation.Columns.Add("CountryCode", typeof(string));
            ipInformation.Columns.Add("CountryName", typeof(string));
            ipInformation.Columns.Add("RegionCode", typeof(string));
            ipInformation.Columns.Add("RegionName", typeof(string));
            ipInformation.Columns.Add("City", typeof(string));
            ipInformation.Columns.Add("ZipPostalCode", typeof(string));
            ipInformation.Columns.Add("Latitude", typeof(string));
            ipInformation.Columns.Add("Longitude", typeof(string));
            ipInformation.Columns.Add("TimeZone", typeof(string));
            ipInformation.Columns.Add("Gmtoffset", typeof(string));
            ipInformation.Columns.Add("Dstoffset", typeof(string));
        }

        public void getInfo(List<string> requestList)
        {
            /* The bread and butter. This method calls buildRequests(...) to break the list of IP's into groups of 25 with distinct IP's. 
             */
             
            requestList = buildRequests(requestList);
            XmlDocument doc = new XmlDocument();
            string temp = String.Empty;

            foreach (string requestURL in requestList)
            {
                doc.Load(requestURL);

                // Step 3: Parse through the returned results and create the array

                foreach (XmlNode currentNode in doc.GetElementsByTagName("Location"))
                {
                    foreach (XmlNode valueNode in currentNode.ChildNodes)
                    {
                        dict.Add(valueNode.LocalName.ToString(), valueNode.InnerText);
                    }

                    ipInformation.Rows.Add(dict["Ip"], dict["Status"], dict["CountryCode"], dict["CountryName"], dict["RegionCode"],
                        dict["RegionName"], dict["City"], dict["ZipPostalCode"], dict["Latitude"], dict["Longitude"], 
                        dict["Timezone"], dict["Gmtoffset"], dict["Dstoffset"]);
                    if (countryHits.ContainsKey(dict["RegionName"]))
                    {
                        countryHits[dict["RegionName"]]++;
                    }
                    else if (dict["RegionName"] != "")
                    {
                        countryHits.Add(dict["RegionName"], 1);
                    }
                    dict.Clear();
                }

            }

        }
        public DataTable getipInformation()
        {
            /* A simple Getter method to return the private DataTable holding all the results from ipinfodb. 
             * getInfo(...) must have been run or this DataTable will be empty.
             */

            return ipInformation;
        }
        public XmlDocument generateKML()
        {
            /*  I STILL don't fully understand XML in C#, but this seems to work. The color of the push pin is still not changing though.
             *  I build a document, then attach the elements in order with appendChild.
             */

            XmlDocument kml = new XmlDocument();
            XmlNode declarationNode= null;
            XmlElement kmlElem = null;
            XmlElement placemarkElem = null;
            XmlElement documentElem = null;
            XmlElement nameElem = null;
            XmlText nameText = null;
            XmlElement iconStyleElem = null;
            XmlElement colorElem = null;
            XmlElement pointElem = null;

            XmlElement coordinatesElem = null;
            XmlText coordinatesText = null;

            declarationNode = kml.CreateNode(XmlNodeType.XmlDeclaration, "", "");
            kml.AppendChild(declarationNode);
            kmlElem = kml.CreateElement("kml");
            kmlElem.SetAttribute("xmlns", @"http://www.opengis.net/kml/2.2");
            
            kml.AppendChild(kmlElem);
            documentElem = kml.CreateElement("Document");
            kmlElem.AppendChild(documentElem);

            foreach (DataRow row in ipInformation.AsEnumerable())
            {
                placemarkElem = kml.CreateElement("Placemark");
                nameElem = kml.CreateElement("name");
                nameText = kml.CreateTextNode(row["IP"].ToString());
                pointElem = kml.CreateElement("Point");
                coordinatesElem = kml.CreateElement("coordinates");
                coordinatesText = kml.CreateTextNode(row["Longitude"].ToString() + "," + row["Latitude"].ToString() + ", 0");
                iconStyleElem = kml.CreateElement("IconStyle");
                colorElem = kml.CreateElement("Color");
                colorElem.InnerText = "FFFF0000";

                documentElem.AppendChild(placemarkElem);
                placemarkElem.AppendChild(nameElem);
                iconStyleElem.AppendChild(colorElem);
                nameElem.AppendChild(nameText);
                placemarkElem.AppendChild(pointElem);
                pointElem.AppendChild(coordinatesElem);
                pointElem.AppendChild(iconStyleElem);
                coordinatesElem.AppendChild(coordinatesText);
              
            }

            kml.Save(@"C:\Brooks.kml");

            return kml;
        }
        public string generateGChartByCont()
        {
            /* This is no where near complete. I want to group the information from ipinfodb by hit counters grouped by country. 
             * Then, I want to use this information to generate a map showing hot spots in real time.
             * http://code.google.com/apis/chart/types.html#maps
             */

            string api = @"http://chart.apis.google.com/chart?cht=t&chtm=usa&chs=440x220&chf=bg,s,EAF7FE&chco=FFFFFF,FF0000,FFFF00,00FF00";
            //&chld=GAALAKFLTN&chd=t:0,100,50,32,60
            string chld = "&chld=";
            string chd = "&chd=t:";
            Int64 sum = 0;
            // Build the data string and value string

            foreach (string key in countryHits.Keys)
            {
                chld += key;
                sum += countryHits[key];
            }

            foreach (string key in countryHits.Keys)
            {
                chd += countryHits[key] / sum;
            }


            api += chld;
            api += chd;
            
            

            return api;
        }

        public UInt32 hashIP(string ip)
        {
            try
            {
                string[] octets = ip.Split('.');
                UInt32 temp = ((Convert.ToUInt32(octets[0]) * 256 + Convert.ToUInt32(octets[1])) * 256 +
                    Convert.ToUInt32(octets[2])) * 256 + Convert.ToUInt32(octets[3]);
                return temp;
            }
            catch (Exception e)
            {
                throw new Exception("The ip address is invalid", e);
            }
        }

        public string reverseHashIP(UInt32 ip)
        {
            UInt32 D, C, B, A;
            try
            {
                D = ip % 256;
                C = ((ip - D) / 256) % 256;
                B = ((((ip - D) / 256) - C) / 256) % 256;
                A = (((((ip - D) / 256) - C) / 256) - B) / 256;
                return (A.ToString() + "." + B.ToString() + "." + C.ToString() + "." + D.ToString());
            }
            catch (Exception e)
            {
                throw new Exception("The hash provided is invalid.", e);
            }
        }


    }
}
