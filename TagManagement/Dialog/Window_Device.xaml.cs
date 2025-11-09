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
    public delegate void EventDeviceChanged(Device device);

    /// <summary>
    /// Interaction logic for Window_Device.xaml
    /// </summary>
    public partial class Window_Device : Window
    {
        private Building building = null;
        private Floor floor = null;
        private Room room = null;
        private Device device = null;
        public EventDeviceChanged eventDeviceChanged = null;

        public Window_Device(Building buildingParam, Floor floorParam, Room roomParam, Device deviceParam = null)
        {
            InitializeComponent();
            building = buildingParam;
            floor = floorParam;
            room = roomParam;
            device = deviceParam;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                txt_RoomName.Text = room.RoomName;
                txt_RoomName.IsEnabled = false;

                txt_FloorName.Text = floor.FloorName;
                txt_FloorName.IsEnabled = false;

                cbox_QoS.SelectedIndex = 1;

                if (this.device == null)
                {
                    this.Title = "Add Device";
                }
                else
                {
                    this.Title = "Edit Device";
                    txt_DeviceName.Text = device.DeviceName;
                    txt_Description.Text = device.Description;
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
                if (string.IsNullOrEmpty(txt_DeviceName.Text) || string.IsNullOrWhiteSpace(txt_DeviceName.Text))
                {
                    txt_DeviceName.BorderBrush = Brushes.Red;
                    txt_DeviceName_Error.Text = "The Device Name is empty";
                    return;
                }

                if (check_MultipleTags.IsChecked == true && (string.IsNullOrEmpty(txt_NumberofTags.Text) || string.IsNullOrWhiteSpace(txt_NumberofTags.Text)))
                {
                    txt_NumberofTags.BorderBrush = Brushes.Red;
                    txt_NumberofTags_Error.Text = "Tag Prefix is empty";
                    return;
                }

                if (check_MultipleTags.IsChecked == true && (string.IsNullOrEmpty(txt_TagPrefix.Text) || string.IsNullOrWhiteSpace(txt_TagPrefix.Text)))
                {
                    txt_TagPrefix.BorderBrush = Brushes.Red;
                    txt_TagPrefix_Error.Text = "Tag Name Prefix is empty";
                    return;
                }

                if (check_MultipleTags.IsChecked == true && (string.IsNullOrEmpty(txt_TopicPrefix.Text) || string.IsNullOrWhiteSpace(txt_TopicPrefix.Text)))
                {
                    txt_TopicPrefix.BorderBrush = Brushes.Red;
                    txt_TopicPrefix_Error.Text = "Tag Name Prefix is empty";
                    return;
                }
                else
                {
                    // Clear previous errors
                    txt_DeviceName.BorderBrush = Brushes.Gray;
                    txt_DeviceName_Error.Text = "";
                    txt_NumberofTags.BorderBrush = Brushes.Gray;
                    txt_NumberofTags_Error.Text = "";
                    txt_TagPrefix.BorderBrush = Brushes.Gray;
                    txt_TagPrefix_Error.Text = "";
                    txt_TopicPrefix.BorderBrush = Brushes.Gray;
                    txt_TopicPrefix_Error.Text = "";

                    if (device == null)
                    {
                        Device deviceNew = new Device();
                        deviceNew.DeviceId = room.Devices.Count + 1;
                        deviceNew.DeviceName = txt_DeviceName.Text;
                        deviceNew.Description = txt_Description.Text;
                        deviceNew.Tags = new List<Tag>();
                        Device_Manager.Add(room, deviceNew);

                        if (check_MultipleTags.IsChecked == true)
                        {
                            ushort number = ushort.Parse(txt_NumberofTags.Text);

                            for (int i = 1; i <= number; i++)
                            {
                                Tag tag = new Tag();
                                tag.TagId = i;
                                tag.TagName = string.Format("{0}{1}", txt_TagPrefix.Text, i);
                                tag.Topic = string.Format("{0}{1}", txt_TopicPrefix.Text, i);
                                tag.QoS = byte.Parse((cbox_QoS.SelectedItem as ComboBoxItem).Content.ToString());
                                tag.Description = string.Format("{0}{1}", txt_Description.Text, i);
                                tag.Retain = check_Retain.IsChecked.Value;
                                deviceNew.Tags.Add(tag);
                            }
                        }

                        if (eventDeviceChanged != null) eventDeviceChanged(deviceNew);
                    }
                    else
                    {
                        device.DeviceName = txt_DeviceName.Text;
                        device.Description = txt_Description.Text;

                        Device_Manager.Update(room, device);

                        if (check_MultipleTags.IsChecked == true)
                        {
                            ushort number = ushort.Parse(txt_NumberofTags.Text);

                            device.Tags.Clear();
                            for (int i = 1; i <= number; i++)
                            {
                                Tag tag = new Tag();
                                tag.TagId = i;
                                tag.TagName = string.Format("{0}{1}", txt_TagPrefix.Text, i);
                                tag.Topic = string.Format("{0}{1}", txt_TopicPrefix.Text, i);
                                tag.QoS = byte.Parse((cbox_QoS.SelectedItem as ComboBoxItem).Content.ToString());
                                tag.Description = string.Format("{0}{1}", txt_Description.Text, i);
                                tag.Retain = check_Retain.IsChecked.Value;
                                device.Tags.Add(tag);
                            }
                        }

                        if (eventDeviceChanged != null) eventDeviceChanged(device);

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

        private void check_MultipleTags_CheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = check_MultipleTags.IsChecked.Value;

                txt_NumberofTags.IsEnabled = isChecked;
                txt_TagPrefix.IsEnabled = isChecked;
                txt_TopicPrefix.IsEnabled = isChecked;
                cbox_QoS.IsEnabled = isChecked;
                check_Retain.IsEnabled = isChecked;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
