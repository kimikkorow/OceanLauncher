using Microsoft.Toolkit.Mvvm.ComponentModel;
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

                return;
                throw;
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
                MessageBox.Show("自动搜索失败，请手动指定 YuanShen.exe 所在位置！");
                return;
            }


            SettingProvider.Set(id, vm);


        }

        public bool IsAdministrator()

        {

            WindowsIdentity current = WindowsIdentity.GetCurrent();

            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);

            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);

        }

        const string METADATA_PATH = "patch-metadata";
        const string FILE_NAME = "global-metadata.dat";


        enum CilentType
        {
            cnrel,
            osrel,
            notsupported
        }

        private CilentType GetCilentType()
        {
            string file_path = Path.Combine(Path.GetDirectoryName(vm.Path), "YuanShen_Data", "Managed", "Metadata");

            string file_path_osrel = Path.Combine(Path.GetDirectoryName(vm.Path), "GenshinImpact_Data", "Managed", "Metadata");

            if (Directory.Exists(file_path_osrel))
            {
                return CilentType.osrel;
            }
            else if (Directory.Exists(file_path))
            {
                return CilentType.cnrel;
            }
            else
            {
                return CilentType.notsupported;
            }

        }


        private void Patch(object sender, RoutedEventArgs e)
        {

            if (!IsAdministrator())
            {
                MessageBox.Show("未获取原神文件夹的读写权限，请以管理员身份运行启动器！");
                return;
            }

            try
            {
                Path.GetDirectoryName(vm.Path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("游戏路径不正确！"+ex.Message);
                return
                ;
            }
            string file_path = Path.Combine(Path.GetDirectoryName(vm.Path), "YuanShen_Data", "Managed", "Metadata");
            string file_path_osrel = Path.Combine(Path.GetDirectoryName(vm.Path), "GenshinImpact_Data", "Managed", "Metadata");

            if (GetCilentType() == CilentType.osrel)
            {
                file_path = file_path_osrel;
            }
            //备份
            if (!File.Exists(Path.Combine(file_path, FILE_NAME + ".bak")))
            {
                File.Copy(
                    Path.Combine(file_path, FILE_NAME),
                    Path.Combine(file_path, FILE_NAME + ".bak"));
            }

            //修补
            if (GetCilentType() == CilentType.cnrel)
            {
                

                string patched_file = Path.Combine(METADATA_PATH, "cnrel-"+FILE_NAME);
                if (File.Exists(patched_file))
                {
                    File.Copy(patched_file, Path.Combine(file_path, FILE_NAME), true
                        );
                }
                else
                {
                    MessageBox.Show($"文件不存在！{patched_file}");
                    return;
                }
            }
            else if (GetCilentType() == CilentType.osrel)
            {

                string patched_file = Path.Combine(METADATA_PATH, "osrel-"+FILE_NAME);
                if (File.Exists(patched_file))
                {
                    File.Copy(patched_file, Path.Combine(file_path_osrel, FILE_NAME), true
                        );
                }
                else
                {
                    MessageBox.Show($"文件不存在！{patched_file}");
                    return;
                }
            }
            else
            {
                MessageBox.Show($"不支持的客户端类型！");
                return;
            }
            MessageBox.Show($"成功Patch了客户端！");


        }

        private void UnPatch(object sender, RoutedEventArgs e)
        {
            if (!IsAdministrator())
            {
                MessageBox.Show("未获取原神文件夹的读写权限，请以管理员身份运行启动器！");
                return;
            }

            try
            {
                Path.GetDirectoryName(vm.Path);

            }
            catch (Exception ex)
            {
                MessageBox.Show("游戏路径不正确！" + ex.Message);
                return
                ;
            }
            string file_path = Path.Combine(Path.GetDirectoryName(vm.Path), "YuanShen_Data", "Managed", "Metadata");
            string file_path_osrel = Path.Combine(Path.GetDirectoryName(vm.Path), "GenshinImpact_Data", "Managed", "Metadata");

            if (GetCilentType() == CilentType.cnrel)
            {

                if (File.Exists(Path.Combine(file_path, FILE_NAME + ".bak")))
                {
                    File.Copy(
                        Path.Combine(file_path, FILE_NAME + ".bak"),
                        Path.Combine(file_path, FILE_NAME), true);
                }
                else
                {
                    MessageBox.Show("未找到备份文件！");
                }
            }
            else if (GetCilentType() == CilentType.osrel)
            {

                if (File.Exists(Path.Combine(file_path_osrel, FILE_NAME + ".bak")))
                {
                    File.Copy(
                        Path.Combine(file_path_osrel, FILE_NAME + ".bak"),
                        Path.Combine(file_path_osrel, FILE_NAME), true);
                }
                else
                {
                    MessageBox.Show("未找到备份文件！");
                }
            }
            else
            {
                MessageBox.Show($"不支持的客户端类型！");
                return;
            }

            MessageBox.Show($"成功UnPatch了客户端！");

        }

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(METADATA_PATH))
            {
                Directory.CreateDirectory(METADATA_PATH);
            }

            System.Diagnostics.Process.Start(Path.Combine(Environment.CurrentDirectory,METADATA_PATH));



        }
    }
}
