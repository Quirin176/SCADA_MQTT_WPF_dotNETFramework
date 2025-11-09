using Driver_Tool.Manager;
using MQTT_Protocol.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;

namespace TagManagement.Dialog
{
    public delegate void EventBuildingChanged(Building building);

    /// <summary>
    /// Interaction logic for Window_Building.xaml
    /// </summary>
    public partial class Window_Building : Window
    {
        public EventBuildingChanged eventBuildingChanged = null;
        private Building building = null;

        public Window_Building(Building buildingParam = null)
        {
            InitializeComponent();
            this.building = buildingParam;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (building == null)
                {
                    this.Title = "Add Building";
                }
                else
                {
                    this.Title = "Edit Building";
                    txt_BuildingName.Text = building.BuildingName;
                    txt_Description.Text = building.Description;

                    DIEthernet die = building as DIEthernet;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear previous errors
                txt_BuildingName.BorderBrush = Brushes.Gray;
                txt_ErrorMessage.Text = "";

                if (string.IsNullOrEmpty(txt_BuildingName.Text) || string.IsNullOrWhiteSpace(txt_BuildingName.Text))
                {
                    txt_BuildingName.BorderBrush = Brushes.Red;
                    txt_ErrorMessage.Text = "The Building name is empty";
                    return;
                }

                DIEthernet die = new DIEthernet();
                die.BuildingName = txt_BuildingName.Text;
                if (building == null)
                {
                    die.BuildingId = Building_Manager.Buildings.Count + 1;
                    die.Floors = new List<Floor>();
                    Building_Manager.Add(die);
                    if (eventBuildingChanged != null) eventBuildingChanged(die);
                }
                else
                {
                    die.BuildingId = building.BuildingId;
                    die.Floors = building.Floors;
                    Building_Manager.Update(die);
                    if (eventBuildingChanged != null) eventBuildingChanged(die);
                    this.DialogResult = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            this.Close();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
