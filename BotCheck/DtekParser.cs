using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Chrome;

namespace BotCheck
{
    public class DtekParser:IDisposable
    {
        
        string url = "https://tu-koe.dtek-kem.info";
        //ChromeDriver drive;
        EdgeDriver drive;
        public DtekParser()
        {
            var service = EdgeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            var options = new EdgeOptions();
            drive = new EdgeDriver(service,options);
            drive.Manage().Window.Minimize();
            drive.Navigate().GoToUrl(url);

        }
        public void Authorize(string Pass)
        {
            drive.Url = url;
            drive.FindElement(By.Id("proposal")).SendKeys(Pass);
            drive.FindElement(By.Id("password")).SendKeys(Pass);
            drive.FindElement(By.ClassName("button-dtek")).Click();
        }
        public string GetName()
        {
            return drive.FindElements(By.ClassName("row-content"))[10].Text;
        }
        string GetAdress()
        {
            return drive.FindElements(By.ClassName("row-content"))[5].Text;
        }
        
        string GetStatus()
        {
            try
            {
                string info;
                do
                {
                    drive.FindElements(By.ClassName("bread-crumbs-item"))[1].Click();
                    var table = drive.FindElement(By.XPath("/html/body/div[2]/div[4]"));
                    var tst = table.Text;
                    var htmlOfTable = table.GetAttribute("innerHTML");
                    info = htmlOfTable.Split("\r\n").ToList().Find(x => x.Contains("<div class=\"status-item current\">"));
                } while (info == null);
                return string.Join(" ", Regex.Matches(info, @"([А-яєі]{1,})").Select(x => { return x.Value; }));
            }
            catch (Exception ex)
            {
                Console.Beep();
                return ex.Message;
            }

        }
        public string GetInfo()
        {
            return String.Join(" ", GetName(), GetAdress(), GetStatus());
        }

        public void Dispose()
        {
            drive.Close();
        }
    }
}
