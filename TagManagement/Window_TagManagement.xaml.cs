using Driver_Tool.Manager;
using Microsoft.Win32;
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
using TagManagement.Dialog;

namespace TagManagement
{
    /// <summary>
    /// Interaction logic for Window_TagManagement.xaml
    /// </summary>
    public partial class Window_TagManagement : Window
    {
        private bool IsDataChanged = false;

        public Window_TagManagement()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string xmlFile = Building_Manager.ReadKey(Building_Manager.XML_NAME_DEFAULT);
                if (string.IsNullOrEmpty(xmlFile) || string.IsNullOrWhiteSpace(xmlFile)) return;
                InitializeData(Building_Manager.ReadKey(Building_Manager.XML_NAME_DEFAULT));

                foreach (TreeViewItem item in treeView1.Items)
                {
                    ExpandAll(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void frm_TagManagement_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (!IsDataChanged) return;

                MessageBoxResult result = MessageBox.Show("Are you sure you want to save?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    btn_Save_Click(sender, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExpandAll(TreeViewItem item)
        {
            item.IsExpanded = true;
            foreach (object subItem in item.Items)
            {
                if (subItem is TreeViewItem childItem)
                {
                    ExpandAll(childItem);
                }
            }
        }

        private void InitializeData(string xmlPath)
        {
            Building_Manager.Buildings.Clear();
            Building_Manager.XmlPath = xmlPath;
            List<Building> buildingList = Building_Manager.GetBuildings();
            treeView1.Items.Clear();

            foreach (Building building in buildingList)
            {
                List<TreeViewItem> floorList = new List<TreeViewItem>();
                building.Floors.Sort((x, y) => x.FloorName.CompareTo(y.FloorName));

                foreach (Floor floor in building.Floors)
                {
                    List<TreeViewItem> roomList = new List<TreeViewItem>();
                    foreach (Room room in floor.Rooms)
                    {
                        List<TreeViewItem> tagList = new List<TreeViewItem>();
                        foreach (Device device in room.Devices)
                        {
                            tagList.Add(new TreeViewItem { Header = device.DeviceName });
                        }

                        TreeViewItem roomNode = new TreeViewItem { Header = room.RoomName };
                        foreach (var tag in tagList)
                        {
                            roomNode.Items.Add(tag);
                        }
                        roomList.Add(roomNode);
                    }

                    TreeViewItem floorNode = new TreeViewItem { Header = floor.FloorName };
                    foreach (var room in roomList)
                    {
                        floorNode.Items.Add(room);
                    }
                    floorList.Add(floorNode);
                }

                TreeViewItem buildingNode = new TreeViewItem { Header = building.BuildingName };
                foreach (var floor in floorList)
                {
                    buildingNode.Items.Add(floor);
                }

                treeView1.Items.Add(buildingNode);
            }
        }

        private void btn_New_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Building_Manager.CreatFile(string.Format("{0}\\{1}.xml", AppDomain.CurrentDomain.BaseDirectory, Building_Manager.XML_NAME_DEFAULT));
                treeView1.Items.Clear();
                listView1.Items.Clear();
                Building_Manager.Buildings.Clear();
                IsDataChanged = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog
                {
                    Filter = "Xml Files (*.xml)|*.xml|All files (*.*)|*.*",
                    FileName = "config"
                };

                bool? result = openFileDialog1.ShowDialog();
                if (result == true)
                {
                    InitializeData(openFileDialog1.FileName);
                    IsDataChanged = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(Building_Manager.XmlPath) || string.IsNullOrWhiteSpace(Building_Manager.XmlPath))
                {
                    Building_Manager.Save(string.Format("{0}\\{1}.xml", AppDomain.CurrentDomain.BaseDirectory, Building_Manager.XML_NAME_DEFAULT));
                }
                else
                {
                    Building_Manager.Save(Building_Manager.XmlPath);
                    MessageBox.Show(this, "Data saved successfully!", Msg.MSG_INFORMATION, MessageBoxButton.OK, MessageBoxImage.Error);
                    IsDataChanged = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Xml Files (*.xml)|*.xml|All files (*.*)|*.*",
                    FileName = Building_Manager.XML_NAME_DEFAULT
                };

                bool? result = saveFileDialog.ShowDialog();
                if (result == true) // In WPF, true means OK was clicked
                {
                    string xmlPath = saveFileDialog.FileName;
                    Building_Manager.Save(xmlPath);
                    IsDataChanged = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_Building_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window_Building chFrm = new Window_Building();
                chFrm.eventBuildingChanged += new EventBuildingChanged((ch) =>
                {
                    try
                    {
                        TreeViewItem newNode = new TreeViewItem { Header = ch.BuildingName };
                        treeView1.Items.Add(newNode);
                        newNode.IsSelected = true;
                        IsDataChanged = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
                chFrm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetTreeViewItemLevel(TreeViewItem item)
        {
            int level = 0;
            TreeViewItem parentItem = GetParentTreeViewItem(item);
            while (parentItem != null)
            {
                item = parentItem;
                level++;
                parentItem = GetParentTreeViewItem(item);
            }
            return level;
        }

        private TreeViewItem GetParentTreeViewItem(TreeViewItem item)
        {
            if (item == null) return null;

            DependencyObject parent = VisualTreeHelper.GetParent(item);
            while (parent != null && !(parent is TreeViewItem))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as TreeViewItem;
        }

        private string GetParentHeader(TreeViewItem item, int levelUp)
        {
            for (int i = 0; i < levelUp; i++)
            {
                if (item.Parent is TreeViewItem parent)
                    item = parent;
                else
                    return string.Empty;
            }
            return item.Header?.ToString() ?? string.Empty;
        }

        private string GetParentNodeText(TreeViewItem node, int levelsUp)
        {
            TreeViewItem currentNode = node;
            for (int i = 0; i < levelsUp; i++)
            {
                if (currentNode.Parent is TreeViewItem parentNode)
                    currentNode = parentNode;
                else
                    return string.Empty;
            }
            return currentNode.Header.ToString();
        }

        private void RemoveTreeViewItem(TreeViewItem item)
        {
            if (item == null) return;
            TreeViewItem parent = GetParentTreeViewItem(item);
            if (parent != null)
            {
                parent.Items.Remove(item);
            }
            else
            {
                treeView1.Items.Remove(item);
            }
        }

        private void btn_Floor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Building buildingCurrent = null;
                Window_Floor floorFrm = null;

                if (treeView1.SelectedItem is TreeViewItem selectedItem)
                {
                    int level = GetTreeViewItemLevel(selectedItem);

                    switch (level)
                    {
                        case 0: // Select a Building Node
                            buildingCurrent = Building_Manager.GetByBuildingName(selectedItem.Header.ToString());
                            break;
                        case 1: // Select a Floor Node
                            TreeViewItem parentItem = GetParentTreeViewItem(selectedItem);
                            if (parentItem != null)
                            {
                                buildingCurrent = Building_Manager.GetByBuildingName(parentItem.Header.ToString());
                            }
                            break;
                    }
                }
                else
                {
                    return;
                }

                floorFrm = new Window_Floor(buildingCurrent);
                floorFrm.eventFloorChanged += new EventFloorChanged((floor) =>
                {
                    try
                    {
                        TreeViewItem newNode = new TreeViewItem { Header = floor.FloorName };

                        // Renaming 'selectedItem' to 'currentSelectedItem' inside the lambda
                        if (treeView1.SelectedItem is TreeViewItem currentSelectedItem)
                        {
                            int currentLevel = GetTreeViewItemLevel(currentSelectedItem);

                            if (currentLevel == 0)
                            {
                                currentSelectedItem.Items.Add(newNode);
                                IsDataChanged = true;
                            }
                            else if (currentLevel == 1)
                            {
                                TreeViewItem parentItem = GetParentTreeViewItem(currentSelectedItem);
                                if (parentItem != null)
                                {
                                    parentItem.Items.Add(newNode);
                                    IsDataChanged = true;
                                }
                            }
                            else
                            {
                                return;
                            }

                            newNode.IsSelected = true;
                            newNode.IsExpanded = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });

                floorFrm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_Room_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Building buildingCurrent = null;
                Floor floorCurrent = null;
                Window_Room roomFrm = null;

                if (treeView1.SelectedItem is TreeViewItem selectedItem)
                {
                    int level = GetTreeViewItemLevel(selectedItem);

                    if (level == 1) // Select a Floor Node
                    {
                        TreeViewItem parentItem = GetParentTreeViewItem(selectedItem);
                        if (parentItem != null)
                        {
                            buildingCurrent = Building_Manager.GetByBuildingName(parentItem.Header.ToString());
                            floorCurrent = Floor_Manager.GetByFloorName(buildingCurrent, selectedItem.Header.ToString());
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }

                roomFrm = new Window_Room(buildingCurrent, floorCurrent);
                roomFrm.eventRoomChanged += new EventRoomChanged((room) =>
                {
                    try
                    {
                        if (treeView1.SelectedItem is TreeViewItem currentSelectedItem)
                        {
                            int currentLevel = GetTreeViewItemLevel(currentSelectedItem);

                            if (currentLevel == 1)
                            {
                                TreeViewItem newNode = new TreeViewItem { Header = room.RoomName };
                                currentSelectedItem.Items.Add(newNode);
                                IsDataChanged = true;
                                currentSelectedItem.IsExpanded = true;
                                newNode.IsSelected = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });

                roomFrm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_Device_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Building buildingCurrent = null;
                Floor floorCurrent = null;
                Room roomCurrent = null;
                Window_Device deviceFrm = null;

                if (treeView1.SelectedItem is TreeViewItem selectedItem)
                {
                    int level = GetTreeViewItemLevel(selectedItem);

                    if (level == 2) // Select a Room Node
                    {
                        TreeViewItem floorItem = GetParentTreeViewItem(selectedItem);
                        TreeViewItem buildingItem = GetParentTreeViewItem(floorItem);

                        if (buildingItem != null && floorItem != null)
                        {
                            buildingCurrent = Building_Manager.GetByBuildingName(buildingItem.Header.ToString());
                            floorCurrent = Floor_Manager.GetByFloorName(buildingCurrent, floorItem.Header.ToString());
                            roomCurrent = Room_Manager.GetByRoomName(floorCurrent, selectedItem.Header.ToString());
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }

                deviceFrm = new Window_Device(buildingCurrent, floorCurrent, roomCurrent);
                deviceFrm.eventDeviceChanged += new EventDeviceChanged((device) =>
                {
                    try
                    {
                        if (treeView1.SelectedItem is TreeViewItem currentSelectedItem)
                        {
                            int currentLevel = GetTreeViewItemLevel(currentSelectedItem);

                            if (currentLevel == 2) // Room Node
                            {
                                TreeViewItem newNode = new TreeViewItem { Header = device.DeviceName };
                                currentSelectedItem.Items.Add(newNode);
                                IsDataChanged = true;
                                currentSelectedItem.IsExpanded = true;
                                newNode.IsSelected = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });

                deviceFrm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_Tag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(treeView1.SelectedItem is TreeViewItem selectedItem)) return;

                TreeViewItem roomItem = GetParentTreeViewItem(selectedItem);
                TreeViewItem floorItem = GetParentTreeViewItem(roomItem);
                TreeViewItem buildingItem = GetParentTreeViewItem(floorItem);

                if (buildingItem == null || floorItem == null || roomItem == null)
                    return;

                Building buildingCurrent = Building_Manager.GetByBuildingName(buildingItem.Header.ToString());
                Floor floorCurrent = Floor_Manager.GetByFloorName(buildingCurrent, floorItem.Header.ToString());
                Room roomCurrent = Room_Manager.GetByRoomName(floorCurrent, roomItem.Header.ToString());
                Device deviceCurrent = Device_Manager.GetByDeviceName(roomCurrent, selectedItem.Header.ToString());

                Window_Tag tgFrm = new Window_Tag(buildingCurrent, floorCurrent, roomCurrent, deviceCurrent);
                tgFrm.eventTagChanged += new EventTagChanged((tg) =>
                {
                    try
                    {
                        ListViewItem item = new ListViewItem
                        {
                            Content = new string[]
                            {
                        tg.TagName,
                        tg.Topic.ToString(),
                        tg.QoS.ToString(),
                        tg.Retain.ToString(),
                        tg.Description,
                            }
                        };
                        listView1.Items.Add(item);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });

                tgFrm.ShowDialog();
                selectedItem.IsExpanded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (treeView1.SelectedItem == null) return;
                TreeViewItem selectedNode = treeView1.SelectedItem as TreeViewItem;
                if (selectedNode == null) return;

                int Level = GetTreeViewItemLevel(selectedNode);
                string selectedNodeText = selectedNode.Header.ToString();
                MessageBoxResult result;

                // Delete Tags
                if (listView1.SelectedItems.Count > 0)
                {
                    var itemsToDelete = listView1.SelectedItems.Cast<ListViewItem>().ToList();
                    foreach (ListViewItem item in itemsToDelete)
                    {
                        string tagName = item.Content.ToString(); // Get tag name

                        Building buildingCurrent = Building_Manager.GetByBuildingName(GetParentHeader(selectedNode, 3));
                        Floor floorCurrent = Floor_Manager.GetByFloorName(buildingCurrent, GetParentHeader(selectedNode, 2));
                        Room roomCurrent = Room_Manager.GetByRoomName(floorCurrent, GetParentHeader(selectedNode, 1));
                        Device deviceCurrent = Device_Manager.GetByDeviceName(roomCurrent, selectedNodeText);
                        Tag tagToDelete = Tag_Manager.GetByTagName(deviceCurrent, tagName);

                        Tag_Manager.Delete(deviceCurrent, tagToDelete);
                        listView1.Items.Remove(item);
                        IsDataChanged = true;
                    }
                    return;
                }

                switch (Level)
                {
                    case 0: // Delete Building
                        result = MessageBox.Show($"Are you sure you want to delete building: {selectedNodeText}?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            Building building = Building_Manager.GetByBuildingName(selectedNodeText);
                            Building_Manager.Delete(building);
                            RemoveTreeViewItem(selectedNode);
                            IsDataChanged = true;
                        }
                        break;

                    case 1: // Delete Floor
                        result = MessageBox.Show($"Are you sure you want to delete floor: {selectedNodeText} of building: {GetParentHeader(selectedNode, 1)}?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            Building building = Building_Manager.GetByBuildingName(GetParentHeader(selectedNode, 1));
                            Floor_Manager.Delete(building, selectedNodeText);
                            RemoveTreeViewItem(selectedNode);
                            IsDataChanged = true;
                        }
                        break;

                    case 2: // Delete Room
                        result = MessageBox.Show($"Are you sure you want to delete room: {selectedNodeText} of floor: {GetParentHeader(selectedNode, 1)}, building: {GetParentHeader(selectedNode, 2)}?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            Building building = Building_Manager.GetByBuildingName(GetParentHeader(selectedNode, 2));
                            Floor floor = Floor_Manager.GetByFloorName(building, GetParentHeader(selectedNode, 1));
                            Room room = Room_Manager.GetByRoomName(floor, selectedNodeText);

                            Room_Manager.Delete(floor, room);
                            RemoveTreeViewItem(selectedNode);
                            IsDataChanged = true;
                        }
                        break;

                    case 3: // Delete Device
                        result = MessageBox.Show($"Are you sure you want to delete device: {selectedNodeText} of room: {GetParentHeader(selectedNode, 1)}, floor: {GetParentHeader(selectedNode, 2)}, building: {GetParentHeader(selectedNode, 3)}?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            Building building = Building_Manager.GetByBuildingName(GetParentHeader(selectedNode, 3));
                            Floor floor = Floor_Manager.GetByFloorName(building, GetParentHeader(selectedNode, 2));
                            Room room = Room_Manager.GetByRoomName(floor, GetParentHeader(selectedNode, 1));
                            Device device = Device_Manager.GetByDeviceName(room, selectedNodeText);

                            Device_Manager.Delete(room, device);
                            RemoveTreeViewItem(selectedNode);
                            IsDataChanged = true;
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void treeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (treeView1.SelectedItem == null) return;
                TreeViewItem selectedNode = treeView1.SelectedItem as TreeViewItem;
                if (selectedNode == null) return;

                int Level = GetTreeViewItemLevel(selectedNode);
                string selectedNodeText = selectedNode.Header.ToString();

                this.Selection();
                listView1.Items.Clear(); // Clear list at the beginning

                switch (Level)
                {
                    case 0: // Selected a building
                        break;

                    case 1: // Selected a floor
                        break;

                    case 2: // Selected a room
                        break;

                    case 3: // Selected a device
                        Building buildingCurrent = Building_Manager.GetByBuildingName(GetParentHeader(selectedNode, 3));
                        Floor floorCurrent = Floor_Manager.GetByFloorName(buildingCurrent, GetParentHeader(selectedNode, 2));
                        Room roomCurrent = Room_Manager.GetByRoomName(floorCurrent, GetParentHeader(selectedNode, 1));
                        Device deviceCurrent = Device_Manager.GetByDeviceName(roomCurrent, selectedNodeText);

                        foreach (Tag tag in deviceCurrent.Tags)
                        {
                            listView1.Items.Add(new Tag
                            {
                                TagName = tag.TagName,
                                Topic = tag.Topic,
                                QoS = tag.QoS,
                                Retain = tag.Retain,
                                Description = tag.Description
                            });
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Selection()
        {
            if (treeView1.SelectedItem == null) return;

            TreeViewItem selectedNode = treeView1.SelectedItem as TreeViewItem;
            if (selectedNode == null) return;

            int Level = GetTreeViewItemLevel(selectedNode);

            switch (Level)
            {
                case 0: // Selected a building
                    mn_Building.IsEnabled = btn_Building.IsEnabled = true;
                    mn_Floor.IsEnabled = btn_Floor.IsEnabled = true;
                    mn_Room.IsEnabled = btn_Room.IsEnabled = false;
                    mn_Device.IsEnabled = btn_Device.IsEnabled = false;
                    mn_Tag.IsEnabled = btn_Tag.IsEnabled = false;
                    break;

                case 1: // Selected a floor
                    mn_Building.IsEnabled = btn_Building.IsEnabled = false;
                    mn_Floor.IsEnabled = btn_Floor.IsEnabled = false;
                    mn_Room.IsEnabled = btn_Room.IsEnabled = true;
                    mn_Device.IsEnabled = btn_Device.IsEnabled = true;
                    mn_Tag.IsEnabled = btn_Tag.IsEnabled = false;
                    break;

                case 2: // Selected a room
                    mn_Building.IsEnabled = btn_Building.IsEnabled = false;
                    mn_Floor.IsEnabled = btn_Floor.IsEnabled = false;
                    mn_Room.IsEnabled = btn_Room.IsEnabled = false;
                    mn_Device.IsEnabled = btn_Device.IsEnabled = true;
                    mn_Tag.IsEnabled = btn_Tag.IsEnabled = false;
                    break;

                case 3: // Selected a device
                    mn_Building.IsEnabled = btn_Building.IsEnabled = false;
                    mn_Floor.IsEnabled = btn_Floor.IsEnabled = false;
                    mn_Room.IsEnabled = btn_Room.IsEnabled = false;
                    mn_Device.IsEnabled = btn_Device.IsEnabled = false;
                    mn_Tag.IsEnabled = btn_Tag.IsEnabled = true;
                    break;
            }
        }

        private void treeView1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Right)
                {
                    Selection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void treeView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton != MouseButton.Left) return;

                TreeViewItem selectedItem = treeView1.SelectedItem as TreeViewItem;
                if (selectedItem == null) return;

                int level = GetTreeViewItemLevel(selectedItem);
                string selectedNode = selectedItem.Header.ToString();
                Building buildingCurrent = null;
                Floor floorCurrent = null;
                Room roomCurrent = null;
                Device deviceCurrent = null;

                switch (level)
                {
                    case 0: // Select a building
                        buildingCurrent = Building_Manager.GetByBuildingName(selectedNode);
                        Window_Building buildingFrm = new Window_Building(buildingCurrent);
                        buildingFrm.eventBuildingChanged += (building) =>
                        {
                            selectedItem.Header = building.BuildingName;
                        };
                        buildingFrm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        buildingFrm.ShowDialog();
                        break;

                    case 1: // Select a floor
                        buildingCurrent = Building_Manager.GetByBuildingName(GetParentHeader(selectedItem, 1));
                        floorCurrent = Floor_Manager.GetByFloorName(buildingCurrent, selectedNode);
                        Window_Floor floorFrm = new Window_Floor(buildingCurrent, floorCurrent);
                        floorFrm.eventFloorChanged += (floor) =>
                        {
                            selectedItem.Header = floor.FloorName;
                        };
                        floorFrm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        floorFrm.ShowDialog();
                        break;

                    case 2: // Select a room
                        buildingCurrent = Building_Manager.GetByBuildingName(GetParentHeader(selectedItem, 2));
                        floorCurrent = Floor_Manager.GetByFloorName(buildingCurrent, GetParentHeader(selectedItem, 1));
                        roomCurrent = Room_Manager.GetByRoomName(floorCurrent, selectedNode);
                        Window_Room roomFrm = new Window_Room(buildingCurrent, floorCurrent, roomCurrent);
                        roomFrm.eventRoomChanged += (room) =>
                        {
                            selectedItem.Header = room.RoomName;
                        };
                        roomFrm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        roomFrm.ShowDialog();
                        break;

                    case 3: // Select a device
                        buildingCurrent = Building_Manager.GetByBuildingName(GetParentHeader(selectedItem, 3));
                        floorCurrent = Floor_Manager.GetByFloorName(buildingCurrent, GetParentHeader(selectedItem, 2));
                        roomCurrent = Room_Manager.GetByRoomName(floorCurrent, GetParentHeader(selectedItem, 1));
                        deviceCurrent = Device_Manager.GetByDeviceName(roomCurrent, selectedNode);
                        Window_Device deviceFrm = new Window_Device(buildingCurrent, floorCurrent, roomCurrent, deviceCurrent);
                        deviceFrm.eventDeviceChanged += (device) =>
                        {
                            selectedItem.Header = device.DeviceName;
                            listView1.Items.Clear();
                            foreach (Tag tag in device.Tags)
                            {
                                ListViewItem item = new ListViewItem
                                {
                                    Content = $"{tag.TagName}, {tag.Topic}, {tag.QoS}, {tag.Retain}, {tag.Description}"
                                };
                                listView1.Items.Add(item);
                            }
                        };
                        deviceFrm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        deviceFrm.ShowDialog();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                foreach (var selectedItem in listView1.SelectedItems)
                {
                    if (!(treeView1.SelectedItem is TreeViewItem selectedTreeNode)) return;
                    int Level = GetTreeViewItemLevel(selectedTreeNode);
                    if (Level != 3) return;

                    string selectedNode = selectedTreeNode.Header.ToString();
                    Building buildingCurrent = Building_Manager.GetByBuildingName(GetParentNodeText(selectedTreeNode, 3));
                    Floor floorCurrent = Floor_Manager.GetByFloorName(buildingCurrent, GetParentNodeText(selectedTreeNode, 2));
                    Room roomCurrent = Room_Manager.GetByRoomName(floorCurrent, GetParentNodeText(selectedTreeNode, 1));
                    Device deviceCurrent = Device_Manager.GetByDeviceName(roomCurrent, selectedNode);

                    if (selectedItem is Tag tagCurrent)
                    {
                        string tagName = tagCurrent.TagName;

                        Window_Tag tgFrm = new Window_Tag(buildingCurrent, floorCurrent, roomCurrent, deviceCurrent, tagCurrent);
                        tgFrm.eventTagChanged += new EventTagChanged((tag) =>
                        {
                            Tag_Manager.Update(deviceCurrent, tagCurrent);
                            IsDataChanged = true;
                        });

                        tgFrm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        tgFrm.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
