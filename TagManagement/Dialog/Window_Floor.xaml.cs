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
    public delegate void EventFloorChanged(Floor floor);

    /// <summary>
    /// Interaction logic for Window_Floor.xaml
    /// </summary>
    public partial class Window_Floor : Window
    {
        private Building building = null;
        private Floor floor = null;
        public EventFloorChanged eventFloorChanged = null;

        public Window_Floor(Building buildingParam, Floor floorParam = null)
        {
            InitializeComponent();
            this.building = buildingParam;
            this.floor = floorParam;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (floor == null)
                {
                    this.Title = "Add Floor";
                }
                else
                {
                    this.Title = "Edit Floor";
                    txt_FloorName.Text = floor.FloorName;
                    txt_Description.Text = floor.Description;
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
                if (string.IsNullOrEmpty(txt_FloorName.Text) || string.IsNullOrWhiteSpace(txt_FloorName.Text))
                {
                    txt_FloorName.BorderBrush = Brushes.Red;
                    txt_ErrorMessage.Text = "The Floor name is empty";
                    return;
                }
                else
                {
                    // Clear previous errors
                    txt_FloorName.BorderBrush = Brushes.Gray;
                    txt_ErrorMessage.Text = "";

                    if (floor == null)
                    {
                        Floor dvNew = new Floor();
                        dvNew.FloorId = building.Floors.Count + 1;
                        dvNew.FloorName = txt_FloorName.Text;
                        dvNew.Description = txt_Description.Text;
                        dvNew.Rooms = new List<Room>();
                        Floor_Manager.Add(building, dvNew);
                        if (eventFloorChanged != null) eventFloorChanged(dvNew);
                    }
                    else
                    {
                        floor.FloorName = txt_FloorName.Text;
                        floor.Description = txt_Description.Text;
                        Floor_Manager.Update(building, floor);
                        if (eventFloorChanged != null) eventFloorChanged(floor);
                        this.DialogResult = true;
                    }
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
