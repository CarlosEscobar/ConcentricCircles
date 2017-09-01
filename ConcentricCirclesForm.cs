using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Windows.Forms;

namespace AutoCadAddIns.ConcentricCircles
{
    /// <summary>
    /// Class defining Concentric Circles Form
    /// </summary>
    public partial class ConcentricCirclesForm : Form
    {
        
        /// <summary>
        /// Initialize Concentric Circles Form
        /// </summary>
        public ConcentricCirclesForm()
        {
            InitializeComponent();
            InitializeCircles();
        }

        /// <summary>
        /// Initialize numbers of circles combobox and circles list view
        /// </summary>
        private void InitializeCircles()
        {
            //Default value for concentric circle X coordinate
            xOriginTextBox.Text = "0.0";
            //Default value for concentric circle Y coordinate
            yOriginTextBox.Text = "0.0";
            //Default value for concentric circle radius factor
            radiusFactorTextBox.Text = "1.0";
            //Array with numbers from 1 to 20
            string[] numbers = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10",
                                 "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" };

            //Populate number of circles combo box
            foreach (string number in numbers)
            {
                numberOfCirclesComboBox.Items.Add(number);
            }
            //Default value for number of circles
            numberOfCirclesComboBox.SelectedIndex = 0;
            
            //Add circles list view for color picker
            colorCircleListView.View = View.Details;
            //Make list view multi select
            colorCircleListView.MultiSelect = true;
            colorCircleListView.HideSelection = false;
            //Show checkboxes on each item
            colorCircleListView.CheckBoxes = true;
            colorCircleListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            //Define Column for 'Circle'
            ColumnHeader circleColumnHeader = new ColumnHeader();
            circleColumnHeader.Text = "Circle";
            circleColumnHeader.TextAlign = HorizontalAlignment.Left;
            circleColumnHeader.Width = 100;
            //Define Column for 'Color'
            ColumnHeader colorColumnHeader = new ColumnHeader();
            colorColumnHeader.Text = "Color";
            colorColumnHeader.TextAlign = HorizontalAlignment.Center;
            colorColumnHeader.Width = 60;
            //Add columns to list view
            colorCircleListView.Columns.Add(circleColumnHeader);
            colorCircleListView.Columns.Add(colorColumnHeader);
            
            //Populate list view and add default color for circles
            foreach (string number in numbers)
            {
                ListViewItem listItem = new ListViewItem("Circle " + number);
                listItem.SubItems.Add(" ");
                listItem.UseItemStyleForSubItems = false;
                listItem.SubItems[1].BackColor = System.Drawing.Color.Black;
                colorCircleListView.Items.Add(listItem);
            }
        }

        /// <summary>
        /// Submit Concentric Circles Form
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void submitButton_Click(object sender, EventArgs e)
        {
            //Assign values from Concentric Circles Form
            ConcentricCircles.numberOfCircles = Convert.ToInt32(numberOfCirclesComboBox.SelectedIndex+1);
            ConcentricCircles.radiusFactor = Convert.ToDouble(radiusFactorTextBox.Text);
            ConcentricCircles.centerX = Convert.ToDouble(xOriginTextBox.Text);
            ConcentricCircles.centerY = Convert.ToDouble(yOriginTextBox.Text);

            //Get current autocad application document
            Document currentDocument = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            //Execute 'DrawConcentricCircles' command
            currentDocument.SendStringToExecute(".DrawConcentricCircles ", true, false, false);
        }

        /// <summary>
        /// Shows the color picker
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void colorButton_Click(object sender, EventArgs e)
        {
            //Define a color dialog
            ColorDialog colorPickerDialog = new ColorDialog();
            colorPickerDialog.AllowFullOpen = false;
            colorPickerDialog.ShowHelp = true;
            
            if (colorPickerDialog.ShowDialog() == DialogResult.OK)
            {
                //Update color picker button with latest color.
                colorButton.BackColor = colorPickerDialog.Color;
                
                foreach (ListViewItem item in colorCircleListView.CheckedItems)
                {
                    //Sets new color for selected circles on list view
                    ConcentricCircles.circlesColors[item.Index] = Convert.ToInt32(colorPickerDialog.Color.R) + "," + Convert.ToInt32(colorPickerDialog.Color.G) + "," + Convert.ToInt32(colorPickerDialog.Color.B);
                    //Updates list view color graphically
                    item.SubItems[1].BackColor = colorPickerDialog.Color;
                }
                //Refresh circles list view
                colorCircleListView.Refresh();
            }
        }
    }
}
