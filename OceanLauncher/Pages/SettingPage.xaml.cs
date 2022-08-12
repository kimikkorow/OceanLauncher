﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using OceanLauncher.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using WpfWidgetDesktop.Utils;

namespace OceanLauncher.Pages
{
    /// <summary>
    /// SettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : Page
    {

        public static string id = "core.cfg";
        public class CFG
        {
            public string Height = "1080";
            public string Width = "1920";
            public string Port = "1145";
            //public string Path = @"C:\Program Files\Genshin Impact\Genshin Impact Game\YuanShen.exe";
            public string Path="";
            public string Args = "";
        }


        SettingVM vm = new SettingVM
        {

        };
        public SettingPage()
        {
            InitializeComponent();
            DataContext = vm;

            CFG cfg = new CFG();
            try
            {
                cfg = JsonConvert.DeserializeObject<CFG>(SettingProvider.Get(id));
            }
            catch { }
            finally
            {
                if (cfg == null)
                {
                    cfg = new CFG();
                }
                vm.Args = cfg.Args;
                vm.Width = cfg.Width;
                vm.Height = cfg.Height;
                vm.Path = cfg.Path;
                vm.Port = cfg.Port;
            }
        }


        public class SettingVM : ObservableObject
        {
            public ICommand GoHome { get; set; }

            public SettingVM()
            {
                GoHome = new RelayCommand(() =>
                {
                    GlobalProps.NavigateTo(new Home());
                });
            }

            private string path;

            public string Path
            {
                get { return path; }
                set { SetProperty(ref path, value); }
            }
            private string width;

            public string Width
            {
                get { return width; }
                set { SetProperty(ref width, value); }
            }

            private string height;

            public string Height
            {
                get { return height; }
                set { SetProperty(ref height, value); }
            }

            private string args;

            public string Args
            {
                get { return args; }
                set { SetProperty(ref args, value); }
            }

            private string port;

            public string Port
            {
                get { return port; }
                set { SetProperty(ref port, value); }
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //CFG cfg = new CFG();
            //cfg.Args = vm.Args;
            //cfg.Width = vm.Width;
            //cfg.Height = vm.Height;
            //cfg.Path = vm.Path;
            //cfg.Port = vm.Port;

            SettingProvider.Set(id, vm);

            GlobalProps.frame.Navigate(new Home()); 
        }

        private void SearchPath(object sender, RoutedEventArgs e)
        {
            string gpath="";
            try
            {
                gpath = GameRegReader.GetGamePath();

            }
            catch (Exception)
            {
                MessageBox.Show("自动搜索失败，请手动指定 YuanShen.exe 所在位置！");
                gpath = GameRegReader.ChoseLauncherPath();
            }
            string cn = Path.Combine(gpath, "YuanShen.exe");
            string os = Path.Combine(gpath, "GenshinImpact.exe");
            if (File.Exists(cn))
            {
                vm.Path = cn;
            }
            else if (File.Exists(os))
            {
                vm.Path = os;
            }
            else
            {
                MessageBox.Show("未能找到游戏资源，建议确认路径是否正确");
                return;
            }

            MessageBox.Show("搜索成功！");
            SettingProvider.Set(id, vm);


        }

        

        private void Patch(object sender, RoutedEventArgs e)
        {

            new Utils.PatchHelper().Patch();


        }

        private void UnPatch(object sender, RoutedEventArgs e)
        {
            new Utils.PatchHelper().UnPatch();
        }

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            new Utils.PatchHelper().OpenFolder();


        }
    }
}
