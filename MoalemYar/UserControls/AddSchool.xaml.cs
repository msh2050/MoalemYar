﻿/****************************** ghost1372.github.io ******************************\
*	Module Name:	AddSchool.xaml.cs
*	Project:		MoalemYar
*	Copyright (C) 2017 Mahdi Hosseini, All rights reserved.
*	This software may be modified and distributed under the terms of the MIT license.  See LICENSE file for details.
*
*	Written by Mahdi Hosseini <Mahdidvb72@gmail.com>,  2018, 3, 30, 10:38 ق.ظ
*
***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MoalemYar.UserControls
{
    /// <summary>
    /// Interaction logic for AddSchool.xaml
    /// </summary>
    public partial class AddSchool : UserControl
    {
        public Brush BorderColor { get; set; }
        internal static AddSchool main;
        private int runOnce = 0;
        private PersianCalendar pc = new PersianCalendar();
        private string strDate;
        private List<DataClass.Tables.School> _initialCollection;

        public AddSchool()
        {
            InitializeComponent();
            this.DataContext = this;
            main = this;
            var color = (Color)ColorConverter.ConvertFromString(AppVariable.ReadSetting(AppVariable.SkinCode));
            var brush = new SolidColorBrush(color);
            BorderColor = brush;
            GenerateEducateYear();
        }

        #region "Async Query"

        public async static Task<List<DataClass.Tables.School>> GetAllSchoolsAsync()
        {
            using (var db = new DataClass.myDbContext())
            {
                var query = db.Schools.Select(x => x);
                return await query.ToListAsync();
            }
        }

        public static async Task<string> DeleteSchoolAsync(long id)
        {
            using (var db = new DataClass.myDbContext())
            {
                var DeleteSchool = await db.Schools.FindAsync(id);
                db.Schools.Remove(DeleteSchool);
                await db.SaveChangesAsync();
                return "School Deleted Successfully";
            }
        }

        public async static Task<string> UpdateSchoolAsync(long id, string Name, string Base, string Admin, string Year)
        {
            using (var db = new DataClass.myDbContext())
            {
                var EditSchool = await db.Schools.FindAsync(id);
                EditSchool.SchoolName = Name;
                EditSchool.Base = Base;
                EditSchool.Admin = Admin;
                EditSchool.Year = Year;
                await db.SaveChangesAsync();
                return "School Updated Successfully";
            }
        }

        public async static Task<string> InsertSchoolAsync(string Name, string Base, string Admin, string Year)
        {
            using (var db = new DataClass.myDbContext())
            {
                var School = new DataClass.Tables.School();
                School.SchoolName = Name;
                School.Base = Base;
                School.Admin = Admin;
                School.Year = Year;
                db.Schools.Add(School);

                await db.SaveChangesAsync();

                return "School Added Successfully";
            }
        }

        #endregion "Async Query"

        #region Func get Query Wait"

        private void getSchool()
        {
            try
            {
                var query = GetAllSchoolsAsync();
                query.Wait();

                List<DataClass.Tables.School> data = query.Result;
                _initialCollection = query.Result;
                if (data.Any())
                {
                    dataGrid.ItemsSource = data;
                }
                else
                {
                    dataGrid.ItemsSource = null;
                    MainWindow.main.ShowNoDataNotification("School");
                }
            }
            catch (Exception)
            {
            }
        }
        private void deleteSchool(long id)
        {
            using (var db = new DataClass.myDbContext())
            {
                var checkQuery = db.Students.Where(x=>x.BaseId == id).Any();
                if (checkQuery)
                {
                    MainWindow.main.ShowDeleteExistNotification("مدرسه", "دانش آموزان");
                }
                else
                {
                    var query = DeleteSchoolAsync(id);
                    query.Wait();
                    MainWindow.main.getexHint();
                    MainWindow.main.ShowDeletedNotification(true, txtSchool.Text, "مدرسه");
                }
            }
        }

        private void updateSchool(long id, string Name, string Base, string Admin, string Year)
        {
            var query = UpdateSchoolAsync(id, Name, Base, Admin, Year);
            query.Wait();
        }

        private void addSchool(string Name, string Base, string Admin, string Year)
        {
            var query = InsertSchoolAsync(Name, Base, Admin, Year);
            query.Wait();
            MainWindow.main.getexHint();
        }

        #endregion Func get Query Wait"

        public T FindElementByName<T>(FrameworkElement element, string sChildName) where T : FrameworkElement
        {
            T childElement = null;
            var nChildCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < nChildCount; i++)
            {
                FrameworkElement child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;

                if (child == null)
                    continue;

                if (child is T && child.Name.Equals(sChildName))
                {
                    childElement = (T)child;
                    break;
                }

                childElement = FindElementByName<T>(child, sChildName);

                if (childElement != null)
                    break;
            }
            return childElement;
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj)
            where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.main.exContent.Content = null;
        }

        private void tabc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabc.SelectedIndex == 1)
            {
                if (runOnce == 0)
                {
                    getSchool();
                    runOnce = 1;
                }
            }
        }

        private void btnEditSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dynamic selectedItem = dataGrid.SelectedItems[0];
                long id = selectedItem.Id;
                updateSchool(id, txtSchool.Text, getComboValue(), txtAdmin.Text, txtYear.Text);
                MainWindow.main.ShowUpdateDataNotification(true, txtSchool.Text, "مدرسه");
                editGrid.IsEnabled = false;
                getSchool();
            }
            catch (Exception)
            {
                MainWindow.main.ShowUpdateDataNotification(false, txtSchool.Text, "مدرسه");
            }
        }

        private void btnEditCancel_Click(object sender, RoutedEventArgs e)
        {
            txtAdmin.Text = string.Empty;
            txtSchool.Text = string.Empty;
            txtYear.Text = string.Empty;
            setComboValue(null);
            editGrid.IsEnabled = false;
        }

        private string getComboValue()
        {
            var element = FindElementByName<ComboBox>(cmbContent, "cmbBase");
            return element.Text;
        }

        private void setComboValue(string index)
        {
            var element = FindElementByName<ComboBox>(cmbContent, "cmbBase");
            switch (index)
            {
                case "اول":
                    element.SelectedIndex = 0;
                    break;

                case "دوم":
                    element.SelectedIndex = 1;
                    break;

                case "سوم":
                    element.SelectedIndex = 2;
                    break;

                case "چهارم":
                    element.SelectedIndex = 3;
                    break;

                case "پنجم":
                    element.SelectedIndex = 4;
                    break;

                case "ششم":
                    element.SelectedIndex = 5;
                    break;

                case null:
                    element.SelectedIndex = -1;
                    break;
            }
        }

        private void txtEditSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(dataGrid.ItemsSource != null)
            {
                if (txtEditSearch.Text != string.Empty)
                    dataGrid.ItemsSource = _initialCollection.Where(x => x.SchoolName.Contains(txtEditSearch.Text) || x.Admin.Contains(txtEditSearch.Text) || x.Base.Contains(txtEditSearch.Text) || x.Year.Contains(txtEditSearch.Text)).Select(x => x);
                else
                    dataGrid.ItemsSource = _initialCollection.Select(x => x);
            }
        }

        private void GenerateEducateYear()
        {
            strDate = pc.GetYear(DateTime.Now).ToString("0000");
            string Year = strDate.Substring(2, 2);
            int NextYear = Convert.ToInt32(Year) + 1;
            txtAddYear.Text = Year + "-" + NextYear;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var element = FindElementByName<ComboBox>(cmbAddContent, "cmbBase");
            if (txtAddSchool.Text == string.Empty || txtAddAdmin.Text == string.Empty || txtAddYear.Text == string.Empty || element.SelectedIndex == -1)
            {
                MainWindow.main.ShowFillAllDataNotification();
            }
            else
            {
                try
                {
                    addSchool(txtAddSchool.Text, element.Text, txtAddAdmin.Text, txtAddYear.Text);
                    MainWindow.main.ShowAddDataNotification(true, txtAddSchool.Text, "مدرسه");
                    txtAddAdmin.Text = string.Empty;
                    txtAddSchool.Text = string.Empty;
                    txtAddSchool.Focus();
                }
                catch (Exception)
                {
                    MainWindow.main.ShowAddDataNotification(false, txtAddSchool.Text, "مدرسه");
                }
            }
        }

        private void txtEditSearch_ButtonClick(object sender, EventArgs e)
        {
            getSchool();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.main.ShowDeleteConfirmNotification(txtSchool.Text, "مدرسه");
        }

        public void deleteSchool()
        {
            try
            {
                dynamic selectedItem = dataGrid.SelectedItems[0];
                long id = selectedItem.Id;
                deleteSchool(id);
                editGrid.IsEnabled = false;
                getSchool();
            }
            catch (Exception)
            {
                MainWindow.main.ShowDeletedNotification(false, txtSchool.Text, "مدرسه");
            }
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                dynamic selectedItem = dataGrid.SelectedItems[0];
                txtAdmin.Text = selectedItem.Admin;
                txtSchool.Text = selectedItem.SchoolName;
                txtYear.Text = selectedItem.Year;
                setComboValue(selectedItem.Base);
                editGrid.IsEnabled = true;
            }
            catch (Exception)
            {
            }
        }
    }
}