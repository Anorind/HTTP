using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveCharts;
using LiveCharts.WinForms;
using LiveCharts.Definitions.Charts;
using LiveCharts.Wpf;
using LiveCharts.Helpers;
using System.Windows.Media;
using System.Reflection;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using HTTP.WeatherClasses;

namespace HTTP
{
    public partial class Form : System.Windows.Forms.Form
    {
        private LiveCharts.WinForms.CartesianChart cartesianChartTemperature;
        private LiveCharts.WinForms.CartesianChart cartesianChartHumidity;
        public Form()
        {
            InitializeComponent();

            cartesianChartTemperature = new LiveCharts.WinForms.CartesianChart
            {
                Series = new SeriesCollection { new LineSeries { Title = "Температура" } },
                Location = new Point(50, 100),
                Size = new Size(700, 140),
                AxisX = new AxesCollection
                {
                    new Axis
                    {
                        Title = "Час"
                        /*Labels = new[] { "0", "50", "2", "3", "4", "5" },*/
                        //Separator = new Separator { StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 3 } }
                    }
                },
                AxisY = new AxesCollection { new Axis { Title = "Температура", LabelFormatter = value => value.ToString() + "°C" } }

            };
            cartesianChartHumidity = new LiveCharts.WinForms.CartesianChart
            {
                Series = new SeriesCollection { new LineSeries { Title = "Вологість" } },
                Location = new Point(50, 300),
                Size = new Size(700, 140),
                AxisX = new AxesCollection { new Axis { Title = "Час"} },
                AxisY = new AxesCollection { new Axis { Title = "Вологість", LabelFormatter = value => value.ToString() + "%" } }

            };


            Controls.Add(cartesianChartTemperature);
            Controls.Add(cartesianChartHumidity);
        }
        private void buttonForecast_Click(object sender, EventArgs e)
        {
            double lat = 47;
            double lon = 32;

            try
            {
                lat = double.Parse(textBoxLatitude.Text);
                lon = double.Parse(textBoxLongitude.Text);




                HttpWebRequest reqw = HttpWebRequest.Create($"https://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&appid=9837d8a822f6d95e107ed9037401c0b9") as HttpWebRequest;
                HttpWebResponse resp = reqw.GetResponse() as HttpWebResponse;
                StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                //Console.WriteLine(sr.ReadToEnd());
                string myJsonResponse = sr.ReadToEnd();
                Root myDeserealisedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

                var valuesTemperature = new ChartValues<double>();
                var valuesHumidity = new ChartValues<double>();
                var labelsTemperature = new List<string>();
                var labelsHumidity = new List<string>();
                DateTime twoDaysLater = DateTime.Now.AddDays(2);


                foreach (var item in myDeserealisedClass.list)
                {
                    // Перетворюємо Unix timestamp в DateTime
                    DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dt = dt.AddSeconds(item.dt).ToLocalTime();

                    // Якщо час менше, ніж 2 дні вперед, додаємо температуру та вологість 
                    if (dt <= twoDaysLater)
                    {
                        // Перетворюємо температуру з Кельвінів в градуси Цельсія
                        double tempCelsius = item.main.temp - 273.15;
                        valuesTemperature.Add(tempCelsius);
                        valuesHumidity.Add(item.main.humidity);
                        labelsTemperature.Add(dt.ToString("g")); // Додаємо час до міток
                        labelsHumidity.Add(dt.ToString("g"));
                    }
                }

                labelCity.Text = myDeserealisedClass.city.name;
                labelCountry.Text = myDeserealisedClass.city.country;

                cartesianChartTemperature.Series[0].Values = valuesTemperature;
                cartesianChartTemperature.AxisX[0].Labels = labelsTemperature;
                cartesianChartHumidity.Series[0].Values = valuesHumidity;
                cartesianChartHumidity.AxisX[0].Labels = labelsHumidity;

                cartesianChartTemperature.Update();
                cartesianChartHumidity.Update();

                sr.Close();
            }
            catch (FormatException)
            {
                MessageBox.Show("Будь-ласка, введіть допустимі числа");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка: " + ex.Message);
            }
        }

    }
}
