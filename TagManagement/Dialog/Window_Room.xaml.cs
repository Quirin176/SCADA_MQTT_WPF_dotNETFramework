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
    public delegate void EventRoomChanged(Room room);

    /// <summary>
    /// Interaction logic for Window_Room.xaml
    /// </summary>
    public partial class Window_Room : Window
    {
        private Building building = null;
        private Floor floor = null;
        private Room room = null;
        public EventRoomChanged eventRoomChanged = null;

        public Window_Room(Building buildingParam, Floor floorParam, Room roomParam = null)
        {
            InitializeComponent();
            this.building = buildingParam;
            this.floor = floorParam;
            this.room = roomParam;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                txt_FloorName.Text = this.floor.FloorName;
                txt_FloorName.IsEnabled = false;

                if (this.room == null)
                {
                    this.Title = "Add Room";
                }
                else
                {
                    this.Title = "Edit Room";
                    txt_RoomName.Text = room.RoomName;
                    txt_Description.Text = room.Description;
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
                if (string.IsNullOrEmpty(txt_RoomName.Text) || string.IsNullOrWhiteSpace(txt_RoomName.Text))
                {
                    txt_RoomName.BorderBrush = Brushes.Red;
                    txt_ErrorMessage.Text = "The Room name is empty";
                    return;
                }
                else
                {
                    // Clear previous errors
                    txt_RoomName.BorderBrush = Brushes.Gray;
                    txt_ErrorMessage.Text = "";

                    if (room == null)
                    {
                        Room dvNew = new Room();
                        dvNew.RoomId = floor.Rooms.Count + 1;
                        dvNew.RoomName = txt_RoomName.Text;
                        dvNew.Description = txt_Description.Text;
                        dvNew.Devices = new List<Device>();
                        Room_Manager.Add(floor, dvNew);
                        if (eventRoomChanged != null) eventRoomChanged(dvNew);
                    }
                    else
                    {
                        room.RoomName = txt_RoomName.Text;
                        room.Description = txt_Description.Text;
                        Room_Manager.Update(floor, room);
                        if (eventRoomChanged != null) eventRoomChanged(room);
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
