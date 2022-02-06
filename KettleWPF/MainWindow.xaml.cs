using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace KettleWPF
{

    public partial class MainWindow : Window
    {
        private HttpClient client = new HttpClient();
        bool focus = false;
        int nowDelay = 5000;
        public MainWindow()
        {
            InitializeComponent();
            ModeSwitcher.SelectionChanged += (n, e) => SetMode();
            TempSlider.ValueChanged += (n, e) => setTempChange();

            CancellationToken cancellationToken = new CancellationToken();
            getStatus(cancellationToken);

            Activated += (e, v) =>
            {
                focus = true;
            };
            Deactivated += (e, v) => { focus = false; };
        }

        private void SetMode()
        {
            int nowMode = ModeSwitcher.SelectedIndex;
            nowDelay = 10000;
            btn.Content = "Сохранить!";
            switch (nowMode)
            {
                case 0:
                    setTemp.Content = "НН°";
                    TempSlider.IsEnabled = false;
                    break;
                case 1:
                case 2:
                    setTemp.Content = "50°";
                    TempSlider.IsEnabled = true;
                    break;
                    
            }
        }

        private async void btnClick(object sender, RoutedEventArgs e)
        {
            btn.IsEnabled = false;
            switch (btn.Content)
            {
                case "Сохранить!":
                    string path = "";
                    if (TempSlider.IsEnabled)
                    {
                        path = ModeSwitcher.SelectedIndex + "/" + TempSlider.Value;
                    }
                    else
                    {
                        path = "0/0";
                    }

                    try
                    {
                        string response = await client.GetStringAsync("http://"+Properties.Resources.host+":"+Properties.Resources.port+"/" + Properties.Resources.key + "/set/" + path);
                        // if (response != "OK") MessageBox.Show(response); // Вроде ругается, а вроде и ладно
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Сохранить настройки не удалось.");
                    }
                    break;

                case "Работай!":
                    Console.WriteLine("Start!");
                    try
                    {
                        string response = await client.GetStringAsync("http://" + Properties.Resources.host + ":" + Properties.Resources.port + "/" + Properties.Resources.key + "/run");
                        // if (response != "OK") MessageBox.Show(response); // Тут тот же прикол
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Увы, но ты не попьешь сегодня чай(");
                    }
                    break;

                case "Отдохни пока":
                    Console.WriteLine("Stop!");
                    try
                    {
                        string response = await client.GetStringAsync("http://" + Properties.Resources.host + ":" + Properties.Resources.port + "/" + Properties.Resources.key + "/stop");
                        // if (response != "OK") MessageBox.Show(response); // И тут
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Не удалось остановить чайник, беги к нему скорее!");
                    }
                    break;
            }
        }

        private void setTempChange ()
        {
            setTemp.Content = TempSlider.Value + "°";
        }

        private async Task getStatus(CancellationToken cancellationToken)
        {
            while (true)
            {
                string nd = nowDelay+"";
                await Task.Delay(nowDelay, cancellationToken);
                if(int.Parse(nd)!=nowDelay) await Task.Delay(nowDelay, cancellationToken);
                nowDelay = 5000;
                if (focus)
                {
                    try
                    {
                        string response = await client.GetStringAsync("http://" + Properties.Resources.host + ":" + Properties.Resources.port + "/" + Properties.Resources.key + "/status");
                        if (response != "ERR")
                        {
                            btn.IsEnabled = true;
                            string[] status = response.Split('-');
                            string mode = "0";
                            switch (status[0])
                            {
                                case "00":
                                    mode = "0";
                                    break;
                                case "01":
                                    mode = "1";
                                    break;
                                case "02":
                                    mode = "2";
                                    break;
                            }
                            ModeSwitcher.SelectedIndex = int.Parse(mode);


                            int settedTemp = Convert.ToInt32(status[1], 16);
                            if ((string)setTemp.Content == "--°")
                            {
                                if (mode == "0")
                                {
                                    setTemp.Content = "НН°";
                                }
                                else
                                {
                                    setTemp.Content = settedTemp + "°";
                                    TempSlider.Value = settedTemp;
                                }
                            }


                            int nowTemp = Convert.ToInt32(status[2], 16);
                            Temp.Content = nowTemp + "°";


                            switch (status[3])
                            {
                                case "00":
                                    btn.Content = "Работай!"; // Включить
                                    ModeSwitcher.IsEnabled = true;
                                    if (mode == "0")
                                    {
                                        TempSlider.IsEnabled = false;
                                    }
                                    else
                                    {
                                        TempSlider.IsEnabled = true;
                                    }
                                    break;
                                case "02":
                                    btn.Content = "Отдохни пока"; // Выключить
                                    ModeSwitcher.IsEnabled = false;
                                    TempSlider.IsEnabled = false;
                                    break;
                            }

                        }
                        else
                        {
                            ModeSwitcher.IsEnabled = false;
                            TempSlider.IsEnabled = false;
                            btn.IsEnabled = false;
                        }
                        

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Сервер не отвечает");
                        Hide();
                    }
                }
            }

        }
    }

}
