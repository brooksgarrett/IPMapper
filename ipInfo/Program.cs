using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace com.brooksgarrett.ipInfo
{
    class Program
    {
        static ipInfoHelper helper = new ipInfoHelper();

        static void Main(string[] args)
        {
            testMap();
        }

        static void runGUI()
        {
            MasterForm mf = new MasterForm();
            mf.ShowDialog();
        }

        static void testHash()
        {
            string input = String.Empty;
            Regex validIP = new Regex(@"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)");
            
            while (true)
            {
                input = Console.ReadLine();
                if (validIP.Match(input).Success)
                {
                    Console.WriteLine(helper.hashIP(input));
                } else 
                {
                    try 
                    {
                        Console.WriteLine(helper.reverseHashIP(Convert.ToUInt32(input)));
                    } catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine("Please only enter a valid IP address or hash.");
                    }
                }
            }
        }

        static void testMap()
        {
            string fileName = @"C:\Documents and Settings\e10674\Desktop\ip.txt";
            string line;
            List<string> ipList = new List<string>();
            StreamReader input = new StreamReader(File.OpenRead(fileName));
            while ((line = input.ReadLine()) != null)
            {
                ipList.Add(line);
            }

            
            helper.getInfo(ipList);
            Console.WriteLine(helper.generateGChartByCont());
            Console.ReadKey();
        }
    }
}
