using RegistryBinaryEditor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AutumnDudesSaveEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _saveDumpFolder = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Dump";

        private SaveFile buttonTreeSelectedSaveFile = null;
        private SaveFile axisTreeSelectedSaveFile = null;

        private ObservableCollection<SaveFile> saveFiles = new ObservableCollection<SaveFile>();
        private ObservableCollection<TreeViewRoot> _SettingsTreeViewRoot { get; set; } = new ObservableCollection<TreeViewRoot>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Dump.DumpHKCUBinaryValuesToFiles(@"Software\Mediatonic\FallGuys_client", _saveDumpFolder);
        }

        private void ButtonTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (buttonMapsTreeView.SelectedItem as ActionElementMap != null)
                    buttonTreeSelectedSaveFile.ButtonMaps.Remove(buttonMapsTreeView.SelectedItem as ActionElementMap);
            }
        }


        private void AxisTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (axisMapsTreeView.SelectedItem as ActionElementMap != null)
                    axisTreeSelectedSaveFile.AxisMaps.Remove(axisMapsTreeView.SelectedItem as ActionElementMap);
            }
        }


        private void SaveActionElementMapsToFiles()
        {
            foreach (SaveFile saveFile in saveFiles)
            {
                SaveFileParser.SaveFile(saveFile);
            }
        }

        private void loadSaveDataButton_Click(object sender, RoutedEventArgs e)
        {
            _SettingsTreeViewRoot.Clear();
            saveFiles.Clear();

            List<string> listOfFiles = new List<string>(Directory.GetFiles(_saveDumpFolder));

            foreach (string filePath in listOfFiles)
            {
                var saveFile = SaveFileParser.LoadFile(filePath);
                if (saveFile != null)
                    saveFiles.Add(saveFile);
            }

            _SettingsTreeViewRoot.Add(new TreeViewRoot() { SaveFiles = saveFiles });
            axisMapsTreeView.ItemsSource = _SettingsTreeViewRoot;
            buttonMapsTreeView.ItemsSource = _SettingsTreeViewRoot;
        }

        private void HandleActionElementMapItemDoubleClick(object sender, RoutedEventArgs e)
        {
            if (((TreeViewItem)sender).DataContext is ActionElementMap item)
            {
                ActionModifyWindow actionModifyWindow = new ActionModifyWindow(item);
                actionModifyWindow.ShowDialog();
            }
        }

        private void HandleSaveFileButtonItemRightClick(object sender, RoutedEventArgs e)
        {
            if (((TreeViewItem)sender).DataContext is SaveFile saveFile)
            {
                saveFile.ButtonMaps.Add(ActionElementMap.GenerateEmptyActionElementMap());
            }
        }

        private void HandleSaveFileAxisItemRightClick(object sender, RoutedEventArgs e)
        {
            if (((TreeViewItem)sender).DataContext is SaveFile saveFile)
            {
                Console.WriteLine("test");
                saveFile.AxisMaps.Add(ActionElementMap.GenerateEmptyActionElementMap());
            }
        }

        private void writeDataToFilesButton_Click(object sender, RoutedEventArgs e)
        {
            SaveActionElementMapsToFiles();
        }

        private void pushFilesToRegistryButton_Click(object sender, RoutedEventArgs e)
        {
            RegistryBinaryEditor.Dump.WriteHKCUBinaryValuesFromFiles(@"Software\Mediatonic\FallGuys_client", _saveDumpFolder);
        }

        private void AxisTreeView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (((TreeViewItem)sender).DataContext is SaveFile saveFile)
            {
                axisTreeSelectedSaveFile = saveFile;
            }
        }

        private void ButtonTreeView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (((TreeViewItem)sender).DataContext is SaveFile saveFile)
            {
                buttonTreeSelectedSaveFile = saveFile;
            }
        }
    }




    public class TreeViewRoot
    {
        public ObservableCollection<SaveFile> SaveFiles { get; set; } = new ObservableCollection<SaveFile>();
    }
}
