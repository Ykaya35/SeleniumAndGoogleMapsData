using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading;
using System.Windows.Forms;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Microsoft.Office.Interop.Excel;
using OpenQA.Selenium.Interactions;
using System.Threading.Tasks;

namespace GoogleMapBoot
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        Thread th;
        ChromeDriver drv;
        private string MapsUrl = "https://www.google.com/maps";

        private void Form1_Load(object sender, EventArgs e)
        {
            th = new Thread(loadUrl); th.Start();

        }
        private void loadUrl()
        {
            dataGridView1.ColumnCount = 4;
            dataGridView1.Columns[0].Name = "İsim";
            dataGridView1.Columns[1].Name = "Url";
            dataGridView1.Columns[2].Name = "Tel No";
            dataGridView1.Columns[3].Name = "Adres";
            //dataGridView1.Columns[4].Name = "işletme";

            //ChromeOptions options = new ChromeOptions();
            //options.AddArgument("--headless");
            drv = new ChromeDriver();
            drv.Manage().Window.Maximize();
            drv.Navigate().GoToUrl(MapsUrl);

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            drv.Close();
        }

        private async void button_Click(object sender, EventArgs e)
        {
            //th = new Thread(vericek); th.Start();

            await Task.Run(() =>
            {
                vericek();
            });
        }
        private void DataGridViewEkle(string isim, string Url, string tel_no, string Adres)
        {
            if (dataGridView1.InvokeRequired)
            {
                dataGridView1.Invoke((MethodInvoker)delegate
                {
                    dataGridView1.Rows.Add(isim, Url, tel_no, Adres);
                });
            }
            else
            {
                dataGridView1.Rows.Add(isim, Url, tel_no, Adres);
            }
        }


        private void vericek()
        {
            WebDriverWait wait = new WebDriverWait(drv, TimeSpan.FromSeconds(10));

            drv.FindElement(By.XPath("//*[@name='q']")).SendKeys(txt_city.Text + OpenQA.Selenium.Keys.Enter);
            Thread.Sleep(4000);
            // Düğmeyi tanımlayın (Örneğin, düğme XPath veya ID ile tanımlanabilir)
            IWebElement button = drv.FindElement(By.ClassName("D6NGZc"));

            // Düğmeye tıkla
            button.Click();
            int rowCount = 0; // Satır sayacı
            for (int i = 0; i < drv.FindElements(By.XPath("//*[@class='Nv2PK THOPZb CpccDe ']"))
                .Count; i++)
            {
                try
                {
                    drv.FindElements(By.XPath("//*[@class='Nv2PK THOPZb CpccDe ']//*[@class='hfpxzc']"))[i].Click();
                    Thread.Sleep(4000);
                }
                catch
                {
                    MessageBox.Show("İstenmeyen bir arama ya da sistemsel bir sorun oluştu. Tekrar deneyiniz.");
                }

                string isim = "";
                string Url = "";
                string tel_no = "";
                string Adres = "";
                string isletme = "";

                IWebElement elementName = drv.FindElement(By.XPath("//h1[@class='DUwDvf lfPIob']"));
                isim = elementName.Text;

                try
                {
                    IWebElement elementUrl = drv.FindElement(By.XPath("//*[@data-tooltip='Web sitesini aç']"));
                    Url = "www." + elementUrl.Text;
                }
                catch (NoSuchElementException)
                {
                    Url = "Web sitesi yok";
                }

                try
                {
                    IWebElement elementTel = drv.FindElement(By.XPath("//*[@data-tooltip='Telefon numarasını kopyala']"));
                    tel_no = elementTel.Text;
                }
                catch
                {
                    tel_no = "Telefon numarası yok";
                }

                try
                {

                    WebDriverWait await = new WebDriverWait(drv, TimeSpan.FromSeconds(10));
                    IWebElement elementAdres = wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@data-tooltip='Adresi kopyala']")));
                    Adres = elementAdres.Text;
                }
                catch
                {
                    Adres = "Adres Yok";
                }


                //IWebElement elementisletme = drv.FindElement(By.XPath("//*[@class='DkEaL']"));
        
                //isletme = elementisletme.Text;
                DataGridViewEkle(isim, Url, tel_no, Adres);
                rowCount++; // Satır sayacını artır

                if (rowCount % 18 == 0)
                {
                    Thread.Sleep(5000); // Her 18 satırda 5 saniye beklemek için
                    scrollCountGeri();
                }

                scrollCountGo();

            }

            MessageBox.Show("Veriler Çekildi");

        }

        private void UpdateUI(int progress)
        {
            // İşlem ilerlemesini GUI üzerinde güncelle
            labelProgress.Text = "İlerleme: " + progress + "%";
        }

        private void scrollCountGeri()
        {
            var scrollOrigin = new WheelInputDevice.ScrollOrigin
            {
                Viewport = true,
                XOffset = 237,
                YOffset = 499
            };
            new OpenQA.Selenium.Interactions.Actions(drv)
                .ScrollFromOrigin(scrollOrigin, 0, -300)
                .Perform();
        }

        private void scrollCountGo()
        {
            var scrollOrigin = new WheelInputDevice.ScrollOrigin
            {
                Viewport = true,
                XOffset = 237,
                YOffset = 499
            };
            new OpenQA.Selenium.Interactions.Actions(drv)
                .ScrollFromOrigin(scrollOrigin, 0, 200)
                .Perform();
        }


        private void button3_Click(object sender, EventArgs e) /// Tablo Temizle 
        {
            dataGridView1.Rows.Clear();
        }

        private void button2_Click(object sender, EventArgs e) // Aranacak alanı temizler 
        {
            if (txt_city.Text != null)
            {
                txt_city.Clear();
                drv.FindElement(By.XPath("//*[@aria-label='Kapat']")).Click();
            }
        }

        private void txt_city_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                th = new Thread(vericek); th.Start();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridView1.SelectedRows)
            {
                if (!item.IsNewRow) // Yeni bir satırı silme
                {
                    dataGridView1.Rows.RemoveAt(item.Index);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            drv.Quit();
        }
    }
}
