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
    public delegate void EventTagChanged(Tag tag);

    /// <summary>
    /// Interaction logic for Window_Tag.xaml
    /// </summary>
    public partial class Window_Tag : Window
    {
        private Building building;
        private Floor floor;
        private Room room;
        private Device device;
        private Tag tag;
        public EventTagChanged eventTagChanged = null;

        public Window_Tag(Building buildingParam, Floor floorParam, Room roomParam, Device deviceParam, Tag tagParam = null)
        {
            InitializeComponent();

            building = buildingParam;
            floor = floorParam;
            room = roomParam;
            device = deviceParam;
            tag = tagParam;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                txt_FloorName.Text = floor.FloorName;
                txt_RoomName.Text = room.RoomName;
                txt_DeviceName.Text = device.DeviceName;

                txt_FloorName.IsEnabled = false;
                txt_RoomName.IsEnabled = false;
                txt_DeviceName.IsEnabled = false;

                if (tag == null)
                {
                    this.Title = "Add Tag";
                    cbox_QoS.SelectedIndex = 1;
                }
                else
                {
                    this.Title = "Edit Tag";
                    txt_TagName.Text = tag.TagName;
                    txt_Topic.Text = tag.Topic;
                    cbox_QoS.SelectedItem = string.Format("{0}", tag.QoS);
                    check_Retain.IsChecked = tag.Retain;
                    txt_Description.Text = tag.Description;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            try
            {
                if ((string.IsNullOrEmpty(txt_TagName.Text) || string.IsNullOrWhiteSpace(txt_TagName.Text)))
                {
                    txt_TagName.BorderBrush = Brushes.Red;
                    txt_TagName_Error.Text = "The Tag Name is empty";
                    return;
                }
                if ((string.IsNullOrEmpty(txt_Topic.Text) || string.IsNullOrWhiteSpace(txt_Topic.Text)))
                {
                    txt_Topic.BorderBrush = Brushes.Red;
                    txt_Topic_Error.Text = "The Topic is empty";
                    return;
                }
                else
                {
                    // Clear previous errors
                    txt_TagName.BorderBrush = Brushes.Gray;
                    txt_TagName_Error.Text = "";
                    txt_Topic.BorderBrush = Brushes.Gray;
                    txt_Topic_Error.Text = "";

                    if (tag == null)
                    {
                        Tag newTg = new Tag();
                        newTg.TagId = device.Tags.Count + 1;
                        newTg.TagName = txt_TagName.Text;
                        newTg.Topic = txt_Topic.Text;
                        newTg.QoS = byte.Parse((cbox_QoS.SelectedItem as ComboBoxItem).Content.ToString());
                        newTg.Retain = check_Retain.IsChecked.Value;
                        newTg.Description = txt_Description.Text;
                        Tag_Manager.Add(device, newTg);
                        if (eventTagChanged != null) eventTagChanged(newTg);
                    }
                    else
                    {
                        tag.TagName = txt_TagName.Text;
                        tag.Topic = txt_Topic.Text;
                        tag.Description = txt_Description.Text;
                        tag.QoS = byte.Parse((cbox_QoS.SelectedItem as ComboBoxItem).Content.ToString());
                        tag.Retain = check_Retain.IsChecked.Value;
                        Tag_Manager.Update(device, tag);
                        if (eventTagChanged != null) eventTagChanged(tag);
                        this.DialogResult = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
